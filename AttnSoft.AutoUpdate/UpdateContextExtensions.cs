
using AttnSoft.AutoUpdate.Common;
using System.Collections.Generic;


namespace AttnSoft.AutoUpdate;
public static partial class UpdateContextExtensions
{
    public static UpdateContext UseOssGetVersionInfo(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo += GetVersionInfoFromOss;
        return context;
    }
    private static List<VersionInfo> GetVersionInfoFromOss(UpdateContext context)
    {
        var httpService = HttpFactory.GetHttpService(context.UpdateUrl);
        var verInfos = httpService.Get<List<VersionInfo>>("");
        return verInfos;
    }
    public static UpdateContext UseWebApiGetVersionInfo(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo += GetVersionInfoFroWebApi;
        return context;
    }
    private static List<VersionInfo> GetVersionInfoFroWebApi(UpdateContext context)
    {
        var httpService = HttpFactory.GetHttpService(context.UpdateUrl);
        var postData = new Dictionary<string, object> {
            { "Version",context.ClientVersion.ToString()},
            { "AppKey",context.AppSecretKey},
        };
        var verInfos = httpService.Post<List<VersionInfo>>("", null, null, postData);
        return verInfos;
    }
}
