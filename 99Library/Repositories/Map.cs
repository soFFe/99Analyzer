using System;

namespace NinetyNineLibrary.Repositories
{
    public class Map : IRepository
    {
        public string Name { get; set; }

        public Map(string name)
        {
            Name = name;
        }

        public string ToHTML()
        {
            throw new NotImplementedException();
        }
    }
}