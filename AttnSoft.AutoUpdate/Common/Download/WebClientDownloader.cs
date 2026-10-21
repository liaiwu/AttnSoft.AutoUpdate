
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#if NETFRAMEWORK

namespace AttnSoft.AutoUpdate.Common
{
    /// <summary>
    /// 文件下载组件
    /// </summary>
    public class FileDownloader
    {
        const int BufferSize = 64 * 1024;

        /// <summary>
        /// 下载进度变化事件,参数1:文件总大小,参数2:下载进度
        /// </summary>
        public event Action<long, int>? OnProgressChanged;
        public event Action<Exception>? OnDownloadException;
        public event Action? OnDownloadCompleted;

        /// <summary>
        /// 连续多少毫秒收不到数据即判为停滞并中止；0 = 不启用。默认 30000。
        /// </summary>
        public int DownloadStallTimeoutMs { get; set; } = 30_000;

        public async Task DownloadFileAsync(string url, string filePath, CancellationToken ct = default)
        {
            int stallMs = DownloadStallTimeoutMs;
            int stalled = 0;
            try
            {
                if (IsHttps(url))
                {
                    UpdateLog.Warning("下载升级包已禁用服务器证书校验（与取版本信息通道口径一致），HTTPS 下升级包可被中间人替换。");
                }

                // 只在**实例级**关掉服务器证书校验：不校验的口径要和取版本信息通道一致，
                // 但绝不能用 ServicePointManager.ServerCertificateValidationCallback —— 那是进程级全局，
                // 会污染宿主进程里其他组件的 TLS 信任判断。
                // 同理不设 TLS 协议版本：net461 上根本没有实例级开关（HttpWebRequest / ServicePoint 都没有
                // SecurityProtocol 属性），而宿主 target ≥4.6 时默认已含 TLS 1.2。
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ServerCertificateValidationCallback = delegate { return true; };
                using (ct.Register(() => { try { request.Abort(); } catch { } }))
                {
                    // net461 上 BCL 自带的超时在异步路径上全部无效：Timeout 管不了 GetResponseAsync，
                    // ReadWriteTimeout 只管同步读；CancellationToken 也打不断已经挂住的等。
                    // 唯一能打断挂起请求的是 request.Abort()，所以每个等待自己挂一个计时去触发它。
                    // 实测见 docs/active/RESEARCH-download-stall-timeout.md 第 10 节。
                    WebResponse response;
                    using (CancellationTokenSource headCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                    {
                        if (stallMs > 0)
                        {
                            headCts.CancelAfter(stallMs);
                        }
                        using (headCts.Token.Register(() =>
                        {
                            Interlocked.Exchange(ref stalled, 1);
                            try { request.Abort(); } catch { }
                        }))
                        {
                            response = await request.GetResponseAsync().ConfigureAwait(false);
                        }
                    }
                    using (response)
                    using (Stream contentStream = response.GetResponseStream())
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true))
                    {
                        long totalBytes = response.ContentLength;
                        if (totalBytes == -1)
                        {
                            throw new InvalidDataException("Can't get the file size.");
                        }
                        long totalBytesRead = 0;
                        byte[] buffer = new byte[BufferSize];
                        int preProgress = 0;
                        // 每次等待新建一个 linked CTS 而不是复用 + Reset：CTS 不能 Reset，
                        // 而复用同一个配 Timer 会在「数据刚好到达」与「计时器已入队」之间互踩。
                        while (true)
                        {
                            int bytesRead;
                            if (stallMs > 0)
                            {
                                using (CancellationTokenSource readCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                                {
                                    readCts.CancelAfter(stallMs);
                                    using (readCts.Token.Register(() =>
                                    {
                                        Interlocked.Exchange(ref stalled, 1);
                                        try { request.Abort(); } catch { }
                                    }))
                                    {
                                        bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                                    }
                                }
                            }
                            else
                            {
                                bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                            }
                            if (bytesRead <= 0) break;

                            await fileStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                            totalBytesRead += bytesRead;
                            int progress = (int)((totalBytesRead * 100) / totalBytes);
                            if (progress > preProgress)
                            {
                                preProgress = progress;
                                try
                                {
                                    OnProgressChanged?.Invoke(totalBytes, progress);
                                }
                                catch (Exception ex)
                                {
                                    // 消费者回调失败不是下载失败：包已完整落盘，不能让它落进下面的 catch
                                    // 被当成下载出错，从而删掉完整包并中止升级
                                    UpdateLog.Warning("下载进度回调异常", ex);
                                }
                            }
                        }
                    }
                }
                try
                {
                    OnDownloadCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    UpdateLog.Warning("下载完成回调异常", ex);
                }
            }
            catch (Exception ex)
            {
                // 停滞计时触发的取消不是消费者主动取消，换成 TimeoutException 才看得出是停滞而非人为中止。
                if (stalled == 1 && !ct.IsCancellationRequested)
                {
                    Exception failure = new TimeoutException($"下载停滞：连续 {stallMs} 毫秒未收到数据，已中止。", ex);
                    NotifyDownloadException(failure);
                    TryDeletePartial(filePath);
                    throw failure;
                }
                // 与 net6+ 通道对齐：那边取消时抛 OperationCanceledException，这边 Abort() 抛出来的是
                // WebException(RequestCanceled)。ct 都要求取消了，出来什么都不认——统一换算成
                // OperationCanceledException，否则跨目标框架写 catch (OperationCanceledException)
                // 的消费者在 net461 上时有时无。
                if (ct.IsCancellationRequested)
                {
                    Exception cancelled = new OperationCanceledException("下载已取消。", ex, ct);
                    NotifyDownloadException(cancelled);
                    TryDeletePartial(filePath);
                    throw cancelled;
                }
                NotifyDownloadException(ex);
                TryDeletePartial(filePath);
                throw;
            }
        }

        /// <summary>
        /// 取小体量响应体（版本信息 JSON 等）。不触发任何消费者事件——
        /// <see cref="OnDownloadProgressChanged"/> 一类事件是给升级包准备的，宿主拿它驱动进度条。
        /// </summary>
        public Task<byte[]> GetDataAsync(string url, CancellationToken ct = default)
            => SendForDataAsync(CreateRequest(url), null, ct);

        /// <summary>
        /// POST 一段请求体并取回响应体，用于 WebApi 方式取版本信息。
        /// </summary>
        public Task<byte[]> PostDataAsync(string url, byte[] body, string contentType,
            string? acceptHeader = null, CancellationToken ct = default)
        {
            HttpWebRequest request = CreateRequest(url);
            request.Method = "POST";
            request.ContentLength = body.LongLength;
            request.ContentType = contentType;
            if (!string.IsNullOrEmpty(acceptHeader))
                request.Accept = acceptHeader;
            return SendForDataAsync(request, body, ct);
        }

        private async Task<byte[]> SendForDataAsync(HttpWebRequest request, byte[]? body, CancellationToken ct)
        {
            int timeoutMs = DownloadStallTimeoutMs;
            int timedOut = 0;
            try
            {
                using (ct.Register(() => { try { request.Abort(); } catch { } }))
                // 一次性总时限，不像 DownloadFileAsync 那样每次等待挂停滞计时：POST 挂死多半发生在
                // 「请求已发出、服务端不回响应」，那时请求体早发完了，盯每次读只会让计时到点触发，
                // 绕一圈还是总时限。响应只有几 KB，两种口径观测等价，
                // 而 net6+ 侧用的正是 HttpClient.Timeout（总时限），这里对齐。
                using (CancellationTokenSource deadlineCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    if (timeoutMs > 0)
                    {
                        deadlineCts.CancelAfter(timeoutMs);
                        using (deadlineCts.Token.Register(() =>
                        {
                            Interlocked.Exchange(ref timedOut, 1);
                            try { request.Abort(); } catch { }
                        }))
                        {
                            return await SendAndReadAsync(request, body, ct).ConfigureAwait(false);
                        }
                    }
                    return await SendAndReadAsync(request, body, ct).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // 与下载通道同一口径：超时与人为取消要分得开，跨目标框架的异常类型也要归一
                if (timedOut == 1 && !ct.IsCancellationRequested)
                {
                    throw new TimeoutException($"取数据超时：{timeoutMs} 毫秒内未完成。", ex);
                }
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException("取数据已取消。", ex, ct);
                }
                throw;
            }
        }

        static async Task<byte[]> SendAndReadAsync(HttpWebRequest request, byte[]? body, CancellationToken ct)
        {
            if (body != null)
            {
                using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await requestStream.WriteAsync(body, 0, body.Length, ct).ConfigureAwait(false);
                }
            }
            using (WebResponse response = await request.GetResponseAsync().ConfigureAwait(false))
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream buffer = new MemoryStream())
            {
                byte[] chunk = new byte[8192];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(chunk, 0, chunk.Length, ct).ConfigureAwait(false)) > 0)
                {
                    buffer.Write(chunk, 0, bytesRead);
                }
                return buffer.ToArray();
            }
        }

        // 消费者在这个回调里抛异常，既不能盖掉真正的下载异常，也不能跳过后面的清理与 rethrow
        private void NotifyDownloadException(Exception failure)
        {
            try
            {
                OnDownloadException?.Invoke(failure);
            }
            catch (Exception ex)
            {
                UpdateLog.Warning("下载异常回调异常", ex);
            }
        }

        static HttpWebRequest CreateRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // 证书校验口径与 DownloadFileAsync 一致，理由见那里的注释
            request.ServerCertificateValidationCallback = delegate { return true; };
            return request;
        }

        private static bool IsHttps(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }

        private static void TryDeletePartial(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
            }
            catch
            {
                // 清理失败不能盖掉真正的下载异常
            }
        }
    }
}

#endif
