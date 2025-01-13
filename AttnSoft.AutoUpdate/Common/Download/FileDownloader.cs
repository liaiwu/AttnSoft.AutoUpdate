using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Common
{
    /// <summary>
    /// 文件下载组件
    /// </summary>
    public class FileDownloader
    {
        //public event Action? OnDownloadCanceled;
        /// <summary>
        /// 下载进度变化事件,参数1:文件总大小,参数2:下载进度
        /// </summary>
        public event Action<long, int>? OnProgressChanged;
        public event Action<Exception>? OnDownloadException;
        public event Action? OnDownloadCompleted;

        public async Task DownloadFileAsync(string url, string filePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        if (totalBytes == -1)
                        {
                            OnDownloadException?.Invoke(new Exception("Can't get the file size."));
                            return;
                        }
                        long totalBytesRead = 0;
                        byte[] buffer = new byte[10240];
                        int preProgress = 0;
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 10240, true))
                        {
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                int progress = (int)((totalBytesRead * 100) / totalBytes);
                                if (progress>preProgress)
                                {
                                    preProgress= progress;
                                    OnProgressChanged?.Invoke(totalBytes, progress);
                                }
                            }
                        }
                    }
                }
                OnDownloadCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                OnDownloadException?.Invoke(ex);
            }
        }

        //public async Task DownloadFileAsync(string url, string filePath, CancellationToken cancellation)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation))
        //            {
        //                response.EnsureSuccessStatusCode();
        //                long totalBytes = response.Content.Headers.ContentLength ?? -1;
        //                if (totalBytes == -1)
        //                {
        //                    OnDownloadException?.Invoke(new Exception("Can't get the file size."));
        //                    return;
        //                }
        //                long totalBytesRead = 0;
        //                byte[] buffer = new byte[10240];

        //                using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 10240, true))
        //                {
        //                    int bytesRead;
        //                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellation)) > 0)
        //                    {
        //                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellation);
        //                        totalBytesRead += bytesRead;
        //                        int progress = (int)((totalBytesRead * 100) / totalBytes);
        //                        OnProgressChanged?.Invoke(totalBytes, progress);
        //                    }
        //                }
        //            }
        //        }
        //        OnDownloadCompleted?.Invoke();
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        OnDownloadCanceled?.Invoke();
        //    }
        //    catch (Exception ex)
        //    {
        //        OnDownloadException?.Invoke(ex);
        //    }
        //}

    }
}
