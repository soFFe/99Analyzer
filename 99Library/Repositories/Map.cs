using System;
using System.Collections.Generic;

namespace NinetyNineLibrary.Repositories
{
    public class Map : IRepository
    {
        public string Name { get; set; }
        public Dictionary<AnalyzerConstants.TeamSide, int> Score { get; set; }

        public Map(string name)
        {
            Name = name;
            Score = new Dictionary<AnalyzerConstants.TeamSide, int>();
        }

        public Map(string name, Dictionary<AnalyzerConstants.TeamSide, int> score)
        {
            Name = name;
            Score = score;
        }

        public string ToHTML()
        {
            throw new NotImplementedException();
        }
    }
}