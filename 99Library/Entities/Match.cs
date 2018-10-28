using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public class Match : Entity
    {
        public Match(HtmlNode divisionRow)
        {

        }

        public override bool ParseInfo()
        {
            return true;
        }
    }
}
