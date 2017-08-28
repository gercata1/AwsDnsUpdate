using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AwsDnsUpdate
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }


        static async Task MainAsync(string[] args)
        {
            #region LoadingAppSettings
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            #endregion

            var ipUrl = Configuration["ipUrl"];

            var client = new HttpClient();

            var stringresponse = await client.GetStringAsync(ipUrl);

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(stringresponse);

            string ip = jsonResponse.ip_addr;

            Console.WriteLine(ip);


            //Console.WriteLine($"ipUrl = {Configuration["ipUrl"]}");
            //Console.WriteLine(
            //    $"awskey = {Configuration["aws:key"]}");
            //Console.WriteLine(
            //    $"awssecret = {Configuration["aws:secret"]}");
            //Console.WriteLine();


            Console.Read();
        }
    }
}
