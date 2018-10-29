using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinetyNineLibrary
{
    public static class ErrorHandling
    {
        private static List<string> errors = new List<string>();
        private static string joinedString = string.Empty;
        private static bool hasErrors = false;

        public static string JoinedString { get => joinedString; private set => joinedString = value; }
        public static bool HasErrors { get => hasErrors; private set => hasErrors = value; }

        public static void Error(string strError)
        {
            hasErrors = true;
            errors.Add(strError);
            Log(strError);

            if (joinedString.Length > 0)
                joinedString += "\r\n";

            joinedString += strError;
        }

        public static void Clear()
        {
            joinedString = string.Empty;
            hasErrors = false;
            errors.Clear();
        }

        public static void Log(string strError)
        {
            // todo
        }
    }
}
