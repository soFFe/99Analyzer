using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Season : Entity
    {
        public string Name { get; set; }
        public int Number { get; set; }

        public Season(HtmlNode teamRow)
        {
            node = teamRow;
        }

        public override bool ParseInfo()
        {
            var anchor = node.SelectSingleNode("td[1]/a");
            Name = anchor.InnerText; // "99Damage Liga<br>Saison 10"
            url = anchor.GetAttributeValue("href", ""); // doesn't really matter if we don't find the season link. yet?

            var numberMatch = Regex.Match(Name, @"Saison ([\d]{1,})");
            if (!numberMatch.Success)
                return false;

            Number = Convert.ToInt32(numberMatch.Groups[1].Value);

            return true;
        }

        public override string ToString()
        {
            return Number.ToString();
        }
    }
}
