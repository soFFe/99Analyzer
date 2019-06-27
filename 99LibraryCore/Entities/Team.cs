using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Team : Entity
    {
        public string Name { get; set; }
        public string Shortname { get; set; }
        public string LogoURL { get; set; }
        public Dictionary<Season, Division> Seasons { get; set; } = new Dictionary<Season, Division>();

        public Team(HtmlNode teamHtml)
        {
            node = teamHtml;
        }

        public override bool ParseInfo()
        {
            // Get team name
            try
            {
                var h2Name = node.SelectSingleNode("//div[@id='content']/div[2]/h2");
                var h2NameSplit = h2Name.InnerText.Trim().Split(' ');
                var h2NameSplitShortName = h2NameSplit.Last();
                Name = string.Join(" ", h2NameSplit.Where(s => s != h2NameSplitShortName));
                Shortname = h2NameSplitShortName.Trim(new char[] { '(', ')' });
            }
            catch (Exception x)
            {
                ErrorHandling.Error("Couldn't parse the name of the team");
                ErrorHandling.Log(x.Message);
                return false;
            }

            // Get Team Logo
            try
            {
                var imgNode = node.SelectSingleNode("//div[@id='content']/img[1]");
                LogoURL = imgNode.Attributes["src"].Value;
            }
            catch (Exception)
            {
                // whatever..
            }

            // Get played seasons and their divisions
            try
            {
                var tblSeasons = node.SelectSingleNode("//div[@id='content']/table[last()-1]");
                var trSeasons = tblSeasons.Elements("tr");
                var n = 0;
                foreach (var tr in trSeasons)
                {
                    n++;
                    try
                    {
                        // get season
                        var season = new Season(tr);
                        season.ParseInfo();

                        // get division basic info
                        var division = new Division(tr);
                        division.ParseInfo();

                        Seasons.Add(season, division);
                    }
                    catch (Exception x)
                    {
                        ErrorHandling.Log($"Fetching info for season #{ n } failed");
                        ErrorHandling.Log(x.Message);
                    }
                }
            }
            catch (Exception x)
            {
                ErrorHandling.Error($"Couldn't parse the played seasons for Team \"{ Name }\"");
                ErrorHandling.Log(x.Message);
                return false;
            }

            return true;
        }
    }
}
