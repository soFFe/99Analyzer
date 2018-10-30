﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NinetyNineLibrary.Repositories;

namespace NinetyNineLibrary.Entities
{
    public class Match : Entity
    {
        private AnalyzerConstants.MatchStatus status;
        private Dictionary<AnalyzerConstants.TeamSide, int> mapScores = new Dictionary<AnalyzerConstants.TeamSide, int>();
        private Dictionary<AnalyzerConstants.TeamSide, string> teamNames = new Dictionary<AnalyzerConstants.TeamSide, string>();
        private List<Vote> votes = new List<Vote>();

        public AnalyzerConstants.MatchStatus Status { get => status; set => status = value; }
        public Dictionary<AnalyzerConstants.TeamSide, int> MapScores { get => mapScores; set => mapScores = value; }
        public Dictionary<AnalyzerConstants.TeamSide, string> TeamNames { get => teamNames; set => teamNames = value; }
        public List<Vote> Votes { get => votes; set => votes = value; }

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
                foreach(var sVote in voteData)
                {
                    if (sVote == "timeouted")
                    {
                        status = AnalyzerConstants.MatchStatus.DefWin;
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
                    Map map = new Map(reMap);

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

                    // create vote instance
                    var vote = new Vote(side, type, map);
                    votes.Add(vote);
                }

                voteDataFound = true;
            }

            if (!voteDataFound)
            {
                ErrorHandling.Log($"Unexpected Error: Could not find vote data for presumably played match { url }");
                return false;
            }

            // todo: get round score per map

            return true;
        }
    }
}
