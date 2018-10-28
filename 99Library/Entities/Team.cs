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
        private string name;
        private string shortname;
        private Dictionary<Season, Division> seasons = new Dictionary<Season, Division>();

        public string Name { get => name; set => name = value; }
        public string Shortname { get => shortname; set => shortname = value; }
        public Dictionary<Season, Division> Seasons { get => seasons; set => seasons = value; }

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
                name = h2NameSplit.First();

                char[] trimChars = { '(', ')' };
                shortname = h2NameSplit.Last().Trim(trimChars);
            }
            catch (Exception x)
            {
                ErrorHandling.Error("Couldn't parse the name of the team");
                ErrorHandling.Log(x.Message);
                return false;
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

                        seasons.Add(season, division);
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
                ErrorHandling.Error($"Couldn't parse the played seasons for Team \"{ name }\"");
                ErrorHandling.Log(x.Message);
                return false;
            }

            return true;
        }
    }
}
