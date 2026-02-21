using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System.Linq;

namespace LinksAndMore.Services;

public interface ISemanticEngine : IDisposable
{
    float[] GenerateEmbedding(string text);
    float CalculateCosineSimilarity(float[] vectorA, float[] vectorB);
    bool IsLoaded { get; }
}

public class SemanticEngine : ISemanticEngine
{
    private InferenceSession? _session;
    private BertTokenizer? _tokenizer;
    public bool IsLoaded { get; private set; }

    public SemanticEngine()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (ModelDownloadService.IsModelDownloaded())
        {
            try 
            {
                _session = new InferenceSession(ModelDownloadService.ModelFilePath);
                _tokenizer = BertTokenizer.Create(ModelDownloadService.VocabFilePath);
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading AI components: {ex.Message}");
            }
        }
    }

    public float[] GenerateEmbedding(string text)
    {
        if (!IsLoaded)
        {
            Initialize();
        }

        if (!IsLoaded || _tokenizer == null || _session == null || string.IsNullOrWhiteSpace(text))
        {
            return new float[384];
        }

        // Tokenize
        var encodeResults = _tokenizer.EncodeToIds(text, 256, out _, out _);
        
        long[] inputIds = encodeResults.Select(t => (long)t).ToArray();
        long[] attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();
        long[] tokenTypeIds = new long[inputIds.Length];
        
        var dimensions = new[] { 1, inputIds.Length };
        
        var inputIdsTensor = new DenseTensor<long>(inputIds, dimensions);
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, dimensions);
        var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIds, dimensions);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
        };

        // Inference
        using var runResults = _session.Run(inputs);
        
        // Output named 'last_hidden_state' typically [1, seq_len, 384] for MiniLM
        var firstOutput = runResults.First().AsEnumerable<float>().ToArray();
        
        if (firstOutput.Length > 384) 
        {
            return MeanPooling(firstOutput, inputIds.Length, 384);
        }
        
        return firstOutput.Take(384).ToArray();
    }
    
    private float[] MeanPooling(float[] lastHiddenState, int seqLength, int hiddenSize)
    {
        var pooled = new float[hiddenSize];
        for (int i = 0; i < seqLength; i++)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                pooled[j] += lastHiddenState[i * hiddenSize + j];
            }
        }
        for (int j = 0; j < hiddenSize; j++)
        {
            pooled[j] /= seqLength;
        }
        
        // L2 Normalize
        float sum = 0;
        for (int j = 0; j < hiddenSize; j++)
        {
            sum += pooled[j] * pooled[j];
        }
        float norm = (float)Math.Sqrt(sum);
        if (norm > 0)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                pooled[j] /= norm;
            }
        }
        
        return pooled;
    }

    public float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null || vectorA.Length != vectorB.Length) return 0f;

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0) return 0;

        return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
