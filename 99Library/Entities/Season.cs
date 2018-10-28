using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Season : Entity
    {
        private string name;
        private int number;

        public string Name { get => name; set => name = value; }
        public int Number { get => number; set => number = value; }

        public Season(HtmlNode teamRow)
        {
            node = teamRow;
        }

        public override bool ParseInfo()
        {
            var anchor = node.SelectSingleNode("td[1]/a");
            name = anchor.InnerText; // "99Damage Liga<br>Saison 10"
            url = anchor.GetAttributeValue("href", ""); // doesn't really matter if we don't find the season link. yet?

            var numberMatch = Regex.Match(name, @"Saison ([\d]{1,})");
            if (!numberMatch.Success)
                return false;

            number = Convert.ToInt32(numberMatch.Groups[1].Value);

            return true;
        }
    }
}
