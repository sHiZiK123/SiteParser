using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JRPC.Client;
using SimpleServiceLibrary;

namespace SimpleServiceClient {
    class Program {
        
        static void Main(string[] args) {
            PagesSaveHelper pagesSaveHelper = new PagesSaveHelper();
            string[] domainNames = {
                "http://xn----8sbijoijw0aeo.xn--p1ai/",
                "http://uk4gorodtula.ru/",
                "http://rem-super.ru/"
            };
            string saveDataPath = "Saved pages";

            var client = new JRpcClient("http://127.0.0.1:12345");
            var proxy = client.GetProxy<ISimpleService>("SimpleService");
            Console.WriteLine("Start parsing sites...");
            ReadOnlyCollection<Uri>[] sites = proxy.GetWebsitesLinks(domainNames).Result;
            Console.WriteLine();
            Console.WriteLine("End parsing sites.");
            Console.WriteLine("Start downloading pages...");
            pagesSaveHelper.Save(sites, saveDataPath);
            Console.ReadLine();
        }
    }
}

