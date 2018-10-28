using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NinetyNineLibrary.Entities
{
    public abstract class Entity
    {
        protected HtmlNode node;

        protected string url;

        public string Url { get => url; set => url = value; }

        public abstract bool ParseInfo();
    }
}
