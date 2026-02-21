using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LinksAndMore.Services;

public class ModelDownloadService
{
    private static readonly string ModelDirPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "LinksAndMore", "Models");

    public static readonly string ModelFilePath = Path.Combine(ModelDirPath, "model.onnx");
    public static readonly string VocabFilePath = Path.Combine(ModelDirPath, "vocab.txt");

    // URLs to the raw files from HuggingFace for all-MiniLM-L6-v2
    private const string ModelUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/onnx/model_quantized.onnx";
    private const string VocabUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/vocab.txt";

    public static bool IsModelDownloaded()
    {
        return File.Exists(ModelFilePath) && File.Exists(VocabFilePath);
    }

    public static async Task DownloadModelAsync(IProgress<double>? progress = null)
    {
        if (!Directory.Exists(ModelDirPath))
        {
            Directory.CreateDirectory(ModelDirPath);
        }

        using var client = new HttpClient();
        
        // Download Model
        await DownloadFileAsync(client, ModelUrl, ModelFilePath, progress, 0.0, 0.95);
        
        // Download Vocab
        await DownloadFileAsync(client, VocabUrl, VocabFilePath, progress, 0.95, 1.0);
    }

    private static async Task DownloadFileAsync(HttpClient client, string url, string destination, IProgress<double>? progress, double startProgress, double endProgress)
    {
        if (File.Exists(destination)) return;

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        var totalRead = 0L;
        var read = 0;

        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            await fileStream.WriteAsync(buffer, 0, read);
            totalRead += read;

            if (canReportProgress)
            {
                var currentProgress = startProgress + ((double)totalRead / totalBytes) * (endProgress - startProgress);
                progress?.Report(currentProgress);
            }
        }
    }
}
