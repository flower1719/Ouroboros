using Ouroboros.Tool.AiClient.Model;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ouroboros.Tool.AiClient
{
    public class AiChatClient
    {
        private readonly AiClientOptions _options;
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AiChatClient(AiClientOptions options)
        {
            _options = options;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<AiChatResponse> CompleteAsync(
            AiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = BuildHttpRequest(request, stream: false);
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            await EnsureSuccessWithDetailsAsync(response, cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var dto = JsonSerializer.Deserialize<OpenAiResponseDto>(json, JsonOptions)
                      ?? throw new InvalidOperationException("AI 返回了空响应");

            var choice = dto.Choices?.FirstOrDefault();
            var usage = dto.Usage is not null
                ? new AiTokenUsage(dto.Usage.PromptTokens, dto.Usage.CompletionTokens)
                : null;

            return new AiChatResponse(
                Content: choice?.Message?.Content ?? string.Empty,
                FinishReason: choice?.FinishReason,
                Usage: usage,
                ModelUsed: dto.Model
            );
        }

        /// <summary>
        /// 流式请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<AiChatStreamChunk> StreamAsync(
            AiChatRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var httpRequest = BuildHttpRequest(request, stream: true);

            // 关键坑点：流式请求必须使用 ResponseHeadersRead，否则 HttpClient 会等全部收完才返回！
            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            await EnsureSuccessWithDetailsAsync(response, cancellationToken);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data: ")) continue;

                var data = line["data: ".Length..].Trim();
                if (data == "[DONE]") break;

                var chunkDto = JsonSerializer.Deserialize<OpenAiStreamDto>(data, JsonOptions);
                if (chunkDto is null) continue;

                var choice = chunkDto.Choices?.FirstOrDefault();
                var delta = choice?.Delta?.Content ?? string.Empty;
                var usage = chunkDto.Usage is not null
                    ? new AiTokenUsage(chunkDto.Usage.PromptTokens, chunkDto.Usage.CompletionTokens)
                    : null;

                if (!string.IsNullOrEmpty(delta) || choice?.FinishReason is not null || usage is not null)
                {
                    yield return new AiChatStreamChunk(delta, choice?.FinishReason, usage);
                }
            }
        }

        private HttpRequestMessage BuildHttpRequest(AiChatRequest request, bool stream)
        {
            var url = $"{_options.Endpoint.TrimEnd('/')}/chat/completions";
            var msg = new HttpRequestMessage(HttpMethod.Post, url);
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var payload = new Dictionary<string, object?>
            {
                ["model"] = request.Model ?? _options.DefaultModel,
                ["messages"] = request.Messages.Select(m => new
                {
                    role = m.Role.ToString().ToLowerInvariant(),
                    content = m.Content,
                    tool_call_id = m.ToolCallId
                }),
                ["stream"] = stream,
                ["temperature"] = request.Temperature,
                ["max_tokens"] = request.MaxTokens
            };

            if (stream)
            {
                // 让流式在最后一块返回 Token 统计（大部分兼容接口都支持）
                payload["stream_options"] = new { include_usage = true };
            }
            if (request.JsonMode)
            {
                payload["response_format"] = new { type = "json_object" };
            }
            if (request.ExtraParameters is not null)
            {
                foreach (var kv in request.ExtraParameters)
                    payload[kv.Key] = kv.Value;
            }

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return msg;
        }

        private static async Task EnsureSuccessWithDetailsAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return;
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"AI API 调用失败 ({(int)response.StatusCode}): {errorBody}");
        }

        // --- 内部序列化 DTO ---
        private record OpenAiResponseDto(string? Model, List<ChoiceDto>? Choices, UsageDto? Usage);
        private record ChoiceDto(MessageDto? Message, string? FinishReason);
        private record MessageDto(string? Content);
        private record OpenAiStreamDto(List<StreamChoiceDto>? Choices, UsageDto? Usage);
        private record StreamChoiceDto(MessageDto? Delta, string? FinishReason);
        private record UsageDto(int PromptTokens, int CompletionTokens);
    }
}
