using MoreLinq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Amicitia.github.io.PageCreator;

namespace Amicitia.github.io
{
    class Program
    {
        public static string indexPath; //Path to website root directory
        public static List<string> gameList = new List<string>() { "p3fes", "p4", "p5", "p5r", "p4g", "p3p", "p3d", "p4d", "p5d", "pq", "pq2", "p4au", "smt3", "cfb" }; //Games in dropdown
        public static List<string> tagColors = new List<string>() { "F37E79", "F3BF79", "F3D979", "7AF379", "7998F3", "DE79F3" }; //Hex color values for tags
        public static List<Post> posts; //Posts
        public static int maxPosts = 15; //Number of posts per page

        static void Main(string[] args)
        {
            //Exe Directory
            indexPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            //Order post post from .tsv files by most recent)
            /*posts = Post.Get(indexPath).OrderBy(p => DateTime.Parse(p.Date, CultureInfo.CreateSpecificCulture("en-US"))).ToArray().Reverse().ToList();
            //Delete files if they exist already
            Page.DeleteExisting(indexPath);
            //Create main page with all mods, tools, guides and cheats
            Page.CreateHtml(posts, "index");

            //List all mods, tools, cheats and guides (per game as well)
            Page.CreateType("mod"); // amicitia.github.io/mods
            Page.CreateType("tool"); // amicitia.github.io/tools/p5
            Page.CreateType("cheat"); // amicitia.github.io/cheats/p4
            Page.CreateType("guide"); // amicitia.github.io/guides/p5r

            //Create pages for all content per game (regardless of type)
            Page.CreateGames(posts); //amicitia.github.io/game/p3fes

            //Searchable type ppostsages
            Page.CreateAuthors(posts); //amicitia.github.io/author/TGE
            Page.CreateTags(posts); //amicitia.github.io/tag/BF

            //All individual posts (hyperlinks)
            Page.CreateSingle(posts); //amicitia.github.io/post/amicitia

            //Create flowscript docs*/
            Page.FlowscriptDocs(indexPath);

            Console.WriteLine("Done!");
        }
    }
}
