using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 检查更新
/// </summary>
public interface ICheckUpdate:IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 检查更新中间件
/// </summary>
public class CheckUpdateMiddleware : ICheckUpdate
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        List<VersionInfo>? verInfos;
        if (context.OnGetUpdateVersionInfo != null)
        {
            verInfos =await context.OnGetUpdateVersionInfo.Invoke(context);
        }
        else
        {
            verInfos = await UpdateContextExtensions.GetVersionInfoFromOss(context); 
        }
        if (verInfos == null)
        {
            context.OnUpdateException?.Invoke(new Exception("Failed to retrieve version information from server!"));
            return;
        }
        var updatVerInfo = UpdateApp.GetUpdateVersion(verInfos, context.ClientVersion);
        if (updatVerInfo == null)
        {
            Console.WriteLine("Version is newer, no update required!");
            return;
        }
        context.IsMainUpdate = true;
        context.UpdateVersion = updatVerInfo;
        if (context.OnFindNewVersion != null)
        {
            await context.OnFindNewVersion(next, context);
        }
        else
        {
            await next(context);
        }
    }
}