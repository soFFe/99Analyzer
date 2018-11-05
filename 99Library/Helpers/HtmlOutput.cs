using NinetyNineLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using NinetyNineLibrary.Repositories;

namespace NinetyNineLibrary.Helpers
{
    public static class HtmlOutput
    {
        public static bool CreateFile(Team team, ICollection<Match> matches, string fileName)
        {
            var html = File.ReadAllText(AnalyzerConstants.HtmlTemplatePath);

            html = html.Replace("/*:title:*/", AnalyzerConstants.ProjectName + " - " + team.Name);

            // select all votes from all matches where votes are present. even if they're not flagged as played.
            var allVotes = from m in matches
                           from v in m.Votes
                           where
                               m.Votes.Count > 0
                           select v;

            // only our votes
            var ourVotes = from m in matches
                        from n in m.TeamNames
                        from v in m.Votes
                        where
                            m.Votes.Count > 0
                            && n.Value == team.Name
                            && v.Team == n.Key
                        select v;

            // === BANS ===
            var bans = GetAllVotesByType(ourVotes, AnalyzerConstants.VoteType.Ban);
            var lBanMaps = bans.Keys;
            var lBanCount = bans.Values;

            html = html
                .Replace("/*:bans.dataset:*/", string.Join(", ", lBanCount))
                .Replace("/*:bans.labels:*/", string.Join("\", \"", lBanMaps));

            // === PICKS ===
            var picks = GetAllVotesByType(ourVotes, AnalyzerConstants.VoteType.Pick);
            var lPickMaps = picks.Keys;
            var lPickCount = picks.Values;

            html = html
                .Replace("/*:picks.dataset:*/", string.Join(", ", lPickCount))
                .Replace("/*:picks.labels:*/", string.Join("\", \"", lPickMaps));

            // == DISREGARDED ==
            var disregardedVotes = (
                from p in AnalyzerConstants.MapPool
                join v in allVotes on p equals v.Map.Name into vp
                from v in vp.DefaultIfEmpty(new Vote(AnalyzerConstants.TeamSide.Unknown, AnalyzerConstants.VoteType.Unknown, new Map(p)))
                where v.Type != AnalyzerConstants.VoteType.Unknown && v.Team != AnalyzerConstants.TeamSide.Unknown && v.Map.Name == p
                group v by p into g
                select new { name = g.Key, count = matches.Count - g.Count()}
                ).ToDictionary( t => t.name, t => t.count );
            var lDisrMaps = disregardedVotes.Keys;
            var lDisrCount = disregardedVotes.Values;

            html = html
                .Replace("/*:disregarded.dataset:*/", string.Join(", ", lDisrCount))
                .Replace("/*:disregarded.labels:*/", string.Join("\", \"", lDisrMaps));

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            try
            {
                doc.Save(fileName, Encoding.UTF8);
                return true;
            }
            catch(Exception x)
            {
                ErrorHandling.Error($"Could not save HTML File: { x.Message }");
                return false;
            }
        }

        private static Dictionary<string, int> GetAllVotesByType(IEnumerable<Vote> votes, AnalyzerConstants.VoteType type)
        {
            var votesByType = (from v in votes
                       where v.Type == type
                       group v by v.Map.Name into g
                       orderby g.Key
                       select g).ToDictionary( t => t.Key, t => t.Count() );

            return votesByType;
        }
    }
}
