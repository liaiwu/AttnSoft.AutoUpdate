
using System;
using System.Net;
using System.Threading.Tasks;

#if NETFRAMEWORK

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
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        OnProgressChanged?.Invoke(e.TotalBytesToReceive, e.ProgressPercentage);
                    };
                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            OnDownloadException?.Invoke(e.Error);
                        }
                        else if (e.Cancelled)
                        {
                            OnDownloadException?.Invoke(new OperationCanceledException("Download was canceled."));
                        }
                        else
                        {
                            OnDownloadCompleted?.Invoke();
                        }
                    };
                    await client.DownloadFileTaskAsync(new Uri(url), filePath);
                }
            }
            catch (Exception ex)
            {
                OnDownloadException?.Invoke(ex);
            }
        }

    }
}

#endif