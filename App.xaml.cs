using System.Windows;
using LinksAndMore.Services;

namespace LinksAndMore;

public partial class App : Application
{
    public static IDataService DataService { get; } = new DataService();

    private const string LegacyHtml = @"
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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // One-time migration if file doesn't exist
            var existingData = await DataService.LoadDataAsync();
            
            if (!existingData.Any())
            {
                var migrated = MigrationService.MigrateFromHtml(LegacyHtml);
                await DataService.SaveDataAsync(migrated);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Startup Error: {ex.Message}");
        }
    }
}
