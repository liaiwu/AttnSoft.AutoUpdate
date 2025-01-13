using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.Shared.Object;
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
public class CheckUpdateUseOSSMiddleware : ICheckUpdate
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        var httpService = HttpFactory.GetHttpService(context.UpdateUrl);
        var verInfos= httpService.Get<List<VersionInfo>>("");
        var updatVerInfo = UpdateApp.GetUpdateVersion(verInfos, context.GetClientVersion());
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