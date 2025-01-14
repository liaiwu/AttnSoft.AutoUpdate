using GeneralUpdate.Common.Shared.Object;
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
            verInfos = context.OnGetUpdateVersionInfo.Invoke(context);
        }
        else
        {
            context.UseOssGetVersionInfo();
            verInfos = context.OnGetUpdateVersionInfo?.Invoke(context);
        }
        if (verInfos == null)
        {
            context.OnUpdateException?.Invoke(new Exception("从服务器获取版本信息失败!"));
            return;
        }
        var updatVerInfo = UpdateApp.GetUpdateVersion(verInfos, context.ClientVersion);
        if (updatVerInfo == null)
        {
            return;
        }
        context.IsMainUpdate = true;
        context.UpdateVersion = updatVerInfo;
        if (context.OnFindNewVersion != null)
        {
            context.OnFindNewVersion(next, context);
        }
        else
        {
            await next(context);
        }
    }
}