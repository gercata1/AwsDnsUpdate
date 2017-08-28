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
                throw new Exception("test exception");

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
        public static string GetFileName()
        {
            string logName = DateTime.Today.ToString("yyyyMMdd") + "_log.txt";
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Log");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            return Path.Combine(basePath , logName);
        }


        public static void Log(string message)
        {
            var logName = GetFileName();
            using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), logName)))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " +  message);
                sw.WriteLine();
                sw.WriteLine("_______________________________________________________________________________________________");
            }

            Console.WriteLine(message);
        }

        public static void Log(Exception e)
        {
            var logName = GetFileName();
            using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), logName)))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " + e.Message);
                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    sw.WriteLine("\t" + e.StackTrace);
                }

                sw.WriteLine();
                sw.WriteLine("_______________________________________________________________________________________________");
            }

            Console.WriteLine(e);
        }
    }
}
