using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.CodeDom.Compiler;

namespace Amicitia.github.io
{
    class Program
    {
        //Data used on all pages
        public static string indexPath; //Path to website root directory
        public static List<string> gameList = new List<string>() { "p3fes", "p4", "p5", "p5r", "p4g", "p3p", "p3d", "p4d", "p5d", "pq", "pq2", "p4au", "smt3", "cfb" }; //Games in dropdown
        public static List<Tuple<string, int>> sortedAuthors = new List<Tuple<string, int>>(); //Authors in order of most contributions ever
        public static List<Tuple<string, int>> sortedTags = new List<Tuple<string, int>>(); //Tags in order of most contributions ever
        public static List<Tuple<string, int>> sortedAuthorsMonthly = new List<Tuple<string, int>>(); //Authors in order of most amount of recent contributions
        public static int maxPosts = 15; //Number of posts per page
        public static List<PostInfo> data; //Posts

        static void Main(string[] args)
        {
            //Set index path to application root directory (where the exe is run from)
            indexPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            //Get all post data from .tsv files (ordered by most recent)
            data = GetData(indexPath).OrderBy(d => DateTime.Parse(d.Date, CultureInfo.CreateSpecificCulture("en-US"))).ToArray().Reverse().ToList();
            //Sort Top Contributors and Tags
            sortedAuthors = SortAuthorsByPostCount();
            sortedAuthorsMonthly = SortAuthorsMonthly(2);
            sortedTags = SortTags();
            //Delete files if they exist already
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
            //Create main page with all mods, tools, guides and cheats
            CreateHtml(data, "index");
            //List all mods, tools, cheats and guides (per game as well)
            CreateTypePages("Mod"); // amicitia.github.io/mods
            CreateTypePages("Tool"); // amicitia.github.io/tools/p5
            CreateTypePages("Cheat"); // amicitia.github.io/cheats/p4
            CreateTypePages("Guide"); // amicitia.github.io/guides/p5r
            //Create pages for all content per game (regardless of type)
            CreateGamePages(); //amicitia.github.io/game/p3fes
            //Searchable type pages
            CreateAuthorPages(); //amicitia.github.io/author/TGE
            CreateTagPages(); //amicitia.github.io/tag/BF
            //All individual posts (hyperlinks)
            CreatePostPages(); //amicitia.github.io/post/amicitia

            Console.WriteLine("Done!");
        }

        private static List<string> GetAuthors(PostInfo post)
        {
            List<string> authors = new List<string>();
            string[] splitAuthors = post.Author.Split(',');
            foreach (var author in splitAuthors)
            {
                if (author != "Unknown Author" && !string.IsNullOrWhiteSpace(author))
                {
                    if (!authors.Any(x => x.Equals(author.Trim())))
                        authors.Add(author.Trim());
                }
            }
            
            //Return list of authors from post
            return authors;
        }

        private static List<Tuple<string, int>> SortAuthorsByPostCount()
        {
            List<Tuple<string, int>> authors = new List<Tuple<string, int>>();
            foreach (PostInfo post in data)
            {
                string[] splitAuthors = post.Author.Split(',');
                //Add to authors list or increase count
                foreach (var author in splitAuthors)
                {
                    if (author != "Unknown Author" && !string.IsNullOrWhiteSpace(author))
                    {
                        //Add author to list with 1 contribution if not already in list
                        if (!authors.Any(x => x.Item1.Equals(author.Trim())))
                            authors.Add(new Tuple<string, int>(author.Trim(), 1));
                        else
                        {
                            //Increase count for author if already found
                            int index = authors.IndexOf(authors.First(x => x.Item1.Equals(author.Trim())));
                            authors[index] = new Tuple<string, int>(author.Trim(), authors[index].Item2 + 1);
                        }
                    }
                }
            }

            //Return reversed list (most to least contributions) of authors
            return authors.OrderBy(x => x.Item2).Reverse().ToList();
        }

        private static List<Tuple<string, int>> SortAuthorsMonthly(int months)
        {
            List<Tuple<string, int>> authors = new List<Tuple<string, int>>();
            foreach (PostInfo post in data)
            {
                string[] splitAuthors = post.Author.Split(',');
                //If post is within the specified number of months...
                if (DateTime.Compare(DateTime.Now.AddDays(-30 * months), DateTime.Parse(post.Date, CultureInfo.CreateSpecificCulture("en-US"))) <= 0)
                {
                    //Add to authors list or increase count
                    foreach (var author in splitAuthors)
                    {
                        if (author != "Unknown Author" && author != "")
                        {
                            if (!authors.Any(x => x.Item1.Equals(author.Trim())))
                                authors.Add(new Tuple<string, int>(author.Trim(), 1));
                            else
                            {
                                int index = authors.IndexOf(authors.First(x => x.Item1.Equals(author.Trim())));
                                authors[index] = new Tuple<string, int>(author.Trim(), authors[index].Item2 + 1);
                            }
                        }
                    }
                }
            }
            return authors.OrderBy(x => x.Item2).Reverse().ToList();
        }

        private static List<Tuple<string, int>> SortTags()
        {
            List<Tuple<string, int>> tags = new List<Tuple<string, int>>();
            foreach (PostInfo post in data)
            {
                string[] splitTags = post.Tags.Split(',');
                //Add to authors list or increase count
                foreach (var tag in splitTags)
                {
                    if (!tags.Any(x => x.Item1.Equals(tag.Trim())))
                        tags.Add(new Tuple<string, int>(tag.Trim(), 1));
                    else
                    {
                        int index = tags.IndexOf(tags.First(x => x.Item1.Equals(tag.Trim())));
                        tags[index] = new Tuple<string, int>(tag.Trim(), tags[index].Item2 + 1);
                    }
                }
            }
            return tags.OrderBy(x => x.Item2).Reverse().ToList();
        }

        private static void CreateGamePages()
        {
            //Create single-post pages for hyperlinks
            foreach (var game in gameList)
            {
                List<PostInfo> dataByGame = new List<PostInfo>();
                foreach (var post in data)
                {
                    string[] splitGames = post.Game.Split(',');
                    for (int i = 0; i < splitGames.Count(); i++)
                        splitGames[i] = splitGames[i].Trim();
                    if (splitGames.Any(x => x.ToUpper().Equals(game.ToUpper())))
                        dataByGame.Add(post);
                }
                CreateHtml(dataByGame, $"game\\{game}");
            }
        }

        private static void CreatePostPages()
        {
            //Create single-post pages for hyperlinks
            foreach (var post in data)
            {
                List<PostInfo> singlePost = new List<PostInfo>() { post };
                CreateHtml(singlePost, $"post\\{post.Hyperlink}");
            }
        }

        private static void CreateTagPages()
        {
            List<string> tagList = new List<string>();
            foreach (var post in data)
            {
                //Split and sanitize individual unique tags from all posts
                var tags = post.Tags.Split(',');
                foreach (var tag in tags)
                {
                    if (!tagList.Contains(tag.Trim()) && string.IsNullOrWhiteSpace(tag.Trim()))
                        tagList.Add(tag.Trim());
                }
            }

            //Create individual pages for unique tags
            foreach (var tag in tagList)
                CreateHtml(data.Where(p => p.Tags.Contains(tag)).ToList(), $"tag\\{tag}");
        }

        private static void CreateAuthorPages()
        {
            List<string> uniqueAuthors = new List<string>();
            foreach (var post in data)
            {
                //Split multiple authors per post and sanitize
                string[] splitAuthors = post.Author.Split(',');
                for (int i = 0; i < splitAuthors.Count(); i++)
                    splitAuthors[i] = Sanitize(splitAuthors[i]);

                //Add to list of unique authors
                foreach (var author in splitAuthors)
                    if (!uniqueAuthors.Contains(author) && !string.IsNullOrEmpty(author))
                        uniqueAuthors.Add(author);
            }

            //Create individual pages for each unique creator
            foreach (var author in uniqueAuthors)
            {
                var newData = data.Where(p => p.Author.Contains(author)).ToList();
                CreateHtml(newData, $"author\\{author}");
            }

        }

        private static void CreateTypePages(string type)
        {
            //Get list of all data matching type
            List<PostInfo> typeData = data.Where(p => p.Type.Equals(type)).ToList();

            //Create page with all posts matching type
            CreateHtml(typeData, type.ToLower() + "s");
            foreach (var game in gameList)
            {
                List<PostInfo> typeDataByGame = new List<PostInfo>();
                foreach (var post in typeData)
                {
                    string[] splitGames = post.Game.Split(',');
                    for (int i = 0; i < splitGames.Count(); i++)
                        splitGames[i] = splitGames[i].Trim();
                    if (splitGames.Any(x => x.ToUpper().Equals(game.ToUpper())))
                        typeDataByGame.Add(post);
                }
                CreateHtml(typeDataByGame, $"{type.ToLower()}s\\{game}");
            }
        }

        private static void CreateHtml(List<PostInfo> postData, string url)
        {
            string content = "";
            int pages = 0;
            int pagePosts = 0;
            //For each post...
            for (int i = 0; i < postData.Count; i++)
            {
                //Start of page
                if (pagePosts == 0)
                {
                    content += Properties.Resources.IndexContentHeader; //Head tags, static body content
                    //Show total number of results
                    if (!url.Contains("\\post\\"))
                        content += $"{url.Split('\\').Last()} ({postData.Count} results)<br>";
                    if (postData.Count == 0) //Inform user if no posts found
                        content += $"<br><center>Sorry! No posts matching your query were found. Please check again later.</center>";
                    else if (url.Contains("p5r")) //Show Pan thank you message
                        content += "<center>Special thanks to <a href=\"https://twitter.com/regularpanties\">@regularpanties</a> for the generous donation of a 6.72 PS4<br>and a plethora of documentation that made this section possible.<br><br>All P5R mods are for the JP version only due to firmware restraints.</center><br>";

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
                    content += WritePost(postData[i], false);
                else
                    content += WritePost(postData[i]);
                pagePosts++;

                //End of page, create new page
                if (pagePosts == maxPosts || postData.Count - 1 == i)
                {
                    pages++;
                    pagePosts = 0;
                    if (pages == 1)
                        CreatePage(content, $"{url}.html", pages, (postData.Count - i > 0));
                    else
                        CreatePage(content, $"{url}\\{pages}.html", pages, (postData.Count - i > 0));
                    content = "";
                }
            }
        }

        private static void CreatePage(string content, string url, int pageNumber, bool morePages)
        {
            /*
             *   Header and Navigation
             */

            //Top of page (head tags)
            string html = Properties.Resources.IndexHeader;
            string pageName = "";
            //Top of content navigation
            foreach (var split in url.Split('\\'))
            {
                if (split == "mods" || split == "tools" || split == "guides" || split == "cheats")
                {
                    html += $" ► <a href=\"https://amicitia.github.io/{split}\">{FirstLetterToUpperCase(split)}</a>";
                    pageName += $" {FirstLetterToUpperCase(split.Replace(".html", ""))}";
                }
                else if (gameList.Any(g => g.Equals(split)))
                {
                    html += $" ► <a href=\"https://amicitia.github.io/game/{split}\">{split.ToUpper()}</a>";
                    pageName += $" {FirstLetterToUpperCase(split.Replace(".html", ""))}";
                }
                else if (split != "index")
                {
                    html += $" ► {split.Replace(".html", "")}";
                    pageName += $" {split.Replace(".html", "")}";
                }
            }
            if (!String.IsNullOrEmpty(pageName))
                html = html.Replace("Amicitia</title>", $"Amicitia -{pageName}</title>");

            //Blog posts, top contributors, closing header div before content
            html += Properties.Resources.IndexSidebar;
            //Only show on non-individual post pages
            if (!url.Contains("post"))
            {
                //Top Contributors
                html += "<h3><i class=\"fas fa-users\"></i> Top Contributors</h3><br><table><tr><td style=\"padding: 0px;\">All Time</td><td style=\"padding: 0px;\">Past 2 Months</td></tr>";
                for (int i = 0; i < 10; i++)
                {
                    if (sortedAuthors.Count >= i && sortedAuthors[i].Item2 >= 2)
                        html += $"<tr><td style=\"padding: 0px;\">{i + 1}. <a href=\"https://amicitia.github.io/author/{sortedAuthors[i].Item1}\">{sortedAuthors[i].Item1}</a> ({sortedAuthors[i].Item2})</td>";
                    else
                        html += "<tr><td style=\"padding: 0px;\"></td>";
                    if (sortedAuthorsMonthly.Count >= i && sortedAuthorsMonthly[i].Item2 >= 2)
                        html += $"<td style=\"padding: 0px;\"><a href=\"https://amicitia.github.io/author/{sortedAuthorsMonthly[i].Item1}\">{sortedAuthorsMonthly[i].Item1}</a> ({sortedAuthorsMonthly[i].Item2})</td></tr>";
                    else
                        html += "<td style=\"padding: 0px;\"></td></tr>";
                }
                html += "</table>";
                //Popular Tags
                html += "<h3><i class=\"fas fa-tags\"></i> Popular Tags</h3><br>";
                int color = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (color == colors.Count())
                        color = 0;
                    if (sortedTags.Count >= i && sortedTags[i].Item2 >= 2)
                        html += $"<a href=\"https://amicitia.github.io/tag/{sortedTags[i].Item1}\"><div class=\"tag\" style=\"border-left: 4px solid #{colors[color++]}; \"><p class=\"noselect\">{sortedTags[i].Item1}</p></div></a>";
                }
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

            /*
             *   Pagination
             */

            //Set up for pagination and ref link depth
            int depth = url.Count(c => c == '\\');
            string url2 = url.Replace(".html","");
            if (depth == 1)
                url2 = url2.Replace($"{url}\\{url}",$"{url}");
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

            /*
             *   Page Building
             */

            //Append content header, navigation and footer to content
            html += paginaion; //Top of page navigation
            html += content; //Body Content
            html += paginaion; //Bottom of page navigation
            html += Properties.Resources.IndexFooter; //Footer
            html += $"2020 - {DateTime.Now.Year}. Last updated {DateTime.Now.Month}/{DateTime.Now.Day}/{DateTime.Now.Year}. <a href=\"https://github.com/Amicitia/Amicitia.github.io\"><i class=\"fa fa-github\"></i> Source available on Github</a>. <a href=\"https://twitter.com/AmicitiaTeam\"><i class=\"fa fa-twitter\"></i> Follow</a> for updates!</div></footer></html>";

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

            //Create page
            string htmlPath = Path.Combine(indexPath, url);
            Directory.CreateDirectory(Path.GetDirectoryName(htmlPath));
            File.WriteAllText(htmlPath, html);
            Console.WriteLine(htmlPath);
        }

        private static string WritePost(PostInfo data, bool toggle = true)
        {
            string result;
            List<string> splitGames = data.Game.Split(',').ToList();
            List<string> tagsList = data.Tags.Split(',').ToList();
            List<string> authorList = data.Author.Split(',').ToList();

            //Post Summary
            result = $"<div class=\"toggle\"><div class=\"toggle-title\"><table><tbody><tr><td width='25%'>";
            //Hyperlink
            result += $"<a href=\"https://amicitia.github.io/post/{data.Hyperlink}\"><div class=\"getlink\"><i class=\"fas fa-link\" aria-hidden=\"true\"></i></div></a>";
            //New Label
            if (DateTime.Compare(DateTime.Now.AddDays(-30), DateTime.Parse(data.Date, CultureInfo.CreateSpecificCulture("en-US"))) <= 0)
            {
                result += "<div class=\"ribbon\"><div class=\"new\">NEW</div></div>";
            }
            //Thumbnail
            if (data.Embed.Contains("youtu"))
            {
                string videoID = data.Embed.Substring(data.Embed.IndexOf("v=") + 2);
                string ytThumb = $"https://img.youtube.com/vi/{videoID}/default.jpg";
                result += $"<a href=\"{data.Embed}\"><img src=\"{ytThumb}\" height=\"auto\" width=\"auto\"></td>";
            }
            else if (data.Embed.Contains("streamable.com"))
            {
                result += $"<div style=\"width:100%;height:0px;position:relative;padding-bottom:56.250%;\"><iframe src=\"{data.Embed}\" frameborder=\"0\"width=\"100%\"height=\"100%\"allowfullscreen style=\"width:100%;height:100%;position:absolute;\"></iframe></div>";
            }
            else if (data.Embed != null && data.Embed.Trim() != "")
                result += $"<img src=\"{data.Embed}\" height=\"auto\" width=\"auto\"></td>";
            else
                result += "<img src=\"https://i.imgur.com/5I5Vos8.png\" height=\"auto\" width=\"auto\"></td>";

            //Visible Post Details
            result += $"<td width='50%'><font size=\"3\"><b><font size=\"5\">{data.Title}</font></b><br>{splitGames[0]} {data.Type} by ";
            //Author
            foreach (string author in authorList)
            {
                result += $"<a href=\"https://amicitia.github.io/author/{author.Trim()}\">{author.Trim()}</a>";
                if (authorList.IndexOf(author) != authorList.Count() - 1)
                {
                    result += ", ";
                }
            }
            result += $"</font></td><td width='25%'><center>{data.Date}</center></td></tr></tbody></table></div>";
            //Hidden Post Details
            if (toggle)
                result += $"<div class=\"toggle-inner\">";
            result += $"<div class=\"cheat\"><table><tbody><tr><td>{data.Description}";
            //Update
            if (!String.IsNullOrEmpty(data.UpdateText) && data.Type != "Cheat")
                result += $"<br><div class=\"update\">{data.UpdateText}</div>";

            result += "</td><td>";
            //Download
            if (data.Type == "Mod" || data.Type == "Tool")
            {
                result += $"<center><font size=\"4\"><a href=\"{data.DownloadURL}\"><i class=\"fas fa-{data.DownloadIcon}\" aria-hidden=\"true\"></i> Download {data.DownloadText}</a><br>";
                if (data.DownloadURL2 != "")
                    result += $"<a href=\"{data.DownloadURL2}\"><i class=\"fas fa-{data.DownloadIcon2}\" aria-hidden=\"true\"></i> Download {data.DownloadText2}</a><br>";
                if (data.SourceURL != "")
                    result += $"<a href=\"{data.SourceURL}\"><i class=\"fas fa-file-code\" aria-hidden=\"true\"></i> Source Code</a>";
                result += "</center></div>";
            }
            else if (data.Type == "Cheat")
                    result += $"<div id=\"cheat{data.ID}\" onclick=\"copyDivToClipboard('cheat{data.ID}')\"><div class=\"cheatcode\">{data.UpdateText}</div></div>";

            //Related Guides
            if (!String.IsNullOrEmpty(data.GuideURL))
                result += $"<br><center><a href=\"{data.GuideURL}\"><i class=\"fas fa-info-circle\" aria-hidden=\"true\"></i> {data.GuideText}</a></center>";
            //Thread Link
            if (!String.IsNullOrEmpty(data.ThreadURL))
                result += $"<center><a href=\"{data.ThreadURL}\"><i class=\"fas fa-comment\" aria-hidden=\"true\"></i> Feedback</a></center>";

            //End of table
            result += "</tr></tbody></table>";

            //Tags
            int color = 0;
            foreach (string tag in tagsList)
            {
                if (color == colors.Count())
                    color = 0;
                result += $"<a href=\"https://amicitia.github.io/tag/{tag.Trim()}\"><div class=\"tag\" style=\"border-left: 4px solid #{colors[color++]}; \"><p class=\"noselect\">{tag}</p></div></a>";
            }
            //End Entry
            result += "</div></div></div>";

            return result;
        }

        public class PostInfo
        {
            public PostInfo(int ID, string Type, string Title, string Game, string Author, float Version, string Date, string Tags, string Hyperlink, string Description, string Embed, string DownloadURL, string DownloadText, string DownloadIcon, string DownloadURL2, string DownloadText2, string DownloadIcon2, string UpdateText, string SourceURL, string GuideURL, string GuideText, string ThreadURL)
            {
                this.ID = ID;
                this.Type = Type;
                this.Title = Title;
                this.Game = Game;
                this.Author = Author;
                this.Version = Version;
                this.Date = Date;
                this.Tags = Tags;
                this.Hyperlink = Hyperlink;
                this.Description = Description;
                this.Embed = Embed;
                this.DownloadURL = DownloadURL;
                this.DownloadText = DownloadText;
                this.DownloadIcon = DownloadIcon;
                this.DownloadURL2 = DownloadURL2;
                this.DownloadText2 = DownloadText2;
                this.DownloadIcon2 = DownloadIcon2;
                this.UpdateText = UpdateText;
                this.SourceURL = SourceURL;
                this.GuideURL = GuideURL;
                this.GuideText = GuideText;
                this.ThreadURL = ThreadURL;
            }
            public int ID { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public string Game { get; set; }
            public string Author { get; set; }
            public float Version { get; set; }
            public string Date { get; set; }
            public string Tags { get; set; }
            public string Hyperlink { get; set; }
            public string Description { get; set; }
            public string Embed { get; set; }
            public string DownloadURL { get; set; }
            public string DownloadText { get; set; }
            public string DownloadIcon { get; set; }
            public string DownloadURL2 { get; set; }
            public string DownloadText2 { get; set; }
            public string DownloadIcon2 { get; set; }
            public string UpdateText { get; set; }
            public string SourceURL { get; set; }
            public string GuideURL { get; set; }
            public string GuideText { get; set; }
            public string ThreadURL { get; set; }
        }

        public static List<PostInfo> GetData(string indexPath)
        {
            data = new List<PostInfo>();
            int index = 0;
            foreach (var tsv in Directory.GetFiles($"{indexPath}\\db").Where(x => Path.GetExtension(x).Equals(".tsv")))
            {
                string type = FirstLetterToUpperCase(Path.GetFileNameWithoutExtension(tsv).Replace("s", ""));
                string[] tsvFile = File.ReadAllLines(tsv);
                for (int i = 1; i < tsvFile.Length; i++)
                {
                    //Separate tabs and remove whitespace/quotation marks
                    var split = tsvFile[i].Split('\t');
                    for (int x = 0; x < split.Count(); x++)
                        split[x] = Sanitize(split[x]);

                    if (split.Any(x => !String.IsNullOrEmpty(x)))
                    {
                        PostInfo pi = new PostInfo(0, "", "", "", "", 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                        if (type == "Mod" || type == "Tool")
                            pi = new PostInfo(index, type, split[1], split[2], split[3], Convert.ToSingle(split[4]), split[5], split[6], split[0], split[7], split[9], split[10], split[11], split[12], split[15], split[16], split[17], split[8], split[13], split[18], split[19], split[14]);
                        else if (type == "Cheat")
                            pi = new PostInfo(index, type, split[1], split[2], split[3], 1, split[4], split[5], split[0], split[6], "", "", "", "", "", "", "", split[7], "", "", "", split[8]);
                        else if (type == "Guide")
                            pi = new PostInfo(index, type, split[1], split[2], split[3], 1, split[4], split[5], split[0], split[6], split[7], "", "", "", "", "", "", "", "", split[8], split[9], "");
                        index++;
                        data.Add(pi);
                    }
                }
            }
            return data;
        }

        public static string[] colors = { "F37E79", "F3BF79", "F3D979", "7AF379", "7998F3", "DE79F3" };

        public static string FirstLetterToUpperCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static string Sanitize(string s)
        {
            s = s.Trim();
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return string.Empty;

            return s.TrimStart('\"').TrimEnd('\"').Trim();
        }
    }
}
