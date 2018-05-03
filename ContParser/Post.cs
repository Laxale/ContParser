using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContParser
{
    internal class Post 
    {
        public string Name { get; set; }

        public Autor PostAutor { get; set; }

        public List<string> ImageUrls { get; } = new List<string>();


        public static Post Parse(string line) 
        {
            var split = line.Split(';');

            var post = new Post
            {
                Name = split[0].Trim(),
                PostAutor = new Autor
                {
                    Name = split[1].Trim()
                }
            };

            return post;
        }

        public static string ToLine(Post post) 
        {
            return $"{post.Name}; {post.PostAutor.Name}";
        }
    }
}