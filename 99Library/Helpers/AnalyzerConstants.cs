namespace NinetyNineLibrary
{
    public static class AnalyzerConstants
    {
        public const string Version = "1.0";
        public const string WindowTitle = "99Damage Analyzer v" + Version;
        public const string UserAgent = "99Analyzer";

        public const string TeamUrlPattern = @"^https?:\/\/csgo\.99damage\.de\/de\/leagues\/teams\/.+$";
        public const string MatchUrlPattern = @"^https?:\/\/csgo\.99damage\.de\/de\/leagues\/matches\/.+$";
        public const string DivisonOverviewThirdColumnTeamNamePattern = @"^vs\.\s+(.+)$";
        public const string MapvotePattern = @"^(T[12]) (bans|picks) (.+)$";

        public enum TeamSide { A, B, Unknown };
        public enum MatchStatus { NotPlayed, DefWin, Played };
        public enum VoteType { Ban, Pick, Unknown };
    }
}