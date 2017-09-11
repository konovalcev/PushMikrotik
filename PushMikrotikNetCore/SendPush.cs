using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PushMikrotik
{
    internal class MikrotikVersions
    {
        public static string CurrentVersion { get; set; }
        public static string NewVersion { get; set; }
        public static string Status { get; set; }
    }

    internal class SendPush
    {
        static void Main(string[] args)
        {
            try
            {
                CheckUpdatesAsync();
                Console.WriteLine(MikrotikVersions.Status);
                if (MikrotikVersions.Status != "New version is available" & MikrotikVersions.CurrentVersion.Equals(MikrotikVersions.NewVersion))
                {
                    SendPushAlertAsync($"Mikrotik: {MikrotikVersions.Status}", $"Current version is: {MikrotikVersions.CurrentVersion} The new version is: {MikrotikVersions.NewVersion}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void CheckUpdatesAsync()
        {
            var router = new MikrotikAPI(); // router ip 
            try
            {
                if (!router.Login("username", "password"))
                {
                    Console.WriteLine("Could not log in");
                    router.Close();
                }
                router.Send("/system/package/update/check-for-updates"); // path to command
                router.Send("", true); // confirm to execute
                var results = router.Read();
                results.RemoveAt(0);
                var MikrotikVersions = new MikrotikVersions();
                MikrotikVersions.CurrentVersion = getBetween(results.First(), "=current-version=", "=latest");
                MikrotikVersions.NewVersion = getBetween(results.First(), "=latest-version=", "=status=");
                MikrotikVersions.Status = getBetween(results.First(), "status=", "=.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void SendPushAlertAsync(string title, string body)
        {
            var url = @"https://api.pushbullet.com/v2/pushes";
            var apiKey = "o.H8Z1jRwOyS6sE9Dywz851MF26B18qT7G";
            var type = "note";
            var title1 = title;
            var body1 = body;
            var data = Encoding.ASCII.GetBytes(String.Format("{{ \"type\": \"{0}\", \"title\": \"{1}\", \"body\": \"{2}\"}}", type, title1, body1));
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Access-Token", apiKey);
                client.PostAsync(url, new StringContent(Encoding.UTF8.GetString(data), Encoding.UTF8, "application/json"));
                var result = client.GetAsync(url).Result;
            }
        }
        private static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
