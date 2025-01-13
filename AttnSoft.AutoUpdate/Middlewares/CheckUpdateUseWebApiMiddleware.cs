using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.Shared.Object;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;

public class CheckUpdateUseWebApiMiddleware : ICheckUpdate
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        var httpService = HttpFactory.GetHttpService(context.UpdateUrl);
        var postData=new Dictionary<string, object> {
            { "Version",context.GetClientVersion().ToString()},
            { "AppKey",context.AppSecretKey},
        };
        var verInfos= httpService.Post<List<VersionInfo>>("",null,null,postData);
        //var verDto = httpService.Post<VersionRespDTO>("", null, null, postData);
        //if (verDto.Code == 200 && verDto.Body.Count > 0)
        //{
            //var verInfos = verDto.Body;
            if (verInfos == null)
            {
                return;
            }
            var updatVerInfo = UpdateApp.GetUpdateVersion(verInfos, context.GetClientVersion());
            if (updatVerInfo == null)
            {
                return;
            }
            context.IsMainUpdate = true;
            context.UpdateVersion = updatVerInfo;

            await next(context);
        //}

    }
}