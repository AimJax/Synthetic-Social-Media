using System.Net.Http.Json;
using System.Text.Json;

namespace SyntheticSocialWorld.Simulation;

/// <summary>
/// Interface for AI content generation
/// </summary>
public interface IAIProvider
{
    string ModelName { get; }
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync();
}

/// <summary>
/// Ollama LLM provider for natural language content generation
/// </summary>
public class OllamaAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly Action<string>? _logAction;

    public string ModelName => _model;

    public OllamaAIProvider(string baseUrl = "http://localhost:11434", string model = "qwen3:4b", Action<string>? logAction = null)
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        _baseUrl = baseUrl;
        _model = model;
        _logAction = logAction;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Limit response tokens to get concise social media posts
            var request = new
            {
                model = _model,
                prompt = $"/no think\n{prompt}\n\nWrite ONLY the post content, nothing else. Maximum 140 characters.",
                stream = false,
                options = new
                {
                    temperature = 0.8,
                    num_predict = 100,  // Keep responses short
                    stop = new[] { "\n\n", "---", "**", "##" }  // Stop at formatting markers
                }
            };

            _logAction?.Invoke($"[Ollama] Generating content...");

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/api/generate",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logAction?.Invoke($"[Ollama] Request failed with status {response.StatusCode}");
                return GetFallbackContent(prompt);
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken);
            
            if (result == null || string.IsNullOrWhiteSpace(result.response))
            {
                _logAction?.Invoke("[Ollama] Empty response received");
                return GetFallbackContent(prompt);
            }

            // Clean up the response - remove thinking content if present
            var content = result.response.Trim();
            var firstNewline = content.IndexOf('\n');
            if (firstNewline > 0 && firstNewline < 50)
            {
                content = content.Substring(0, firstNewline).Trim();
            }
            
            // Ensure max length for social media
            if (content.Length > 280)
            {
                content = content.Substring(0, 277) + "...";
            }

            _logAction?.Invoke($"[Ollama] Generated: {content.Substring(0, Math.Min(50, content.Length))}...");
            return content;
        }
        catch (Exception ex)
        {
            _logAction?.Invoke($"[Ollama] Error: {ex.Message}");
            return GetFallbackContent(prompt);
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string GetFallbackContent(string prompt)
    {
        var fallbacks = new[]
        {
            "Having a great day! What's on your mind?",
            "Just sharing my thoughts with everyone.",
            "Interesting times we live in!",
            "Taking it easy today.",
            "Life is good!",
            "Nothing better than a peaceful moment.",
            "Feeling grateful for today.",
            "Making the most of every moment.",
            "Taking one day at a time.",
            "Staying positive and moving forward!"
        };
        
        var hash = prompt.GetHashCode();
        return fallbacks[Math.Abs(hash) % fallbacks.Length];
    }

    private class OllamaResponse
    {
        public string? response { get; set; }
        public bool done { get; set; }
    }
}

/// <summary>
/// Mock AI provider for testing without Ollama
/// </summary>
public class MockAIProvider : IAIProvider
{
    public string ModelName => "mock";

    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var responses = new[]
        {
            "Just had an amazing coffee! Highly recommend it.",
            "Feeling grateful for my friends today.",
            "Can't believe how fast time flies!",
            "Working on something exciting. More soon!",
            "What a beautiful day to be alive!",
            "Reflecting on life and all its adventures.",
            "Grateful for the little things.",
            "Taking a moment to appreciate the world.",
            "Sometimes the best things are the simplest.",
            "Embracing the journey, one day at a time."
        };

        var random = new Random();
        return Task.FromResult(responses[random.Next(responses.Length)]);
    }

    public Task<bool> IsAvailableAsync() => Task.FromResult(true);
}
