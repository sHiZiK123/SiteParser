using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public class SiteParser : ISiteParser, IDisposable
    {
        const int numberOfThreads = 15;

        Uri mainUri;
        volatile int pendingTasks;
        readonly IMessageWorker messageWorker;
        readonly BlockingCollection<Uri> pageUris = new BlockingCollection<Uri>();
        HttpClient httpClient = new HttpClient();
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        readonly ConcurrentDictionary<string, Uri> processedUris = new ConcurrentDictionary<string, Uri>();
        readonly CancellationToken token;
        readonly List<Task> tasks;
        IEnumerable<IParserLinkCore> parsers;
        TaskCompletionSource<ReadOnlyCollection<Uri>> resultTask;
        bool disposed;

        public SiteParser(IMessageWorker messageWorker)
        {
            this.messageWorker = messageWorker ?? throw new ArgumentNullException(nameof(messageWorker));

            token = cancellationTokenSource.Token;
            tasks = Enumerable.Range(1, numberOfThreads).Select(x => StartProcessPageTasks()).ToList();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ReadOnlyCollection<Uri>> GetUrlTreeAsync(IEnumerable<IParserLinkCore> parsingStrategies, Uri address)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(SiteParser));
            }

            this.parsers = parsingStrategies ?? throw new ArgumentNullException(nameof(parsingStrategies));

            this.mainUri = address;
            resultTask = new TaskCompletionSource<ReadOnlyCollection<Uri>>();
            FilterUri(address);
            return await resultTask.Task;
        }

        private void FilterUri(Uri uri)
        {
            if (uri.Host == mainUri.Host)
            {
                Interlocked.Increment(ref pendingTasks);
                pageUris.Add(uri);
                return;
            }
        }

        private void CheckPendingTasks()
        {
            Interlocked.Decrement(ref pendingTasks);
            if (pendingTasks == 0)
            {
                var result = new ReadOnlyCollection<Uri>(processedUris.Values.ToList());
                resultTask.SetResult(result);
            }
        }

        private Task StartProcessPageTasks()
        {
            var t = Task.Run(async () =>
            {
                while (!pageUris.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    await ProcessPage(token);
                }
            }, token);

            return Task.CompletedTask;
        }


        private async Task ProcessPage(CancellationToken token)
        {
            var uri = pageUris.Take(token);

            token.ThrowIfCancellationRequested();

            bool canAdd = processedUris.TryAdd(uri.AbsoluteUri, uri);
            if (canAdd)
            {
                string page = await LoadLinkAsyns(uri.AbsoluteUri, token);
                if (page == string.Empty)
                    return;
                token.ThrowIfCancellationRequested();

                var newFoundUrls = parsers.SelectMany(s => s.ProcessPage(page));
                foreach (var newUrl in newFoundUrls)
                {
                    var patchedUrl = newUrl.StartsWith("www") ? mainUri.Scheme + "://" + newUrl : newUrl;

                    if (Uri.TryCreate(patchedUrl, UriKind.RelativeOrAbsolute, out Uri parsedUri))
                    {
                        if (!parsedUri.IsAbsoluteUri)
                        {
                            parsedUri = new Uri(uri, parsedUri);
                        }

                        FilterUri(parsedUri);
                    }
                }
            }

            CheckPendingTasks();
        }

        private async Task<string> LoadLinkAsyns(string url, CancellationToken token)
        {
            try
            {
                messageWorker.PrintMessage("Process page "+ url);
                using (var response = await httpClient.GetAsync(url, token))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStringAsync();
                    return string.Empty;
                }
            }
            catch (HttpRequestException)
            {
                messageWorker.PrintMessage($"The error has happened while loading: {url}");
                return string.Empty;
            }
        }

        public void Dispose()
        {
            disposed = true;
            resultTask.TrySetException(new TaskCanceledException());
            cancellationTokenSource.Cancel(true);
            Task.WhenAll(tasks).ContinueWith(t => cancellationTokenSource.Dispose()).Wait();
            cancellationTokenSource.Dispose();
            httpClient.Dispose();
            pageUris.Dispose();
        }
    }
}
