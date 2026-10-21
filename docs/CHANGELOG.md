# 更新日志

## v4.0.0 (2026-10-17)
- **新增** 日志门面 `UpdateLog`，级别与输出接口直接采用 Microsoft.Extensions.Logging 的 `LogLevel` / `ILogger`（Trace/Debug/Information/Warning/Error/Critical/None）；net461 由组件内置同名同命名空间的兼容定义，全框架写法一致
- **改进** Release 构建同样有日志（此前日志被 `#if DEBUG` 包裹，发布版一行日志都没有）
- **改进** 默认输出到 `System.Diagnostics.Trace`，不再重定向宿主的 `Console.Out`，不再向安装目录写日志文件
- **新增** `context.UseMicrosoftLogging(ILoggerFactory)`，一行接入宿主的日志框架（.NET 6 及以上）；同时把 `MinimumLevel` 放宽到 `Trace`，级别交由宿主的日志配置决定
- **新增** `FileUpdateLogger`；升级执行器 `Upgrade.exe` 写 `{安装目录}\upgrade_logs\upgrade.log`（安装目录不可写时退到临时目录并记下原因），Release 亦有日志（此前 Release 完全无日志，Debug 才有文件）
- **新增** `UpdateLog.Category` 标明日志来源：默认 Trace sink 与 `FileUpdateLogger` 加 `[Category]` 前缀，`UseMicrosoftLogging` 下作为 MEL 的 category 供宿主过滤路由；`Upgrade.exe` 自带 `[Upgrade]`
- **修复** `upgrade_logs` 加入默认跳过目录，不再被差分比较当成应用文件
- **修复** **https 下载不到升级包**。两条下载通道此前校验服务器证书，而取版本信息通道早已完全不校验——这个不对称导致自签证书的 https 服务器「取版本信息成功、下载失败」（`UntrustedRoot`），升级卡死在下载步。现在四条通道口径一致，都不校验服务器证书：自签、已过期、域名不匹配的证书都能正常下载
- **新增** `UpdateContext.DownloadStallTimeoutMs`（默认 30000 毫秒，`0` = 关闭）。此前下载中途停滞会**永久挂死**：`HttpClient.Timeout` 在 `ResponseHeadersRead` 下不覆盖响应体读取，对端既不续传也不关连接时下载不报错、不超时、主程序不启动
- **改进** 取版本信息改走与下载升级包同一套通道（`FileDownloader`），因此同样受 `DownloadStallTimeoutMs` 约束。**此前 net461 取版本信息完全没有超时**——服务端连上但不回数据就永久挂住；net6+ 用的是 `HttpClient` 默认的 100 秒
- **修复** net6+ 取版本信息失败时直接抛出，宿主拿不到 null，「第一个 URL 取不到就走备用 URL」的两级兜底在 .NET 6+ 上形同虚设。现在两条通道失败一律返回 null
- **修复** net461 取版本信息失败时 `OnUpdateException` 触发**两次**（catch 里一次、`CheckUpdateMiddleware` 的 null 分支再一次） — **破坏性变更**：现在恰好一次，只在 null 分支触发
- **新增** `FileDownloader.DownloadFileAsync` 的可选 `CancellationToken` 参数（源码兼容，不破坏现有调用）
- **修复** net461 下载失败时 `OnDownloadException` 触发**两次**（`DownloadFileCompleted` 与 catch 各一次），消费者弹两遍错误框 — **破坏性变更**：现在恰好一次，`e.Cancelled` 分支的异常改由 catch 发出
- **改造** **库全程异步，不再阻塞宿主线程**。核心库所有 `await` 加 `ConfigureAwait(false)`，`StartUpdateAsync` 用 `Task.Run` 把整条更新管道交棒线程池。此前在 WinForms/WPF 宿主里，下载完成后的哈希校验与解压会回到 UI 线程执行、界面假死；现在这些都在线程池上跑 — **破坏性变更**：`OnFindNewVersion` / `OnBeforeInstall` / `OnUpdateCompleted` / `OnDownloadProgressChanged` / `OnDownloadCompleted` / `OnDownloadException` / `OnUpdateException` 今后全部在**线程池线程**触发，不再回到宿主 UI 线程，消费者必须在回调里自行用 `InvokeRequired` / `Invoke` 归位。消费者自己的 `await StartUpdateAsync()` 之后仍回到其原上下文，用法不变
- **注意** `OnFindNewVersion` **不能**用 `InvokeRequired` 守卫：它常在窗体还没 `Show` 过时被调用，此时句柄未创建、`InvokeRequired` 返回 **false**，守卫形同虚设（实测操作照旧跑在线程池上且不抛异常，窗体句柄因此建在池线程上，后面几个事件的守卫也跟着归错位）。要用 `TaskScheduler.FromCurrentSynchronizationContext()` 把整个调用归位——随包 WinForms 示例已改成这种写法
- **移除** `LoggerFactory`（含 `Initialize` / `Restore` / `LogPath`）与 `TimestampWriter` — **破坏性变更**，迁移方式见 [LOGGING.md](LOGGING.md)

## v3.5.0 (2026-06-24)
- **新增** `ProcessHelper` 进程工具类，统一进程扫描和关闭逻辑
- **改进** 升级器安装前自动扫描并关闭应用目录下所有进程，支持多进程应用升级
- **改进** 主程序启动升级器前自动清理残留升级器进程，双重校验（进程名 + 路径）防止误杀
- **重构** `UpgradMiddleware` 移除临时文件写入逻辑，恢复直接写入目标文件
- **移除** 废弃的 `--mainPid` 命令行参数
