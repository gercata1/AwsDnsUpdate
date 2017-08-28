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
            try
            {
                var ipUrl = Configuration["ipUrl"];

                var client = new HttpClient();

                var stringresponse = await client.GetStringAsync(ipUrl);

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(stringresponse);

                string ip = jsonResponse.ip_addr;



                //Console.WriteLine(ip);

                //using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), "output.txt")))
                //{
                //    sw.WriteLine("ip=" + ip);
                //}




                //Console.WriteLine($"ipUrl = {Configuration["ipUrl"]}");
                //Console.WriteLine(
                //    $"awskey = {Configuration["aws:key"]}");
                //Console.WriteLine(
                //    $"awssecret = {Configuration["aws:secret"]}");
                //Console.WriteLine();


                //Console.Read();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }

            
        }



    }

    public static class Logger
    {
        public static string logName = DateTime.Today.ToString("yyyyMMdd") + "_log.txt";

        public static void Log(string message)
        {

            using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), logName)))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " +  message);
            }

            Console.WriteLine(message);
        }

        public static void Log(Exception e)
        {

            using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), logName)))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " + e.Message);
                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    sw.WriteLine("\t" + e.StackTrace);
                }
            }

            Console.WriteLine(e);
        }
    }
}
