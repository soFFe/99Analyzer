using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Division : Entity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Group { get; set; }

        public Division(HtmlNode teamRow)
        {
            node = teamRow;
        }

        public override bool ParseInfo()
        {
            var anchor = node.SelectSingleNode("td[2]/a");
            Name = anchor.InnerText; // "Starter Division, Starter 11" / "Division 5, Division 5.24"

            var splitName = Name.Split(',');
            Type = splitName[0];
            Group = splitName[1].Trim();
            url = anchor.GetAttributeValue("href", "");

            if (string.IsNullOrEmpty(url))
                return false;

            return true;
        }
    }
}