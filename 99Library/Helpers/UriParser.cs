using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NinetyNineLibrary
{
    public class UriParser : IDisposable
    {
        private WebClient client = new WebClient();
        private List<string> urls;

        public List<string> Urls { get => urls; private set => urls = value; }

        public UriParser(string url)
        {
            client.Headers.Add("user-agent", AnalyzerConstants.UserAgent);
            urls = new List<string>
            {
                url
            };
        }

        public UriParser(ICollection<string> urlCollection)
        {
            client.Headers.Add("user-agent", AnalyzerConstants.UserAgent);
            urls = new List<string>();
            foreach(var url in urlCollection)
            {
                urls.Add(url);
            }
        }

        public async Task<string> ParseSingle()
        {
            var url = urls.First();
            var result = await client.DownloadStringTaskAsync(url);

            return result;
        }

        public async Task<ICollection<string>> Parse()
        {
            // parse all urls parallel and asynchronously
            var tasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                tasks.Add(client.DownloadStringTaskAsync(url));
            }

            var results = await Task.WhenAll(tasks);

            return results;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
