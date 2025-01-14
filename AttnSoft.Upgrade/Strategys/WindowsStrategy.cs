using GeneralUpdate.Differential;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UpgradExpress.Objects;

namespace Upgrad.Strategys
{
    /// <summary>
    /// Update policy based on the Windows platform.
    /// </summary>
    public class WindowsStrategy : AbstractStrategy
    {
        private UpgradContext context;

        public override void Create(UpgradContext parameter) => context = parameter;

        public override async Task Execute()
        {
            try
            {
                //var status = ReportType.None;
                //var patchPath = StorageManager.GetTempDirectory(Patchs);
                var patchPath = context.PatchPath;

                var version = context.UpdateVersion;
                try
                {

                    var sourcePath = context.AppPath;
                    var targetPath = context.PatchPath;
                    var backupDirectory = context.BackupPath;
                    await DifferentialCore.Instance?.Dirty(sourcePath, targetPath, backupDirectory);

                    //var context = new PipelineContext();
                    ////Patch middleware
                    //context.Add("BackupDirectory", _configinfo.BackupDirectory);
                    //context.Add("SourcePath", _configinfo.InstallPath);
                    //context.Add("PatchPath", patchPath);
                    ////Driver middleware
                    //if (_configinfo.DriveEnabled == true)
                    //{
                    //    context.Add("DriverOutPut", StorageManager.GetTempDirectory("DriverOutPut"));
                    //    context.Add("FieldMappings", _configinfo.FieldMappings);
                    //}

                    //if (_configinfo.DriveEnabled is null or false)
                    //{
                    //    var driver = new DriverMiddleware();
                    //    await driver.InvokeAsync(context);
                    //}
                    //var patcher=new PatchMiddleware();
                    //await patcher.InvokeAsync(context);

                    //var pipelineBuilder = new PipelineBuilder(context)
                    //    .UseMiddleware<PatchMiddleware>()
                    //    //.UseMiddleware<CompressMiddleware>()
                    //    //.UseMiddleware<HashMiddleware>()
                    //    .UseMiddlewareIf<DriverMiddleware>(_configinfo.DriveEnabled);
                    //await pipelineBuilder.Build();

                    //status = ReportType.Success;
                    //if (!string.IsNullOrEmpty(_configinfo.ReportUrl))
                    //{
                    //    await VersionService.Report(_configinfo.ReportUrl, version.RecordId, status
                    //        , _configinfo.Scheme, _configinfo.Token);
                    //}
                }
                catch (Exception e)
                {
                    //status = ReportType.Failure;
                    Console.WriteLine("出错了:"+e.ToString());
                    //EventManager.Instance.Dispatch(this, new ExceptionEventArgs(e, e.Message));
                }

                //if (!string.IsNullOrEmpty(_configinfo.UpdateLogUrl))
                //{
                //    OpenBrowser(_configinfo.UpdateLogUrl);
                //}

                Clear(patchPath);
                StartApp();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //EventManager.Instance.Dispatch(this, new ExceptionEventArgs(e, e.Message));
            }

        }

        public override void StartApp()
        {
            try
            {
                //var mainAppPath = CheckPath(_configinfo.AppPath, _configinfo.Version.StartAppCmd);
                //if (string.IsNullOrEmpty(mainAppPath))
                //    throw new Exception($"Can't find the app {mainAppPath}!");
                var mainAppPath= Path.Combine(context.AppPath, context.UpdateVersion.StartAppCmd);
                Process.Start(mainAppPath);

                //if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //{
                //    var bowlAppPath = CheckPath(_configinfo.InstallPath, _configinfo.Bowl);
                //    if (!string.IsNullOrEmpty(bowlAppPath))
                //        Process.Start(bowlAppPath);
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //EventManager.Instance.Dispatch(this, new ExceptionEventArgs(e, e.Message));
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}