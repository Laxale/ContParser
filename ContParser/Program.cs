using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContParser
{
    using System.IO;
    using System.Net.Http;
    using System.Windows;


    internal class Program 
    {
        private static readonly int maxPostIndex = 1000000;
        private static readonly string authorsParameter = "a";
        private static readonly string authorsFileName = "authors";
        private static readonly string postsFileName = "posts";
        private static readonly string imagesFileName = "images";
        private static readonly List<Autor> autors = new List<Autor>();
        private static readonly List<Post> posts = new List<Post>();


        [STAThread]
        static void Main(string[] args) 
        {
            try
            {
                Console.WriteLine("Started");

                EnsureStorages();
                ReadAutors();
                ReadPosts();
                ProcessInput();

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                StoreCaches();

                Console.WriteLine("Finished");
            }
        }

        private static void StoreCaches() 
        {
            StoreAutorCache();

            var postLines = posts.Select(Post.ToLine);

            File.WriteAllLines(postsFileName, postLines);
        }

        static void StoreAutorCache() 
        {
            var autorLines = autors.Select(Autor.ToLine);

            File.WriteAllLines(authorsFileName, autorLines);
        }

        private static void ReadAutors() 
        {
            string[] authorLines = File.ReadAllLines(authorsFileName);
            if (!authorLines.Any())
            {
                Console.WriteLine("No authors yet");
            }
            else
            {
                Console.WriteLine("Got autors:");
                foreach (string authorLine in authorLines)
                {
                    Autor autor = Autor.Parse(authorLine);
                    autors.Add(autor);
                }
            }
        }

        static void ReadPosts() 
        {
            string[] postLines = File.ReadAllLines(postsFileName);
            if (!postLines.Any())
            {
                Console.WriteLine("No posts yet");
            }
            else
            {
                Console.WriteLine("Got some posts");
                foreach (string postLine in postLines)
                {
                    Post post = Post.Parse(postLine);
                    posts.Add(post);

                    var postAutor = autors.First(autor => autor.Name == post.PostAutor.Name);
                    post.PostAutor = postAutor;
                    postAutor.Posts.Add(post);
                }
            }
        }

        static void ProcessInput() 
        {
            Console.WriteLine("Input autor root url");
            string url = Console.ReadLine();

            Autor autor = autors.FirstOrDefault(aut => aut.RootUrl == url);
            if (autor == null)
            {
                autor = new Autor
                {
                    RootUrl = url,
                    Name = url.Split('/').First(subLine => subLine.Contains('@')).Replace("@", string.Empty)
                };

                autors.Add(autor);
            }

            Console.WriteLine($"Input autor: { autor }");
            Console.WriteLine("Press p to get autor posts");

            if (Console.ReadLine() == "p")
            {
                ReadAutorPosts(autor);
            }
        }

        static void EnsureStorages() 
        {
            EnsureStorage(authorsFileName);
            EnsureStorage(postsFileName);
            EnsureStorage(imagesFileName);
        }

        static void EnsureStorage(string fileName) 
        {
            if (!File.Exists(fileName))
            {
                using (File.Create(fileName)) { }

                Console.WriteLine($"Created storage '{ fileName }'");
            }
            else
            {
                Console.WriteLine($"Found storage '{ fileName }'");
            }
        }



        static void ReadAutorPosts(Autor autor) 
        {
            Console.WriteLine($"Reading posts of '{ autor }'");

            int lastCachedPostNumber = 
                autor.LastCachedPost == -1 ?
                    1 :
                    autor.LastCachedPost;

            Console.WriteLine($"Starting from post { lastCachedPostNumber }");

            using (var client = new HttpClient())
            {
                for (int index = lastCachedPostNumber - 1; index < maxPostIndex; index++)
                {
                    string postUrl = $"{autor.RootUrl}/{index}";
                    var result = client.GetStringAsync(postUrl).Result;

                    autor.LastCachedPost = index;
                    StoreAutorCache();

                    if (result.Contains("404 ошибка"))
                    {
                        Console.WriteLine($"Post { postUrl } not found. Continuing");
                    }
                    else
                    {
                        var resultLines = result.Split(new []{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        var titleLine = resultLines.First(line => line.Contains("<title>"));
                        if (titleLine.Contains("Платформа для социальной журналистики"))
                        {
                            Console.WriteLine($"Found root redirect on { postUrl }. Continuing");
                        }
                        else
                        {
                            Console.WriteLine($"Found post on { postUrl }");
                            Clipboard.SetText(result);
                            throw new Exception();
                            //autor.Posts.Add();
                        }
                    }
                }
            }
        }
    }
}