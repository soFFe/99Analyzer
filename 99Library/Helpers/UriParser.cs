using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NinetyNineLibrary
{
    public class UriParser
    {
        private List<string> urls;

        public List<string> Urls { get => urls; private set => urls = value; }

        public UriParser(string url)
        {
            urls = new List<string>
            {
                url
            };
        }

        public UriParser(ICollection<string> urlCollection)
        {
            urls = new List<string>();
            foreach(var url in urlCollection)
            {
                urls.Add(url);
            }
        }

        /// <summary>
        /// Parse only one URL asynchronously.
        /// If you provided more than one URL in the constructor, only the first will be prased
        /// </summary>
        /// <returns>Response string</returns>
        public async Task<string> ParseSingleAsync()
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", AnalyzerConstants.UserAgent);
                var url = urls.First();
                var result = await client.DownloadStringTaskAsync(url);

                return result;
            }
        }

        /// <summary>
        /// Parse all URLs parallel and asynchronously
        /// </summary>
        /// <returns>Key-Value Pair containing the requested URL (Key) and the response (Value)</returns>
        public async Task<IDictionary<string, string>> ParseAllAsync()
        {
            // parse all urls parallel and asynchronously
            var tasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", AnalyzerConstants.UserAgent);
                    tasks.Add(client.DownloadStringTaskAsync(url));
                }
            }

            var results = await Task.WhenAll(tasks);

            // Set return value to Dictionary<Url, Result>
            var ret = urls.Select((k, i) => new { k, v = results[i] }).ToDictionary(x => x.k, x => x.v);

            return ret;
        }
    }
}
