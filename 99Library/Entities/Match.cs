using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Match : Entity
    {
        private AnalyzerConstants.MatchStatus status;
        private Dictionary<AnalyzerConstants.TeamSide, int> mapScores = new Dictionary<AnalyzerConstants.TeamSide, int>();
        private Dictionary<AnalyzerConstants.TeamSide, string> teamNames = new Dictionary<AnalyzerConstants.TeamSide, string>();

        public AnalyzerConstants.MatchStatus Status { get => status; set => status = value; }
        public Dictionary<AnalyzerConstants.TeamSide, int> MapScores { get => mapScores; set => mapScores = value; }
        public Dictionary<AnalyzerConstants.TeamSide, string> TeamNames { get => teamNames; set => teamNames = value; }

        public Match(HtmlNode matchDoc)
        {
            node = matchDoc;
        }

        // TODO: check for nodes if they've even been found
        public override bool ParseInfo()
        {
            var strStatus = node.SelectSingleNode("//div[@class='match_logos']/div[@class='score']/div").InnerText;
            if (strStatus == "offen")
            {
                status = AnalyzerConstants.MatchStatus.NotPlayed;
                return true;
            }

            // fetch team names
            var teamNameNodes = node.SelectNodes("//div[@class='match_names']/div[@class='team']/a");
            teamNames.Add(AnalyzerConstants.TeamSide.A, teamNameNodes[0].InnerText.Trim());
            teamNames.Add(AnalyzerConstants.TeamSide.B, teamNameNodes[1].InnerText.Trim());

            // fetch map score
            var teamScoreNodes = node.SelectNodes("//div[@class='match_logos']/div[@class='score']/span");
            mapScores.Add(AnalyzerConstants.TeamSide.A, Convert.ToInt32(teamScoreNodes[0].InnerText));
            mapScores.Add(AnalyzerConstants.TeamSide.B, Convert.ToInt32(teamScoreNodes[1].InnerText));

            // was the match actually played or is this a nasty defwin?
            var resultsNode = node.SelectSingleNode("//div[@id='content']/h2[2]");
            if(resultsNode.InnerText != "Ergebnisse")
            {
                status = AnalyzerConstants.MatchStatus.DefWin;
                return true;
            }

            status = AnalyzerConstants.MatchStatus.Played;

            // todo: analyze log for bans

            return true;
        }
    }
}
