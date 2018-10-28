using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Division : Entity
    {
        private string name;
        private string type;
        private string group;

        public string Name { get => name; set => name = value; }
        public string Type { get => type; set => type = value; }
        public string Group { get => group; set => group = value; }

        public Division(HtmlNode teamRow)
        {
            node = teamRow;
        }

        public override bool ParseInfo()
        {
            var anchor = node.SelectSingleNode("td[2]/a");
            name = anchor.InnerText; // "Starter Division, Starter 11" / "Division 5, Division 5.24"

            var splitName = name.Split(',');
            type = splitName[0];
            group = splitName[1].Trim();
            url = anchor.GetAttributeValue("href", "");

            if (string.IsNullOrEmpty(url))
                return false;

            return true;
        }
    }
}