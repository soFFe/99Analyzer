using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinetyNineLibrary
{
    public static class ErrorHandling
    {
        private static List<string> errors = new List<string>();

        public static string JoinedString { get; private set; } = string.Empty;
        public static bool HasErrors { get; private set; } = false;

        public static void Error(string strError)
        {
            HasErrors = true;
            errors.Add(strError);
            Log(strError);

            if (JoinedString.Length > 0)
                JoinedString += "\r\n";

            JoinedString += strError;
        }

        public static void Clear()
        {
            JoinedString = string.Empty;
            HasErrors = false;
            errors.Clear();
        }

        public static void Log(string strError)
        {
            Directory.CreateDirectory("log");

            using (var sw = File.AppendText("log/99Analyzer.log"))
            {
                var dt = DateTime.Now;
                sw.WriteLine("[" + dt.ToString("u") + "] " + strError);
            }
        }

        public static void WriteToConsole()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(JoinedString + "\n");
            Console.ResetColor();

            Clear();
        }
    }
}
