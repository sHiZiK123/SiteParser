using JRPC.Service;
using SimpleServiceLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleServiceServer {
    public class SimpleService : JRpcModule, ISimpleService {
        string domainName = string.Empty;
       

        public SimpleService() {
        }

        public async Task<ReadOnlyCollection<Uri>[]> GetWebsitesLinks(string[] urls)
        {
            return await GetWebsitesLinksAsync(urls);
        }

        public async Task<ReadOnlyCollection<Uri>[]> GetWebsitesLinksAsync(string[] urls)
        {
            var messageWorker = new MessageWorker();
            var absoluteLinkParser = new AbsoluteLinkParserCore();
            var hrefLinkParserCore = new HrefLinkParserCore();
            var linkParsers = new List<IParserLinkCore>() { absoluteLinkParser, hrefLinkParserCore };
            List<SiteParser> siteParsers = new List<SiteParser>();
            Dictionary<SiteParser, Uri> dictionary = new Dictionary<SiteParser, Uri>();

            foreach (var site in urls)
            {
                var siteParser = new SiteParser(messageWorker);
                siteParsers.Add(siteParser);
                dictionary.Add(siteParser, new Uri(site));
            }

            var tasks = siteParsers.Select(i =>
            {
                return i.GetUrlTreeAsync(linkParsers, dictionary[i]);
            });
            var parsedLinks = await Task.WhenAll(tasks);
            Console.WriteLine("All site are parsed.");
            return parsedLinks;
        }

        public string SimpleMethod() {
            return "DATA FROM SERVICE";
        }
    }
}
