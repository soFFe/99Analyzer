using System;
using System.Threading.Tasks;
using NinetyNineLibrary;

namespace NinetyNineAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            ErrorHandling.Log("--- Started 99Analyzer ---");
            Console.Title = AnalyzerConstants.WindowTitle;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} v{1}", AnalyzerConstants.ProjectName, AnalyzerConstants.Version);
            Console.Write("Team URL: ");
            Console.ResetColor();

            var strUrl = Console.ReadLine();

            var task = Task.Run(() => NinetyNineParser.Parse(strUrl));
            Console.ReadLine();
        }
    }
}
