using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContParser
{
    internal class Autor 
    {
        public string Name { get; set; }

        public string RootUrl { get; set; }

        public int LastCachedPost { get; set; } = -1;

        public List<Post> Posts { get; } = new List<Post>();


        /// <inheritdoc />
        public override string ToString() 
        {
            return $"{Name} | {RootUrl} | { LastCachedPost }";
        }


        public static Autor Parse(string line) 
        {
            var split = line.Split('|');
            var author = new Autor
            {
                Name = split[0].Trim(),
                RootUrl = split[1].Trim(),
                LastCachedPost = int.Parse(split[2].Trim())
            };

            return author;
        }

        public static string ToLine(Autor autor) 
        {
            return $"{autor.Name} | {autor.RootUrl} | {autor.LastCachedPost}";
        }
    }
}