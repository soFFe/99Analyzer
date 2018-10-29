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
        public static async Task<List<string>> ParseMatchLinksAsync(Division div, string teamShortName)
        {
            var matchUrls = new List<string>();

            using (var parser = new UriParser(div.Url))
            {
                var result = await parser.ParseSingleAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(result);

                try
                {
                    var tblMatches = doc.DocumentNode.SelectSingleNode("//table[@class='league_table_matches']");
                    var trMatches = tblMatches.Elements("tr");

                    foreach (var tr in trMatches)
                    {
                        // skip "Spieltag X" and empty cells
                        if (tr.Elements("th").Count() > 0)
                            continue;

                        var matchUrl = tr.SelectSingleNode("td/a").GetAttributeValue("href", "");
                        if (string.IsNullOrEmpty(matchUrl))
                            throw new Exception("Could not fetch MatchUrl");

                        var teamFound = false;

                        var teamColumns = tr.SelectNodes("td/a");

                        // get 2nd and 3rd column anchors
                        for (var i = 1; i <= 2 && !teamFound; i++)
                        {
                            var col = teamColumns[i];
                            var shortName = col.InnerText.Trim();
                            if (i == 2)
                            {
                                var regExName = System.Text.RegularExpressions.Regex.Match(col.InnerText, AnalyzerConstants.DivisonOverviewThirdColumnTeamNamePattern); // very long
                                if (!regExName.Success)
                                {
                                    ErrorHandling.Log($"RegEx for getting the second team's name failed. MatchUrl: \"{ matchUrl }\"");
                                    break;
                                }

                                shortName = regExName.Groups[1].Value;
                            }
                            if (shortName == teamShortName)
                                teamFound = true;
                        }

                        if (teamFound)
                            matchUrls.Add(matchUrl);
                    }
                }
                catch (Exception x)
                {
                    ErrorHandling.Error($"Could not parse Matchlinks for Divison \"{ div.Name }\"");
                    ErrorHandling.Log($"Exception Message: { x.Message }");
                }
            }

            return matchUrls;
        }

        public static async Task<List<Match>> ParseMatchesAsync(ICollection<string> matchUrls)
        {
            var matches = new List<Match>();

            using (var parser = new UriParser(matchUrls))
            {
                var result = await parser.ParseAllAsync();

                foreach (var request in result)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(request.Value);

                    try
                    {
                        var match = new Match(doc.DocumentNode)
                        {
                            Url = request.Key
                        };
                        var success = match.ParseInfo();

                        if (success)
                            matches.Add(match);
                    }
                    catch(Exception x)
                    {
                        ErrorHandling.Log($"Match could not be parsed. Exception: { x.Message }");
                    }
                }
            }

            return matches;
        }
    }
}
