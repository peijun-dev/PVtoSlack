using Codeplex.Data;
using Google.Apis.Analytics.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Analytics.v3.Data;

namespace PVtoSlack
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.Write("WebhookURLを入力してください。 : ");
            var webhook = Console.ReadLine(); // ユーザーの入力した文字列を1行読み込む
            string WEBHOOK_URL = webhook;

            //APIkeyの設定
            X509Certificate2 certificate = new X509Certificate2(@"apikey.p12", "notasecret", X509KeyStorageFlags.Exportable);

            //APIの設定
            string serviceAccountEmail = "pvtoslack@pvtoslack.iam.gserviceaccount.com";
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
            DataResource.GaResource.GetRequest request = service.Data.Ga.Get("ga:133305208", date, date, "ga:pageviews");

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

            //送信
            wc.UploadString(WEBHOOK_URL, pvinfo);
        }

        
    }
}

