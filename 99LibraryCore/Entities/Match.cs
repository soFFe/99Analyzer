using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;
using NinetyNineLibrary.Repositories;

namespace NinetyNineLibrary.Entities
{
    public class Match : Entity
    {
        public AnalyzerConstants.MatchStatus Status { get; set; }
        public Dictionary<AnalyzerConstants.TeamSide, int> Score { get; set; } = new Dictionary<AnalyzerConstants.TeamSide, int>();
        public Dictionary<AnalyzerConstants.TeamSide, string> TeamNames { get; set; } = new Dictionary<AnalyzerConstants.TeamSide, string>();
        public List<Vote> Votes { get; set; } = new List<Vote>();
        public List<Map> Maps { get; set; } = new List<Map>();
        public Season Season { get; set; } = null;

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
                Status = AnalyzerConstants.MatchStatus.NotPlayed;
                return true;
            }

            // fetch team names
            var teamNameNodes = node.SelectNodes("//div[@class='match_names']/div[@class='team']/a");
            TeamNames.Add(AnalyzerConstants.TeamSide.A, teamNameNodes[0].InnerText.Trim());
            TeamNames.Add(AnalyzerConstants.TeamSide.B, teamNameNodes[1].InnerText.Trim());

            // fetch map score
            var teamScoreNodes = node.SelectNodes("//div[@class='match_logos']/div[@class='score']/span");
            Score.Add(AnalyzerConstants.TeamSide.A, Convert.ToInt32(teamScoreNodes[0].InnerText));
            Score.Add(AnalyzerConstants.TeamSide.B, Convert.ToInt32(teamScoreNodes[1].InnerText));

            // was the match actually played or is this a nasty defwin?
            var resultsNode = node.SelectSingleNode("//div[@id='content']/h2[2]");
            if(resultsNode.InnerText != "Ergebnisse")
            {
                Status = AnalyzerConstants.MatchStatus.DefWin;
                return true;
            }

            Status = AnalyzerConstants.MatchStatus.Played;

            // get round score per map
            var nodeFirstMapScore = node.SelectSingleNode("//div[@id='content']/text()[preceding-sibling::br]");
            var firstMapScoreStr = nodeFirstMapScore.InnerText.Trim().Split(':');
            if(firstMapScoreStr.Count() != 3)
            {
                ErrorHandling.Log($"Unexpected Error: Could not find map score for match { url }");
                return false;
            }

            // first map
            var firstMapScores = new Dictionary<AnalyzerConstants.TeamSide, int>();
            var firstMapName = firstMapScoreStr[0];
            var firstMapScoreA = Convert.ToInt32(firstMapScoreStr[1].Trim());
            var firstMapScoreB = Convert.ToInt32(firstMapScoreStr[2].Trim());
            firstMapScores.Add(AnalyzerConstants.TeamSide.A, firstMapScoreA);
            firstMapScores.Add(AnalyzerConstants.TeamSide.B, firstMapScoreB);
            Maps.Add(new Map(firstMapName, firstMapScores));

            // second map
            var secondMapScores = new Dictionary<AnalyzerConstants.TeamSide, int>();
            var secondMapScoreStr = nodeFirstMapScore.NextSibling.NextSibling.InnerText.Trim().Split(':');
            var secondMapName = secondMapScoreStr[0];
            var secondMapScoreA = Convert.ToInt32(secondMapScoreStr[1].Trim());
            var secondMapScoreB = Convert.ToInt32(secondMapScoreStr[2].Trim());
            secondMapScores.Add(AnalyzerConstants.TeamSide.A, secondMapScoreA);
            secondMapScores.Add(AnalyzerConstants.TeamSide.B, secondMapScoreB);
            Maps.Add(new Map(secondMapName, secondMapScores));

            // get vote data from match log
            var logNode = node.SelectSingleNode("//table[@id='match_log']");
            var trNodes = logNode.SelectNodes("tr");
            var voteDataFound = false;
            foreach(var tr in trNodes)
            {
                if (voteDataFound)
                    break;

                var cols = tr.SelectNodes("td");
                if (cols == null || cols.Count == 0)
                    continue;

                var aktion = cols[2].InnerText; // 3rd column: "Aktion"
                if (aktion != "mapvote_ended")
                    continue;

                
                var voteData = cols[3].InnerText.Split(',');
                var voteDateStr = cols[0].SelectSingleNode("span").GetAttributeValue("title", "");
                DateTime voteDate = DateTime.MinValue;
                if(!string.IsNullOrEmpty(voteDateStr))
                {
                    voteDate = DateTime.Parse(voteDateStr);
                }

                foreach(var sVote in voteData)
                {
                    if (sVote == "timeouted")
                    {
                        Status = AnalyzerConstants.MatchStatus.DefWin;
                        break;
                    }
                    var RegExMatch = System.Text.RegularExpressions.Regex.Match(sVote.Trim(), AnalyzerConstants.MapvotePattern);
                    var validateLogEntry = RegExMatch.Success && RegExMatch.Groups.Count > 1; // First group is always the whole string

                    if(!validateLogEntry)
                    {
                        ErrorHandling.Log($"Could not read mapvote entry: \"{ sVote }\"");
                        continue;
                    }

                    // get captured groups
                    var reSide = RegExMatch.Groups[1].Value;
                    var reType = RegExMatch.Groups[2].Value;
                    var reMap = RegExMatch.Groups[3].Value;

                    // interpret groups
                    AnalyzerConstants.TeamSide side = AnalyzerConstants.TeamSide.Unknown;
                    if (reSide == "T1")
                        side = AnalyzerConstants.TeamSide.A;
                    else if (reSide == "T2")
                        side = AnalyzerConstants.TeamSide.B;
                    else
                        ErrorHandling.Log($"Could not read voting team: \"{ reSide }\"");

                    AnalyzerConstants.VoteType type = AnalyzerConstants.VoteType.Unknown;
                    if (reType == "bans")
                        type = AnalyzerConstants.VoteType.Ban;
                    else if (reType == "picks")
                        type = AnalyzerConstants.VoteType.Pick;
                    else
                        ErrorHandling.Log($"Could not read voting type: \"{ reType }\"");

                    var map = new Map(reMap);
                    if (type == AnalyzerConstants.VoteType.Pick)
                        map = Maps.Where(m => m.Name == map.Name).First();

                    // create vote instance
                    Vote vote;
                    if (voteDate != DateTime.MinValue)
                    {
                        vote = new Vote(side, type, map, voteDate);
                    }
                    else
                    {
                        vote = new Vote(side, type, map);
                    }
                    Votes.Add(vote);
                }

                voteDataFound = true;
            }

            if (!voteDataFound)
            {
                ErrorHandling.Log($"Could not find vote data for presumably played match { url }. This may happen if the match has no match log entries anymore.");
                return false;
            }

            return true;
        }
    }
}
