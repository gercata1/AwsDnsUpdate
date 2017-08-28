using Amazon;
using Amazon.Route53;
using Amazon.Route53Domains;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Amazon.Route53.Model;
using System.Collections.Generic;

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

                var webClient = new HttpClient();

                var stringresponse = await webClient.GetStringAsync(ipUrl);

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(stringresponse);

                string externalIp = jsonResponse.ip_addr;

                var client = new AmazonRoute53Client(Configuration["aws:key"], Configuration["aws:secret"], RegionEndpoint.USEast2);

                var domain = Configuration["aws:domain"];
                if (!domain.EndsWith("."))
                {
                    domain += ".";
                }

                var zone = (await client.ListHostedZonesAsync()).HostedZones.FirstOrDefault(x => x.Name == domain);

                if (zone == null)
                    throw new Exception("There's no hosted zone for domain = " + domain);

                var subDomain = Configuration["aws:subdomain"] + "." + domain;
                var recordSet = (await client.ListResourceRecordSetsAsync(new Amazon.Route53.Model.ListResourceRecordSetsRequest { HostedZoneId = zone.Id }))
                                .ResourceRecordSets.FirstOrDefault(x => x.Type == "A" && x.Name == subDomain);

                if (recordSet == null)
                    throw new Exception("There's no subdomain = " + subDomain);

                var resourceRecord = recordSet.ResourceRecords.FirstOrDefault();

                if (resourceRecord == null)
                    throw new Exception("There's no config value for subdomain = " + subDomain);

                if (resourceRecord.Value != externalIp)
                {
                    resourceRecord.Value = externalIp;
                    var change = new Change()
                    {
                        ResourceRecordSet = recordSet,
                        Action = ChangeAction.UPSERT
                    };

                    var changeBatch = new ChangeBatch()
                    {
                        Changes = new List<Change> { change }
                    };


                    await client.ChangeResourceRecordSetsAsync(new ChangeResourceRecordSetsRequest { HostedZoneId = zone.Id, ChangeBatch = changeBatch });
                }

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

            return Path.Combine(basePath, logName);
        }


        public static void Log(string message)
        {
            var logName = GetFileName();
            using (StreamWriter sw = File.AppendText(Path.Combine(Directory.GetCurrentDirectory(), logName)))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " + message);
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
