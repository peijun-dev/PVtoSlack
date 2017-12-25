using Codeplex.Data;
using Google.Apis.Analytics.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Analytics.v3.Data;

namespace PVtoSlack
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //webhookurlの取得
            Console.Write("WebhookURLを入力してください。 : ");
            var webhook = Console.ReadLine(); 
            string WEBHOOK_URL = webhook;

            //APIkeyの設定
            X509Certificate2 certificate = new X509Certificate2(@"apikey.p12", "notasecret", X509KeyStorageFlags.Exportable);

            //APIの設定
            Console.Write("APIのIDを入力してください。 : ");
            var id = Console.ReadLine();

            string serviceAccountEmail = id;
            ServiceAccountCredential credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = new[] { AnalyticsService.Scope.Analytics, AnalyticsService.Scope.AnalyticsReadonly }
            }
            .FromCertificate(certificate));

            AnalyticsService service = new AnalyticsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "pvtoslack",
            });

            string date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            //PVの取得
            Console.Write("GA：");
            var ga = Console.ReadLine();
            var ga1 = "ga:" + ga;
            DataResource.GaResource.GetRequest request = service.Data.Ga.Get(ga1, date, date, "ga:pageviews");

            GaData data = request.Execute();
            
            var wc = new WebClient();

            //送信するBOTの設定
            var pvinfo = DynamicJson.Serialize(new
            {
                text = (data.Rows[0][0]),
                username = "前日のPVを投稿するBOT"

            });

            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json;charset=UTF-8");
            wc.Encoding = Encoding.UTF8;

            while (true) {
                //送信
                wc.UploadString(WEBHOOK_URL, pvinfo);

                //1日待機
                System.Threading.Thread.Sleep(86400000);
            }

        }

        
    }
}

