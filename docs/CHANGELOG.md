# 更新日志

## v4.0.0 (2026-10-21)
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
- **改造** **文件比较改用相对路径作身份键**。原先 `StorageManager.Compare` 把两侧文件塞进二叉排序树 `FileTree`，再按**树中位置**锁步比对（`Compare(node.Left, node0.Left)`），而 `FileNode.Equals` 只看 `Hash` + `Name`、不看路径。受控实验实测：中间插入 1 个文件就报 **1506** 个差异（期望 6），假阳性 99.5%——文件数稍多时差分功能实际不工作。另一条：`Id` 取自枚举序号，顺序键插入 BST 必然退化成右链，递归深度 = 文件数，实测崩溃阈值 **9000 < N ≤ 10000**（默认 1 MB 栈），而 `StackOverflowException` 不可捕获，应用直接死亡。现在用 `Dictionary<string, FileNode>`（键 = `RelativePath`，`StringComparer.Ordinal`）逐个配对，配不上或 `Equals` 不等即判为差异 — **破坏性变更**：删除 `FileTree`，`FileNode` 去掉 `Left` / `Right` / `LeftType` / `RightType` / `Add` / `InfixOrder` / `Search` / `SearchParent`（已确认除自引用外无调用者）
- **修复** **解压升级包时的路径遍历（zip slip）**。恶意升级包里带 `..\..\` 的条目原先会被写到安装目录之外。现在每条条目先 `Path.GetFullPath` 规范化，再断言结果仍在目标根目录之下，越界抛 `InvalidDataException` 且原样上抛（不与「包损坏」混在一起），消费者可单独识别。顺带修掉一处死代码：原先去前导分隔符的正则求值成 `^\*`，在正则里是「行首的字面星号」，从未去掉过任何东西——旧代码靠字符串拼接吸收多余的 `/` 才没出问题
- **修复** **32 位宿主里 `ProcessHelper` 一个进程都杀不掉**。`KillByNameInDir` / `KillAllInDir` 原先用 `Process.MainModule.FileName` 判断进程是否在安装目录下，而 32 位进程读 64 位进程的 `MainModule` 必抛 `Win32Exception(299 ERROR_PARTIAL_COPY)`（实测 0/187 全失败，`Modules` 走同一条通道一起废）。后果：升级前「关掉应用目录下所有进程」在 x86 宿主里形同虚设，被占用的文件必然导致安装失败。现在改调 `QueryFullProcessImageNameW`，跨位数可用（实测 32→64、64→32、64→64 均成功），不必判断位数。同时按 Win32 错误码分开记「读不到路径」：87（进程在枚举与取路径之间已退出）记 Debug，5（没权限，这才是「该杀的没杀掉」）记 Warning；`WaitForExit(5000)` 超时也记 Warning，提示它占用的文件仍会导致安装失败
- **改进** 升级器的文件操作带锁重试。进程刚被结束、杀软或索引服务还攥着句柄时，删除与复制会抛 sharing/lock violation（`ERROR_SHARING_VIOLATION(32)` / `ERROR_LOCK_VIOLATION(33)`），这类等一下就好，原先直接中断整次安装。现在 `FileBusyRetry` 覆盖差异应用的删除与复制（`DifferentialCore.cs:147`、`:194`）和 `Version.json` 的删除与写入（`UpgradApp.cs:46-47`）：每个文件最多 5 次、间隔 1 秒，撞锁 5 次才放弃并记 Warning。其余 `IOException`（磁盘满、路径不存在、权限不足）重试不会变好，直接抛出。实测 5 个文件全锁死时总耗时 4 秒、只有第一个被尝试——`Run` 一抛就中断调用方的 `foreach`，等待不按文件数累加
- **修复** **升级器安装失败后主程序再也起不来**。`Upgrade.exe` 原先只在安装成功的路径上启动主程序，`StartInstall()` 一抛就只留一行 `Install Error` 然后进程结束；而宿主启动升级器后立刻 `Environment.Exit(0)`，升级器是唯一能把主程序拉回来的进程——安装失败一次，应用就得等用户手动开。现在 catch 里记 `Critical` 之后仍调 `StartApp(false)`：**不带** `AttnSoft.AutoUpdate.Successful` 标记（否则宿主的 `CheckCompletionMiddleware` 会误报 `OnUpdateCompleted`），也**不清备份**（留给回滚）。救不了的一种：`new UpgradApp()` 自身失败时 `StartAppCmd` 只存在于 `newVersionInfo.json` 里，此时不知道该启动谁
- **修复** 升级器的成功日志永远落不了盘。结束自身的 `Process.Kill()` 原先写在 `StartApp(true)` 末尾、排在 `Installation is complete.` 之前，那行日志跟着进程一起没——日志里「装成功了」和「装到一半崩了」就只剩 Critical 这一条在不在的区别。现在改成 `UpgradApp.KillSelf()`，由 `Program.cs` 在成功日志**之后**调用
- **改进** 升级器清理备份目录失败不再否定一次已成功的升级。`StorageManager.DeleteDirectory` 不吞异常，原先清备份一抛就冒到 `Program.Main` 的 catch，把刚起来的主程序再起一遍。现在兜住并记 Warning，不影响本次升级结果
- **注意** 升级器从来不是提权启动的。`StartUpgradMiddleware` 里 `#if WINDOWS` 包着的 `Verb = "runas"` + `WindowStyle = Hidden`，在 net461 / net6.0 / net8.0 / net10.0 四个目标上都不成立（没有一带 `-windows` 后缀，`TargetPlatformIdentifier` 全为空），从未编译进任何发布过的包。v4.0.0 删除这条死分支，**零行为变更**；确实需要管理员权限时，正确形态是宿主自己以管理员运行、子进程继承权限
- **新增** 目标框架 `net8.0`，包现为 net461 / net6.0 / net8.0 / net10.0
- **新增** 控制台示例 `Examples/ConsoleUpdate`（.NET 10），演示 `UpdateContext` 加事件订阅的最小用法；示例项目统一归到 `Examples/` 下
- **移除** `LoggerFactory`（含 `Initialize` / `Restore` / `LogPath`）与 `TimestampWriter` — **破坏性变更**，迁移方式见 [LOGGING.md](LOGGING.md)

## v3.5.0 (2026-06-24)
- **新增** `ProcessHelper` 进程工具类，统一进程扫描和关闭逻辑
- **改进** 升级器安装前自动扫描并关闭应用目录下所有进程，支持多进程应用升级
- **改进** 主程序启动升级器前自动清理残留升级器进程，双重校验（进程名 + 路径）防止误杀
- **重构** `UpgradMiddleware` 移除临时文件写入逻辑，恢复直接写入目标文件
- **移除** 废弃的 `--mainPid` 命令行参数
