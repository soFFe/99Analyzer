using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinetyNineLibrary.Entities;
using HtmlAgilityPack;

namespace NinetyNineLibrary.EntityModels
{
    public static class MatchModel
    {
        public static async Task<List<string>> ParseMatchLinksAsync(Division div)
        {
            var MatchURLs = new List<string>();

            using (var parser = new UriParser(div.Url))
            {
                var result = await parser.ParseSingle();
                var doc = new HtmlDocument();
                doc.LoadHtml(result);

                try
                {
                    var tblMatches = doc.DocumentNode.SelectSingleNode("//table[@class='league_table_matches']");
                    var trMatches = tblMatches.Elements("tr");

                    foreach (var tr in trMatches)
                    {
                        // skip "Spieltag X"
                        if (tr.Elements("th").Count() > 0)
                            continue;

                        var match = new Match(tr);
                        match.ParseInfo();
                    }
                }
                catch (Exception)
                {
                    ErrorHandling.Error($"Could not parse Match ");
                }
            }

            return MatchURLs;
        }

        public static async Task<List<Match>> ParseMatchesAsync(ICollection<string> matchUrls)
        {
            var lMatches = new List<Match>();

            using (var parser = new UriParser(matchUrls))
            {
                var result = await parser.Parse();

                foreach (var html in result)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                }
            }

            return lMatches;
        }
    }
}
