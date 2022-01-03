using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JPEGgerServer
{
    [Serializable]
    public class Camera
    {
        public static List<Camera> Cameras = GetCameraList();
        public static List<CameraGroup> CameraGroups = GetCameraGroupList();

        public string Camera_Name { get; set; }
        public string Device_Name { get; set; }
        public int IsNetCam { get; set; }
        public string Ip_Address { get; set; }
        public string Port_Nbr { get; set; }
        public string Username { get; set; }
        public string Pwd { get; set; }
        public string Yardid { get; set; }
        public string URL { get; set; }
        public string VideoURL { get; set; }

        private static List<Camera> GetCameraList()
        {
            var camCollection = Get<List<Camera>>($"cameras?yardid={GetAppSettingValue("YardId")}");// Need to check yarid from Tim
            if (camCollection != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var camera in camCollection)
                {
                    if (!string.IsNullOrEmpty(camera.VideoURL) && !string.IsNullOrEmpty(camera.Username) && !string.IsNullOrEmpty(camera.Pwd) && !string.IsNullOrEmpty(camera.Ip_Address))
                    {
                        sb.Clear();
                        sb.Append("http://");
                        sb.Append(camera.Username);
                        sb.Append(":");
                        sb.Append(camera.Pwd);
                        sb.Append("@");
                        sb.Append(camera.Ip_Address);
                        sb.Append(camera.VideoURL);
                        camera.VideoURL = sb.ToString();

                    }
                    if (!string.IsNullOrEmpty(camera.URL) && !string.IsNullOrEmpty(camera.Ip_Address))
                    {
                        sb.Clear();
                        sb.Append("http://");
                        sb.Append(camera.Ip_Address);
                        sb.Append(camera.URL);
                        camera.URL = sb.ToString();
                    }
                }
                sb = null;
            }
            return camCollection;
        }

        private static List<CameraGroup> GetCameraGroupList()
        {
            return Get<List<CameraGroup>>($"camera_groups?yardid={GetAppSettingValue("YardId")}"); // Need to check yarid from Tim
        }

        public static List<Camera> GetConfiguredCamera(string camera_name)
        {
            var camera = Cameras.Where(x => x.Camera_Name.ToLower() == camera_name.ToLower()).FirstOrDefault();
            if (camera != null)
            {
                return new List<Camera> { camera };
            }
            else
            {
                if (CameraGroups?.Count > 0)
                {
                    var camGroup = CameraGroups.Where(x => x.Cam_Group.ToLower() == camera_name.ToLower());
                    if (camGroup.Any())
                    {
                        return Cameras.Where(x => camGroup.Any(z => x.Camera_Name.ToLower() == z.Cam_Name.ToLower())).ToList();
                    }
                }
            }

            return null;
        }

        public static T Get<T>(string path)
        {
            var httpResponseString = string.Empty;

            var method = "";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", GetAppSettingValue("JPEGgerToken"));
                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    method = GetAppSettingValue("JPEGgerAPI") + path;
                    using (HttpResponseMessage response = client.GetAsync(method).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            httpResponseString = response.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvents($"Exception at Camera.Get: {ex.Message}");
                return JsonConvert.DeserializeObject<T>(httpResponseString);
            }
            return JsonConvert.DeserializeObject<T>(httpResponseString);
        }

        public static string GetAppSettingValue(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        private static void LogEvents(string input)
        {
            Logger.LogWithNoLock($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}:{input}");
        }

    }

    [Serializable]
    public class CameraGroup
    {
        public string Cam_Group { get; set; }
        public string Cam_Name { get; set; }
        public string Yardid { get; set; }
    }
}
