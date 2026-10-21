using System;
using System.Collections.Generic;
using System.IO;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if !NETFRAMEWORK
using System.Net.Http;

namespace AttnSoft.AutoUpdate.Common
{
    /// <summary>
    /// 文件下载组件
    /// </summary>
    public class FileDownloader
    {
        private const int BufferSize = 64 * 1024;

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
            try
            {
                if (IsHttps(url))
                {
                    UpdateLog.Warning("下载升级包已禁用服务器证书校验（与取版本信息通道口径一致），HTTPS 下升级包可被中间人替换。");
                }

                // 不校验服务器证书，与取版本信息通道（UpdateContextExtensions）口径一致：
                // 能中间人下载通道的人同样能改版本 JSON，连带换成恶意包的合法 Hash，
                // 加校验挡不住任何人，只会挡住「自签证书的 https 服务器」这种合法用法。
                // handler 交给 HttpClient 托管，不单独 Dispose。
                using (HttpClient client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                }))
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        if (totalBytes == -1)
                        {
                            throw new InvalidDataException("Can't get the file size.");
                        }
                        long totalBytesRead = 0;
                        byte[] buffer = new byte[BufferSize];
                        int preProgress = 0;
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false), fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true))
                        {
                            // ResponseHeadersRead 下 HttpClient.Timeout 只计到响应头为止，响应体停滞它一律不管：
                            // 对端不关连接就永久挂着。只能每次读自己挂一个停滞计时。
                            // 每次读新建一个 linked CTS 而不是复用 + Reset：CTS 不能 Reset，
                            // 而复用同一个配 Timer 会在「数据刚好到达」与「计时器已入队」之间互踩。
                            int bytesRead;
                            while (true)
                            {
                                if (stallMs > 0)
                                {
                                    using CancellationTokenSource readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                                    readCts.CancelAfter(stallMs);
                                    bytesRead = await contentStream.ReadAsync(buffer.AsMemory(), readCts.Token).ConfigureAwait(false);
                                }
                                else
                                {
                                    bytesRead = await contentStream.ReadAsync(buffer.AsMemory(), ct).ConfigureAwait(false);
                                }
                                if (bytesRead <= 0) break;

                                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
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
                if (stallMs > 0 && ex is OperationCanceledException && !ct.IsCancellationRequested)
                {
                    Exception stalled = new TimeoutException($"下载停滞：连续 {stallMs} 毫秒未收到数据，已中止。", ex);
                    NotifyDownloadException(stalled);
                    TryDeletePartial(filePath);
                    throw stalled;
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
            => SendForDataAsync(new HttpRequestMessage(HttpMethod.Get, url), ct);

        /// <summary>
        /// POST 一段请求体并取回响应体，用于 WebApi 方式取版本信息。
        /// </summary>
        public Task<byte[]> PostDataAsync(string url, byte[] body, string contentType,
            string? acceptHeader = null, CancellationToken ct = default)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new ByteArrayContent(body)
            };
            request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            if (!string.IsNullOrEmpty(acceptHeader))
                request.Headers.TryAddWithoutValidation("Accept", acceptHeader);
            return SendForDataAsync(request, ct);
        }

        private async Task<byte[]> SendForDataAsync(HttpRequestMessage request, CancellationToken ct)
        {
            int timeoutMs = DownloadStallTimeoutMs;
            try
            {
                // 不校验服务器证书，与下载通道口径一致（理由见 DownloadFileAsync 里的注释）。
                // handler 交给 HttpClient 托管，不单独 Dispose。
                using (HttpClient client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                })
                {
                    // 这里用总时限而非每读挂计时：响应体只有几 KB，两种口径观测上等价，
                    // 而总时限是 HttpClient 原生能力。前提是不能用 ResponseHeadersRead——
                    // 那样 Timeout 只计到响应头，实测设 3s 仍会等满 20s。
                    Timeout = timeoutMs > 0 ? TimeSpan.FromMilliseconds(timeoutMs) : Timeout.InfiniteTimeSpan
                })
                {
                    using (HttpResponseMessage response = await client.SendAsync(request, ct).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // 与下载通道同一口径：超时触发的取消不是消费者主动取消，换成 TimeoutException 才看得出差别
                if (timeoutMs > 0 && ex is OperationCanceledException && !ct.IsCancellationRequested)
                {
                    throw new TimeoutException($"取数据超时：{timeoutMs} 毫秒内未完成。", ex);
                }
                throw;
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