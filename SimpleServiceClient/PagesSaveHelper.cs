using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceClient
{
    public class PagesSaveHelper
    {
        static HttpClient httpClient = new HttpClient();

        public void Save(ReadOnlyCollection<Uri>[] sites, string path)
        {
            Task.Run(() => SaveCore(sites, path)).Wait();
            Console.WriteLine("All links are saved.");
        }

        public async Task SaveCore(ReadOnlyCollection<Uri>[] sites, string path)
        {
            foreach (var site in sites)
            {
                var newPath = path + @"\" + site[0].Host;
                Directory.CreateDirectory(newPath);
                foreach (Uri uri in site)
                {
                    var uriStr = uri.ToString();
                    if (!uriStr.Contains("pdf") && !uriStr.Contains("jpg") && !uriStr.Contains("png"))
                        await DownloadAndSavePageCore(uri, newPath);
                }
            }
        }

        private async Task DownloadAndSavePageCore(Uri url, string path)
        {
            var page = await LoadUrlAsyns(url);
            if (page.Contains("<!DOCTYPE"))
                File.WriteAllText(path + @"\page" + Guid.NewGuid() + ".html", page);
        }

        private async Task<string> LoadUrlAsyns(Uri url)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStringAsync();
                    return string.Empty;
                }
            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }
        }
    }
}
