using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGgerServer
{
    public class Logger
    {
        private static readonly string folder = GetAppSettingValue("Trace");
        public static readonly int logsize = int.Parse(GetAppSettingValue("LogSize"));
        public static void LogWithNoLock(string message)
        {
            try
            {
                CreateLogFile(folder);

                FileInfo fi = new FileInfo(folder);
                var size = fi.Length >> 20;
                var fileMode = size >= logsize ? FileMode.Truncate : FileMode.Append;

                using (var fs = new FileStream(folder, fileMode, FileAccess.Write, FileShare.Write))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(message);
                }

            }
            catch (Exception ex)
            {
                LogWithNoLock($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}:{ex.Message}");
            }

        }

        private static void CreateLogFile(string folder)
        {
            if (!File.Exists(folder))
            {
                using (File.Create(folder)) { }
            }
        }

        public static string GetAppSettingValue(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

    }
}
