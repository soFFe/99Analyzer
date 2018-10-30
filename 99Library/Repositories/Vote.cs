using System;

namespace NinetyNineLibrary.Repositories
{
    public class Vote : IRepository
    {
        public AnalyzerConstants.TeamSide Team { get; set; }
        public AnalyzerConstants.VoteType Type { get; set; }
        public Map Map { get; set; }

        public Vote(AnalyzerConstants.TeamSide team, AnalyzerConstants.VoteType type, Map map)
        {
            Team = team;
            Type = type;
            Map = map;
        }

        public string ToHTML()
        {
            throw new NotImplementedException();
        }
    }
}