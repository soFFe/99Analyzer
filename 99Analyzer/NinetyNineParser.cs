using NinetyNineLibrary;
using NinetyNineLibrary.Entities;
using NinetyNineLibrary.EntityModels;
using NinetyNineLibrary.Helpers;
using System;
using System.Collections.Generic;

namespace NinetyNineAnalyzer
{
    public class NinetyNineParser
    {
        public static async void Parse(string sUrl)
        {
            ErrorHandling.Clear();

            List<Match> matches = null;
            Team team = null;
            var tiSuccess = false;
            var miSuccess = false;
            try
            {
                team = await TeamModel.GetInstanceAsync(sUrl);
                tiSuccess = team.ParseInfo();

                if (!tiSuccess)
                {
                    ErrorHandling.Error("Could not parse the team information");
                }

                // Get all MatchURLs of our team for the divisions they played
                var matchUrls = new Dictionary<Season, List<string>>();
                foreach (var season in team.Seasons)
                {
                    if (season.Key.Number < 6)
                    {
                        // cannot parse matches reliably before season 6
                        continue;
                    }

                    var parsedMatchUrls = await MatchModel.ParseMatchLinksAsync(season, team.Shortname);
                    foreach (var url in parsedMatchUrls)
                    {
                        matchUrls.Add(url.Key, url.Value);
                    }
                }

                // Get Match infos
                matches = await MatchModel.ParseMatchesAsync(matchUrls);
                if (matches.Count == 0)
                {
                    ErrorHandling.Error("No valid matches found");
                    miSuccess = false;
                }

                miSuccess = true;
            }
            catch (Exception x)
            {
                ErrorHandling.Error(x.Message);
            }

            if (!tiSuccess || !miSuccess)
            {
                ErrorHandling.WriteToConsole();
            }
            else
            {
                // output
                var bOutput = HtmlOutput.CreateFile(team, matches, team.Name + ".html");
                if (bOutput)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("Successfully wrote ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(team.Name + ".html\n");
                    Console.ResetColor();
                }
                else
                {
                    ErrorHandling.Error("Could not write HTML File");
                    ErrorHandling.WriteToConsole();
                }
            }
        }
    }
}
