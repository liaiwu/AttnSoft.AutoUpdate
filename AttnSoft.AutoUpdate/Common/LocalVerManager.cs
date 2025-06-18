using AttnSoft.AutoUpdate.JsonContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Common
{
    public class LocalVerManager
    {
        const string filename = "Version.json";
        string AppPath;
        public LocalVerManager(string appPath)
        {
            AppPath = appPath;
        }
        VersionInfo? versionInfo;
        public VersionInfo? GetVersionInfo()
        {
            if (versionInfo == null)
            {
                string fileFullName=Path.Combine(AppPath, filename);
                if (File.Exists(fileFullName)) 
                {
                    string jsonString = string.Empty;
                    if (File.Exists(filename))
                    {
                        jsonString = File.ReadAllText(filename);
#if NETFRAMEWORK
                        versionInfo = DefaultJsonConvert.JsonConvert.Deserialize<VersionInfo>(jsonString);
#else
                        versionInfo = DefaultJsonConvert.JsonConvert.Deserialize<VersionInfo>(jsonString, VersionInfoJsonContext.Default.VersionInfo);
#endif
                    }
                }
            }
            return versionInfo;
        }
        public void SaveVersionInfo(VersionInfo versionInfo)
        {
            string fileFullName = Path.Combine(AppPath, filename);
#if NETFRAMEWORK
            string jsonString = DefaultJsonConvert.JsonConvert.Serialize(versionInfo);
#else
            string jsonString = DefaultJsonConvert.JsonConvert.Serialize(versionInfo, VersionInfoJsonContext.Default.VersionInfo);
#endif
            File.WriteAllText(fileFullName, jsonString);
        }

    }
}
