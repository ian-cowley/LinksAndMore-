using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using LinksAndMore.Models;

namespace LinksAndMore.Services;

public static class MigrationService
{
    private const string DefaultDataHtml = @"
    <div class=""main-content"">
        <h2>Welcome to My Page</h2>
        <br>
        <div class=""row"">
            <div class=""column"">
                <h3>Google AI Stuff</h3>
                <p><a href=""https://notebooklm.google.com/"" target=""NoteBookLM"">NoteBookLM</a></p>
                <p><a href=""https://gemini.google.com/app"" target=""Gemini"">Gemini</a></p>
                <p><a href=""https://ai.google/"" target=""GoogleAI"">Google AI</a></p>
                <p><a href=""https://deepmind.google/"" target=""GoogleDeepMind"">Google DeepMind</a></p>
                <p><a href=""https://cloud.google.com/ai"" target=""GoogleCloudAI"">Google Cloud AI</a></p>
                <p><a href=""https://jules.google.com/"" target=""Jules"">Jules</a></p>
                <p><a href=""https://aistudio.google.com/"" target=""GoogleAIStudio"">Google AI Studio</a></p>
                <p><a href=""https://developers.google.com/"" target=""GoogleDevelopers"">Google for Developers</a></p>
            </div>
            <div class=""column"">
                <h3>Other AI Stuff</h3>
                <p><a href=""https://figure.ai/"" target=""Jules"">Jules</a></p>
                <p><a href=""https://huggingface.co/"" target=""HuggingFace"">Hugging Face</a></p>
                <p><a href=""https://openai.com/"" target=""OpenAI"">OpenAI</a></p>
                <p><a href=""https://www.anthropic.com/"" target=""Anthropic"">Anthropic</a></p>
                <p><a href=""https://www.perplexity.ai/"" target=""PerplexityAI"">Perplexity AI</a></p>
                <p><a href=""https://poe.com/"" target=""PoebyQuora"">Poe by Quora</a></p>
                <p><a href=""https://www.kaggle.com/"" target=""Kaggle"">Kaggle</a></p>
                <p><a href=""https://www.tensorflow.org/"" target=""TensorFlow"">TensorFlow</a></p>
            </div>
        </div>
        <div class=""row"">
            <div class=""column"">
                <h3>Local AI</h3>
                <p><a href=""https://developer.nvidia.com/"" target=""NVIDIADev"">NVIDIA Developer</a></p>
                <p><a href=""https://rocm.docs.amd.com/en/latest/"" target=""AMDDev"">AMD ROCm Docs</a></p>
                <p><a href=""https://lmstudio.ai/"" target=""LMStudio"">LM Studio</a></p>
                <p><a href=""https://ollama.ai/"" target=""Ollama"">Ollama</a></p>
                <p><a href=""https://jan.ai/"" target=""Jan"">Jan</a></p>
                <p><a href=""https://gpt4all.io/"" target=""GPT4All"">GPT4All</a></p>
            </div>
            <div class=""column"">
                <h3>Machine Learning</h3>
                <p><a href=""https://www.coursera.org/learn/machine-learning"" target=""CourseraML"">Coursera ML</a></p>
                <p><a href=""https://www.fast.ai/"" target=""FastAI"">fast.ai</a></p>
                <p><a href=""https://scikit-learn.org/stable/"" target=""ScikitLearn"">Scikit-learn</a></p>
                <p><a href=""https://pytorch.org/"" target=""PyTorch"">PyTorch</a></p>
                <p><a href=""https://paperswithcode.com/"" target=""PapersWithCode"">Papers with Code</a></p>
                <p><a href=""https://distill.pub/"" target=""DistillPub"">Distill.pub</a></p>
                <p><a href=""https://www.reddit.com/r/MachineLearning/"" target=""MLSubreddit"">ML Subreddit</a></p>
            </div>
            <div class=""column"">
                <h3>Developer Resources</h3>
                <p><a href=""https://stackoverflow.com/"" target=""StackOverflow"">Stack Overflow</a></p>
                <p><a href=""https://github.com/"" target=""GitHub"">GitHub</a></p>
                <p><a href=""https://news.ycombinator.com/"" target=""HackerNews"">Hacker News</a></p>
                <p><a href=""https://dev.to/"" target=""DevTo"">Dev.to</a></p>
                <p><a href=""https://alistapart.com/"" target=""AListApart"">A List Apart</a></p>
                <p><a href=""https://www.smashingmagazine.com/"" target=""SmashingMagazine"">Smashing Magazine</a></p>
            </div>
        </div>
    </div>";

    public static ObservableCollection<Category> GetDefaultData()
    {
        return MigrateFromHtml(DefaultDataHtml);
    }

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
