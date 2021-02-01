using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amicitia.github.io.Program;

namespace Amicitia.github.io.PageCreator
{
    public class Page
    {
        public static void DeleteExisting(string indexPath)
        {
            //Remove each of these folders (and their files) from exe directory
            string[] thingsToDelete = new string[] { "author", "cheats", "game", "guides", "index", "mods", "post", "tag", "tools" };

            foreach (var thing in thingsToDelete)
            {
                foreach (var file in Directory.GetFiles(indexPath, "*.*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(thing + ".html"))
                        File.Delete(file);
                }
                foreach (var dir in Directory.GetDirectories(indexPath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir) == thing)
                        Directory.Delete(dir, true);
                }
            }
        }

        public static void CreateSingle(List<Post> posts)
        {
            //Create single-post pages for hyperlinks
            foreach (var post in posts)
            {
                List<Post> singlePost = new List<Post>() { post };
                CreateHtml(singlePost, $"post\\{post.Hyperlink}");
            }
        }

        public static void Create(string content, string url, int pageNumber, bool morePages)
        {
            //Html Head Tag contents
            string html = Properties.Resources.IndexHeader;
            //Top of page, site navigation
            string pageName = "";

            foreach (var split in url.Split('\\'))
            {
                if (split == "mods" || split == "tools" || split == "guides" || split == "cheats")
                {
                    //html += $" ► <a href=\"https://amicitia.github.io/{split}\">{split}</a>";
                    pageName += $" {split.Replace(".html", "")}";
                }
                else if (gameList.Any(g => g.Equals(split)))
                {
                    //html += $" ► <a href=\"https://amicitia.github.io/game/{split}\">{split.ToUpper()}</a>";
                    pageName += $" {split.Replace(".html", "")}";
                }
                else if (split != "index")
                {
                    //html += $" ► {split.Replace(".html", "")}";
                    pageName += $" {split.Replace(".html", "")}";
                }
            }
            //Change page title
            if (!String.IsNullOrEmpty(pageName))
                html = html.Replace("Amicitia</title>", $"Amicitia -{pageName}</title>");

            //Closing header div before content
            html += Properties.Resources.IndexSidebar;

            //Set up for pagination and ref link depth
            int depth = url.Count(c => c == '\\');
            string url2 = url.Replace(".html", "");
            if (depth == 1)
                url2 = url2.Replace($"{url}\\{url}", $"{url}");
            //Table for pagination
            string paginaion = "<table><tr>";
            //Previous Page
            if (pageNumber > 1)
            {
                if (pageNumber == 2)
                    paginaion += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", "")}\"><div class=\"unhide\"><i class=\"fa fa-angle-double-left\"></i> Previous Page</div></a></td>";
                else
                    paginaion += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", $"\\{pageNumber - 1}")}\"><div class=\"unhide\"><i class=\"fa fa-angle-double-left\"></i> Previous Page</div></a></td>";
            }
            else
                paginaion += "<td></td>";
            //Next Page
            if (morePages)
            {
                if (pageNumber == 1)
                    paginaion += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2 + $"\\{pageNumber + 1}"}\"><div class=\"unhide\">Next Page <i class=\"fa fa-angle-double-right\"></i></div></a></td>";
                else
                    paginaion += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", $"\\{pageNumber + 1}")}\"><div class=\"unhide\">Next Page <i class=\"fa fa-angle-double-right\"></i></div></a></td>";
            }
            else
                paginaion += "<td></td>";
            //End pagination table
            paginaion += "</tr></table><br>";

            //Append content header, navigation and footer to content
            html += paginaion; //Top of page navigation
            html += content; //Body Content
            html += paginaion; //Bottom of page navigation
            html += Properties.Resources.IndexFooter; //Footer
            html = html.Replace("ShrineFox 2020 - 2021.", $"ShrineFox 2020 - {DateTime.Now.Year}. Last updated {DateTime.Now.Month}/{DateTime.Now.Day}/{DateTime.Now.Year}. <a href=\"https://github.com/Amicitia/Amicitia.github.io\"><i class=\"fa fa-github\"></i> Source available on Github</a>.<br><a href=\"https://twitter.com/AmicitiaTeam\"><i class=\"fa fa-twitter\"></i> Follow</a> for updates!");

            //Replace links based on depth
            if (depth == 1)
            {
                html = html.Replace("\"css", "\"../css");
                html = html.Replace("\"js", "\"../js");
                html = html.Replace("\"images", "\"../images");
            }
            else if (depth == 2)
            {
                html = html.Replace("\"../", "\"../../");
                html = html.Replace("\"css", "\"../../css");
                html = html.Replace("\"js", "\"../../js");
                html = html.Replace("\"images", "\"../../images");
            }
            else if (depth == 3)
            {
                html = html.Replace("\"../../", "\"../../../");
                html = html.Replace("\"css", "\"../../../css");
                html = html.Replace("\"js", "\"../../../js");
                html = html.Replace("\"images", "\"../../../images");
            }

            //Auto-select game and/or type from dropdown
            foreach (var game in gameList)
                if (url.Contains($"\\{game}"))
                    html = html.Replace($"option value=\"{game.ToUpper()}\"", $"option value=\"{game.ToUpper()}\" selected");
            if (url.Contains("mods"))
                html = html.Replace($"option value=\"Mod\"", "option value=\"Mod\" selected");
            if (url.Contains("tools"))
                html = html.Replace($"option value=\"Tool\"", "option value=\"Tool\" selected");
            if (url.Contains("guides"))
                html = html.Replace($"option value=\"Guide\"", "option value=\"Guide\" selected");
            if (url.Contains("cheats"))
                html = html.Replace($"option value=\"Cheat\"", "option value=\"Cheat\" selected");

            //Create page
            string htmlPath = Path.Combine(indexPath, url);
            Directory.CreateDirectory(Path.GetDirectoryName(htmlPath));
            File.WriteAllText(htmlPath, html);
            Console.WriteLine(htmlPath);
        }

        internal static void FlowscriptDocs(string indexPath)
        {
            foreach (string page in new string[] { "compiling", "decompiling", "flowscript", "hookingfunctions", "importing", "messagescript" })
            {
                string content = "";
                content += Properties.Resources.FlowscriptHeader; //Head tags, static body content
                content += File.ReadAllText(Path.Combine(Path.Combine(indexPath, "Templates"), page + ".html"));
                content += Properties.Resources.FlowscriptFooter; //Static footer content
                File.WriteAllText(Path.Combine(Path.Combine(indexPath, "docs"), page + ".html"), content);
            }
            
        }

        public static void CreateHtml(List<Post> postpost, string url)
        {
            string content = "";
            int pages = 1;
            int pagePosts = 0;
            //For each post...
            for (int i = 0; i < postpost.Count; i++)
            {
                //Start of page
                if (pagePosts == 0)
                {
                    content += Properties.Resources.IndexContentHeader; //Head tags, static body content
                    //Show total number of results
                    if (!url.Contains("\\post\\"))
                        content += $"({postpost.Count} results)<br>";
                    if (postpost.Count == 0) //Inform user if no posts found
                        content += $"<br><center>Sorry! No posts matching your query were found. Please check again later.</center>";
                    else if (url.Contains("p5r")) //Show Pan thank you message
                        content += "<center>Special thanks to <a href=\"https://twitter.com/regularpanties\">@regularpanties</a> for the generous donation of a 6.72 PS4<br>and a plethora of documentation that made this section possible.</center><br>";

                    bool matchFound = false; //Show more resources if post is a mod or tool
                    if (!matchFound && (url.Contains("mods") || url.Contains("game")))
                    {
                        if (url.Contains("\\p5") && !url.Contains("\\p5r"))
                            content += "<br><center>To learn how to run P5 mods, see <a href=\"https://amicitia.github.io/post/p5-rpcs3-setupguide\">this guide.</a></center>";
                        else if (url.Contains("\\p5r"))
                            content += "<br><center>To learn how to install and run P5R mods, see <a href=\"https://cdn.discordapp.com/attachments/473138169592938526/757957969823400029/CriPakGUI-P5R.7z\">this guide</a>.";
                        else if (url.Contains("\\p3fes") || url.Contains("\\p4.html") || url.Contains("\\smt3.html"))
                            content += "<br><center>To learn how to run these mods, see <a href=\"https://amicitia.github.io/post/hostfs-guide\">this guide.</a></center>";
                        else if (url.Contains("\\p4g"))
                            content += "<br><center>To learn how to mod the PC version of P4G, see <a href=\"https://gamebanana.com/tuts/13379\">this guide.</a><br>More P4G PC mods available at <a href=\"https://gamebanana.com/games/8263\">gamebanana.com</a>.</center>";
                        else
                            content += "<br><center>To learn how to use these mods, see <a href=\"https://amicitia.github.io/post/p5-rpcs3-modcreationguide\">this guide.</a></center>";
                        matchFound = true;
                    }
                }

                //Add content to page after header
                if (url.Contains("post"))
                    content += Post.Write(postpost[i], true);
                else
                    content += Post.Write(postpost[i], false);

                pagePosts++;
                //End of page, create new page
                if (pagePosts == maxPosts || postpost.Count - 1 == i)
                {
                    pagePosts = 0;
                    if (pages == 1)
                        Create(content, $"{url}.html", pages, postpost.Count - (pages * maxPosts) > 0);
                    else
                        Create(content, $"{url}\\{pages}.html", pages, postpost.Count - (pages * maxPosts) > 0);
                    content = "";

                    pages++;
                }
            }
        }

        public static void CreateGames(List<Post> posts)
        {
            //For each game...
            foreach (var game in gameList)
            {
                //Get games from each post
                List<Post> postsByGame = new List<Post>();
                foreach (var post in posts)
                {
                    if (post.Games.Any(x => x.ToUpper().Equals(game.ToUpper())))
                        postsByGame.Add(post);
                }
                //Create 
                CreateHtml(postsByGame, $"game\\{game}");
            }
        }

        public static void CreateAuthors(List<Post> posts)
        {
            //Get list of individual authors from all posts
            List<string> uniqueAuthors = new List<string>();
            foreach (var post in posts)
            {
                foreach (var author in post.Authors)
                    if (!uniqueAuthors.Contains(author.Trim()))
                        uniqueAuthors.Add(author.Trim());
            }

            //Create individual pages for each unique creator
            foreach (var author in uniqueAuthors)
            {
                var newpost = posts.Where(p => p.Authors.Any(x => x.Trim().Equals(author))).ToList();
                Page.CreateHtml(newpost, $"author\\{author}");
            }
        }

        public static void CreateTags(List<Post> posts)
        {
            //Get list of individual tags from all posts
            List<string> uniqueTags = new List<string>();
            foreach (var post in posts)
            {
                foreach (var tags in post.Tags)
                    if (!uniqueTags.Contains(tags.Trim()))
                        uniqueTags.Add(tags.Trim());
            }

            //Create individual pages for each unique creator
            foreach (var tag in uniqueTags)
            {
                var newpost = posts.Where(p => p.Tags.Any(x => x.Trim().Equals(tag))).ToList();
                Page.CreateHtml(newpost, $"tag\\{tag}");
            }
        }

        public static void CreateType(string type)
        {
            //Get list of all post matching type
            List<Post> typepost = posts.Where(p => p.Type.Equals(type)).ToList();

            //Create page with all posts matching type
            Page.CreateHtml(typepost, type.ToLower() + "s");
            foreach (var game in gameList)
            {
                List<Post> typepostByGame = new List<Post>();
                foreach (var post in typepost)
                {
                    if (post.Games.Any(x => x.Trim().ToUpper().Equals(game.ToUpper())))
                        typepostByGame.Add(post);
                }
                CreateHtml(typepostByGame, $"{type.ToLower()}s\\{game}");
            }
        }
    }
}
