using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinetyNineLibrary.Entities;
using HtmlAgilityPack;

namespace NinetyNineLibrary.EntityModels
{
    public static class TeamModel
    {
        /// <summary>
        /// Gets an instance of Team by providing the team page URL
        /// </summary>
        /// <param name="TeamURL">The URL to the team's page</param>
        public static async Task<Team> GetInstanceAsync(string TeamURL)
        {
            var match = System.Text.RegularExpressions.Regex.Match(TeamURL, AnalyzerConstants.TeamUrlPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success)
                ErrorHandling.Error("The URL provided is not a Team URL");

            var parser = new UriParser(TeamURL);
            var result = await parser.ParseSingleAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            var team = new Team(doc.DocumentNode)
            {
                Url = TeamURL
            };

            return team;
        }
    }
}
