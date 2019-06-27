using System;
using System.Collections.Generic;
using NinetyNineLibrary.Entities;

namespace NinetyNineLibrary
{
    public static class AnalyzerConstants
    {
        public const string ProjectName = "99Analyzer";
        public const string Version = "1.0";
        public const string WindowTitle = ProjectName + " v" + Version;
        public const string UserAgent = ProjectName;

        public const string TeamUrlPattern = @"^https?:\/\/csgo\.99damage\.de\/de\/leagues\/teams\/.+$";
        public const string MatchUrlPattern = @"^https?:\/\/csgo\.99damage\.de\/de\/leagues\/matches\/.+$";
        public const string DivisonOverviewThirdColumnTeamNamePattern = @"^vs\.\s+(.+)$";
        public const string MapvotePattern = @"^(T[12]) (bans|picks) (.+)$";

        public enum TeamSide { A, B, Unknown };
        public enum MatchStatus { NotPlayed, DefWin, Played };
        public enum VoteType { Ban, Pick, Unknown };

        public const string HtmlTemplatePath = "Templates/HtmlTemplate.html";

        public static readonly Dictionary<int, List<string>> MapPool = new Dictionary<int, List<string>>
        {
            // Season 6 is the first season to use the matchlog
            {
                6,
                new List<string>
                {
                    "de_cache",
                    "de_inferno",
                    "de_mirage",
                    "de_nuke",
                    "de_overpass",
                    "de_train",
                    "de_cbble"
                }
            },

            // Since Season 9: dust2 instead of cbble
            {
                9,
                new List<string>
                {
                    "de_cache",
                    "de_inferno",
                    "de_mirage",
                    "de_nuke",
                    "de_overpass",
                    "de_train",
                    "de_dust2"
                }
            },

            // Since Season 12: vertigo instead of cache
            {
                12,
                new List<string>
                {
                    "de_vertigo",
                    "de_dust2",
                    "de_inferno",
                    "de_mirage",
                    "de_nuke",
                    "de_overpass",
                    "de_train"
                }
            }
        };
    }
}