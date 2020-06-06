using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amicitia.github.io
{
    class Program
    {
        public static string indexPath;
        public static List<string> gameList = new List<string>() { "p3fes", "p4", "p5", "p4g", "p3p", "p3d", "p4d", "p5d", "pq", "pq2", "p4au", "smt3", "cfb"};

        static void Main(string[] args)
        {
            indexPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            List<PostInfo> data = GetData(indexPath).OrderBy(d => DateTime.Parse(d.Date, CultureInfo.CreateSpecificCulture("en-US"))).ToArray().Reverse().ToList();

            //List all mods, tools, cheats and guides
            Console.WriteLine("Creating index...");
            CreateIndex(data);
            Console.WriteLine("Creating mod pages...");
            CreateModPages(data.Where(p => p.Type == "Mod").ToList());
            Console.WriteLine("Creating tool pages...");
            CreateToolPages(data.Where(p => p.Type == "Tool").ToList());
            Console.WriteLine("Creating cheat pages...");
            CreateCheatPages(data.Where(p => p.Type == "Cheat").ToList());
            Console.WriteLine("Creating guide pages...");
            CreateGuidePages(data.Where(p => p.Type == "Guide").ToList());
            Console.WriteLine("Creating game pages...");
            CreateGamePages(data);
            //List all searchable pages
            Console.WriteLine("Creating author pages...");
            CreateAuthorPages(data);
            Console.WriteLine("Creating tag pages...");
            CreateTagPages(data);
            //Post hyperlinks
            Console.WriteLine("Creating post hyperlinks...");
            CreatePostPages(data);

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        private static void CreateGamePages(List<PostInfo> data)
        {
            //Create single-post pages for hyperlinks
            foreach (var game in gameList)
            {
                CreateHtml(data.Where(p => p.Game.Equals(game.ToUpper())).ToList(), $"game\\{game}");
            }
        }

        private static void CreatePostPages(List<PostInfo> data)
        {
            //Create single-post pages for hyperlinks
            foreach (var post in data)
            {
                List<PostInfo> singlePost = new List<PostInfo>() { post };
                CreateHtml(singlePost, $"post\\{post.Hyperlink}");
            }
        }

        private static void CreateTagPages(List<PostInfo> data)
        {
            List<string> tagList = new List<string>();
            foreach (var post in data)
            {
                //Split and sanitize individual unique tags from all posts
                var tags = post.Tags.Split(',');
                foreach (var tag in tags)
                {
                    if (!tagList.Contains(tag.Trim()))
                        tagList.Add(tag.Trim());
                }
            }

            //Create individual pages for unique tags
            foreach (var tag in tagList)
                CreateHtml(data.Where(p => p.Tags.Contains(tag)).ToList(), $"tag\\{tag}");
        }

        private static void CreateAuthorPages(List<PostInfo> data)
        {
            List<string> uniqueAuthors = new List<string>();
            foreach (var post in data)
            {
                //Split multiple authors per post and sanitize
                string[] splitAuthors = post.Author.Split(',');
                for (int i = 0; i < splitAuthors.Count(); i++)
                    splitAuthors[i] = splitAuthors[i].Trim();

                //Add to list of unique authors
                foreach (var author in splitAuthors)
                if (!uniqueAuthors.Contains(author))
                    uniqueAuthors.Add(author);
            }

            //Create individual pages for each unique creator
            foreach (var author in uniqueAuthors)
            {
                var newData = data.Where(p => p.Author.Contains(author)).ToList();
                CreateHtml(newData, $"author\\{author}");
            }
                
        }

        private static void CreateGuidePages(List<PostInfo> data)
        {
            //Create index of all guides
            CreateHtml(data, "guides");
            //Create guide indexes narrowed down per game
            foreach (var game in gameList)
            {
                if (game == "p3fes")
                    CreateHtml(data.Where(p => p.Game.Equals("P4") || p.Game.Equals("")).ToList(), $"guides\\p3fes");
                else
                    CreateHtml(data.Where(p => p.Game.Equals(game.ToUpper()) || p.Game.Equals("")).ToList(), $"guides\\{game}");
            }
        }

        private static void CreateCheatPages(List<PostInfo> data)
        {
            CreateHtml(data, "cheats");
            foreach (var game in gameList)
            {
                CreateHtml(data.Where(p => p.Game.Equals(game.ToUpper())).ToList(), $"cheats\\{game}");
            }
        }

        private static void CreateToolPages(List<PostInfo> data)
        {
            CreateHtml(data, "tools");
            foreach (var game in gameList)
            {
                if (game == "p3fes")
                    CreateHtml(data.Where(p => p.Game.Equals("P4") || p.Game.Equals("")).ToList(), $"tools\\p3fes");
                else
                    CreateHtml(data.Where(p => p.Game.Equals(game.ToUpper()) || p.Game.Equals("")).ToList(), $"tools\\{game}");
            }
        }

        private static void CreateModPages(List<PostInfo> data)
        {
            CreateHtml(data, "mods");
            foreach (var game in gameList)
            {
                CreateHtml(data.Where(p => p.Game.Equals(game.ToUpper())).ToList(), $"mods\\{game}");
            }
        }

        private static void CreateHtml(List<PostInfo> data, string url)
        {
            string content = "";

            int postLimit = 0;
            int pageNumber = 1;
            int postNumber = 0;

            //Not found page or results
            if (data.Count == 0)
                content += $"<br><center>Sorry! No posts matching your query were found. Please check again later.</center>";
            else
                content += $" ({data.Count} results)<br>";

            //Extra info on using mods
            if (url.Contains("mods\\p5"))
                content += "<br><center>To learn how to run P5 mods, see <a href=\"https://amicitia.github.io/post/p5-rpcs3-setupguide\">this guide.</a></center>";
            else if (url.Contains("mods\\p3fes") || url.Equals("mods\\p4") || url.Contains("mods\\smt3.html"))
                content += "<br><center>To learn how to run these mods, see <a href=\"https://amicitia.github.io/post/hostfs-guide\">this guide.</a></center>";
            else if (url.Contains("mods\\"))
                content += "<br><center>To learn how to use these mods, see <a href=\"https://amicitia.github.io/post/p5-rpcs3-modcreationguide\">this guide.</a></center>";

            foreach (PostInfo post in data)
            {
                postNumber++;
                if (postLimit < 30)
                {
                    if (url.Contains("post"))
                        content += WritePost(post, false);
                    else
                        content += WritePost(post);
                    postLimit++;
                }
                else
                {
                    if (pageNumber == 1)
                        CreatePage(content, $"{url}.html", pageNumber, (data.Count - postNumber >= 30));
                    else
                        CreatePage(content, $"{url}\\{pageNumber}.html", pageNumber, (data.Count - postNumber >= 30));
                        
                    content = "";

                    if (data.Count - postNumber >= 30)
                        postLimit = 0;
                    else
                        postLimit = data.Count - postNumber;
                    pageNumber++;
                }
            }

            if (pageNumber == 1)
                CreatePage(content, $"{url}.html", pageNumber, false);
        }

        private static void CreateIndex(List<PostInfo> data)
        {
            CreateHtml(data, "index");
        }

        private static void CreatePage(string content, string url, int pageNumber, bool morePages)
        {
            //Header
            string html = Properties.Resources.IndexHeader;
            html += "<br><a href=\"https://amicitia.github.io/\">index</a>";
            foreach (var split in url.Split('\\'))
            {
                if (split == "mods" || split == "tools" || split == "guides" || split == "cheats")
                    html += $" > <a href=\"https://amicitia.github.io/{split}\">{split}</a>";
                else if (gameList.Any(g => g.Equals(split)))
                    html += $" > <a href=\"https://amicitia.github.io/game/{split}\">{split}</a>";
                else if (split != "index")
                    html += $" > {split.Replace(".html", "")}";
            }

            //Auto-select game or type
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

            //Body Content
            html += content;

            //Set up for pagination and ref link depth
            int depth = url.Count(c => c == '\\');
            string url2 = url.Replace(".html","");
            if (depth == 1)
                url2 = url2.Replace($"{url}\\{url}",$"{url}");

            //Table for pagination
            html += "<table><tr>";
            //Page Back
            if (pageNumber > 1)
            {
                if (pageNumber == 2)
                    html += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", "")}\"><div class=\"unhide\">Previous</div></a></td>";
                else
                    html += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", $"\\{pageNumber - 1}")}\"><div class=\"unhide\">Previous</div></a></td>";
            }
            else
                html += "<td></td>";
            //Page Forward if further pages found
            if (morePages)
            {
                if (pageNumber == 1)
                    html += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2 + $"\\{pageNumber + 1}"}\"><div class=\"unhide\">Next</div></a></td>";
                else
                    html += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", $"\\{pageNumber + 1}")}\"><div class=\"unhide\">Next</div></a></td>";
            }
            else if (File.Exists(url2.Replace($"\\{pageNumber}", $"\\{pageNumber + 1}")))
                html += $"<td><a href=\"https:\\\\amicitia.github.io\\{url2.Replace($"\\{pageNumber}", $"\\{pageNumber + 1}")}\"><div class=\"unhide\">Next</div></a></td>";
            else
                html += "<td></td>";

            //End pagination table
            html += "</tr></table>";

            //Footer
            html += Properties.Resources.IndexFooter;
            html += $"{DateTime.Now.Year}. Last updated {DateTime.Now.Month}/{DateTime.Now.Day}/{DateTime.Now.Year}.</div></footer></html>";
            
            //Replace relative links based on depth
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
            
            Console.WriteLine($"Created {htmlPath}");
        }

        private static string WritePost(PostInfo data, bool toggle = true)
        {
            string result;
            string[] splitGames = data.Game.Split(' ');

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
            else if (data.Embed != null)
                result += $"<img src=\"{data.Embed}\" height=\"auto\" width=\"auto\"></td>";
            else
                result += "</td>";

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
                result += $"<center><font size=\"4\"><a href=\"{data.DownloadURL}\"><i class=\"{data.DownloadIcon}\" aria-hidden=\"true\"></i> {data.DownloadText}</a><br>";
                if (data.DownloadURL2 != "")
                    result += $"<a href=\"{data.DownloadURL2}\"><i class=\"{data.DownloadIcon2}\" aria-hidden=\"true\"></i> {data.DownloadText2}</a><br>";
                if (data.SourceURL != "")
                    result += $"<a href=\"{data.SourceURL}\"><i class=\"fas fa-file-code\" aria-hidden=\"true\"></i> Source Code</a>";
                result += "</center></div>";
            }
            else if (data.Type == "Cheat")
                    result += $"<div id=\"cheat{data.ID}\" onclick=\"copyDivToClipboard('cheat{data.ID}')\"><div class=\"cheatcode\">{data.UpdateText}</div></div>";

            //Related Guides
            if (!String.IsNullOrEmpty(data.GuideURL))
                result += $"<br><center><a href=\"https://shrinefox.com/guides/{data.GuideURL}\"><i class=\"fas fa-info-circle\" aria-hidden=\"true\"></i> {data.GuideText}</a></center>";
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
            List<PostInfo> data = new List<PostInfo>();
            string connection = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"{indexPath}\\db\\ShrineFox.mdf\";Integrated Security=True";

            SqlConnection con = new SqlConnection(connection);
            SqlCommand command = new SqlCommand("SELECT * FROM Post", con);
            con.Open();
            SqlDataReader sdr = command.ExecuteReader();
            while (sdr.Read())
            {
                PostInfo pi = new PostInfo(Convert.ToInt32(sdr["ID"]), sdr["Type"].ToString(), sdr["Title"].ToString(), sdr["Game"].ToString(), sdr["Author"].ToString(), Convert.ToSingle(sdr["Version"]), sdr["Date"].ToString(), sdr["Tags"].ToString(), sdr["Hyperlink"].ToString(), sdr["Description"].ToString(), sdr["Embed"].ToString(), sdr["DownloadURL"].ToString(), sdr["DownloadText"].ToString(), sdr["DownloadIcon"].ToString(), sdr["DownloadURL2"].ToString(), sdr["DownloadText2"].ToString(), sdr["DownloadIcon2"].ToString(), sdr["UpdateText"].ToString(), sdr["SourceURL"].ToString(), sdr["GuideURL"].ToString(), sdr["GuideText"].ToString(), sdr["ThreadURL"].ToString());
                data.Add(pi);
            }
            con.Close();
            return data;
        }

        public static string[] colors = { "F37E79", "F3BF79", "F3D979", "7AF379", "7998F3", "DE79F3" };
    }
}
