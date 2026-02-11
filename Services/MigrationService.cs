using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using LinksAndMore.Models;

namespace LinksAndMore.Services;

public static class MigrationService
{
    public static ObservableCollection<Category> MigrateFromHtml(string html)
    {
        var categories = new ObservableCollection<Category>();
        
        // Find all columns/sections
        // Note: Regex for HTML is fragile; consider a proper HTML parser (e.g. HtmlAgilityPack) 
        // if this feature becomes user-facing or needs to handle complex HTML.
        var sections = Regex.Matches(html, @"<h3[^>]*>(.*?)</h3>(.*?)(?=<h3|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match section in sections)
        {
            var categoryName = section.Groups[1].Value.Trim();
            var content = section.Groups[2].Value;

            var category = new Category { Name = categoryName };

            // Links: <p><a href="URL" ...>Name</a></p>
            var links = Regex.Matches(content, @"<a\s+[^>]*href=[""'](.*?)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase);
            foreach (Match link in links)
            {
                category.Items.Add(new DashboardItem
                {
                    Title = link.Groups[2].Value.Trim(),
                    Content = link.Groups[1].Value.Trim(),
                    Type = ItemType.Link
                });
            }

            // Snippets: <pre><code>...</code></pre>
            var snippets = Regex.Matches(content, @"<pre[^>]*>\s*<code[^>]*>(.*?)</code>\s*</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match snippet in snippets)
            {
                category.Items.Add(new DashboardItem
                {
                    Title = "Code Snippet",
                    Content = snippet.Groups[1].Value.Trim(),
                    Type = ItemType.Snippet
                });
            }

            // Notes: <p>Text</p> (that aren't links)
            var notes = Regex.Matches(content, @"<p[^>]*>(?!\s*<a)(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match note in notes)
            {
                var text = StripHtml(note.Groups[1].Value).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    category.Items.Add(new DashboardItem
                    {
                        Title = text.Length > 30 ? text.Substring(0, 30) + "..." : text,
                        Content = text,
                        Type = ItemType.Note
                    });
                }
            }

            if (category.Items.Any())
            {
                categories.Add(category);
            }
        }

        return categories;
    }

    private static string StripHtml(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
