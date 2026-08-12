using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AI.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ErpToolRegistry _toolRegistry;
    private readonly ILogger<OpenAiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ErpToolRegistry toolRegistry,
        ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _toolRegistry = toolRegistry;
        _logger = logger;

        // Set Authorization header from environment variable only
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    #region Dashboard Insights (Non-streaming, Structured Output)

    public async Task<Result<DashboardInsightDto>> GetDashboardInsightsAsync(
        DashboardInsightRequest request, CancellationToken ct)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured. Returning fallback insight.");
            return Result<DashboardInsightDto>.Ok(BuildFallbackInsight());
        }

        try
        {
            var systemPrompt = BuildInsightsSystemPrompt();
            var userContent = JsonSerializer.Serialize(request, JsonOpts);

            var payload = new
            {
                model = _options.Model,
                input = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userContent }
                },
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "dashboard_insight",
                        strict = true,
                        schema = GetDashboardInsightSchema()
                    }
                },
                max_output_tokens = _options.MaxOutputTokens,
                stream = false
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync("responses", payload, JsonOpts, ct);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("OpenAI API error. Status={Status}, Latency={Latency}ms, Body={Body}",
                    response.StatusCode, sw.ElapsedMilliseconds, errorBody);
                
                var fallback = BuildFallbackInsight();
                fallback.Summary = $"HATA: {response.StatusCode}";
                fallback.RiskExplanation = errorBody;
                return Result<DashboardInsightDto>.Ok(fallback);
            }

            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

            // Extract usage for logging (content is NOT logged)
            LogUsage(responseJson, "insights/dashboard", sw.ElapsedMilliseconds);

            // Extract text output from Responses API
            var outputText = ExtractOutputText(responseJson);
            if (string.IsNullOrEmpty(outputText))
            {
                _logger.LogWarning("OpenAI returned empty output for dashboard insights.");
                return Result<DashboardInsightDto>.Ok(BuildFallbackInsight());
            }

            var insight = JsonSerializer.Deserialize<DashboardInsightDto>(outputText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Result<DashboardInsightDto>.Ok(insight ?? BuildFallbackInsight());
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("OpenAI request timed out for dashboard insights.");
            return Result<DashboardInsightDto>.Ok(BuildFallbackInsight());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling OpenAI for dashboard insights.");
            return Result<DashboardInsightDto>.Ok(BuildFallbackInsight());
        }
    }

    #endregion

    #region Chat Assistant (Streaming + Tool Calling)

    public async IAsyncEnumerable<ChatStreamChunk> ChatStreamAsync(
        string userMessage, List<ChatHistoryItemDto>? history, string userId, [EnumeratorCancellation] CancellationToken ct)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            yield return new ChatStreamChunk
            {
                Type = "error",
                Content = "AI asistanı şu an kullanılamıyor. OpenAI API anahtarı yapılandırılmamış."
            };
            yield return new ChatStreamChunk { Type = "done" };
            yield break;
        }

        var systemPrompt = BuildChatSystemPrompt();
        var tools = _toolRegistry.GetToolDefinitions();

        // Build initial input
        var inputMessages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (history != null)
        {
            foreach (var item in history)
            {
                inputMessages.Add(new { role = item.Role, content = item.Content });
            }
        }

        inputMessages.Add(new { role = "user", content = userMessage });

        int iteration = 0;
        string? previousResponseId = null;

        while (iteration < _options.MaxToolCallIterations)
        {
            iteration++;

            object payload;
            if (previousResponseId != null)
            {
                // Subsequent request with tool outputs — use input array with function_call_output
                payload = new
                {
                    model = _options.Model,
                    input = inputMessages,
                    tools,
                    stream = true,
                    max_output_tokens = _options.MaxOutputTokens,
                    previous_response_id = previousResponseId
                };
            }
            else
            {
                payload = new
                {
                    model = _options.Model,
                    input = inputMessages,
                    tools,
                    stream = true,
                    max_output_tokens = _options.MaxOutputTokens
                };
            }

            HttpResponseMessage? response = null;
            ChatStreamChunk? errorChunk = null;
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    "responses")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(payload, JsonOpts),
                        Encoding.UTF8, "application/json")
                };

                response = await _httpClient.SendAsync(httpRequest,
                    HttpCompletionOption.ResponseHeadersRead, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("OpenAI streaming API error. Status={Status}, Body={Body}", response.StatusCode, errBody);
                    errorChunk = new ChatStreamChunk
                    {
                        Type = "error",
                        Content = $"AI asistanı şu an yanıt veremiyor. Hata: {response.StatusCode} - {errBody}"
                    };
                }
            }
            catch (TaskCanceledException)
            {
                errorChunk = new ChatStreamChunk
                {
                    Type = "error",
                    Content = "AI isteği zaman aşımına uğradı."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending streaming request to OpenAI.");
                errorChunk = new ChatStreamChunk
                {
                    Type = "error",
                    Content = "AI asistanı şu an kullanılamıyor."
                };
            }

            if (errorChunk != null)
            {
                yield return errorChunk;
                yield return new ChatStreamChunk { Type = "done" };
                yield break;
            }

            // Parse the SSE stream
            var streamResult = await ProcessStreamAsync(response!, ct);

            // Yield text deltas that were accumulated
            foreach (var textChunk in streamResult.TextChunks)
            {
                yield return textChunk;
            }

            // If there were tool calls, execute them and loop
            if (streamResult.ToolCalls.Count > 0)
            {
                previousResponseId = streamResult.ResponseId;
                inputMessages = new List<object>();

                foreach (var toolCall in streamResult.ToolCalls)
                {
                    var handler = _toolRegistry.GetTool(toolCall.Name);
                    string toolResult;

                    if (handler == null)
                    {
                        _logger.LogWarning("Unknown tool requested: {ToolName}", toolCall.Name);
                        toolResult = JsonSerializer.Serialize(new { error = $"Unknown tool: {toolCall.Name}" });
                    }
                    else
                    {
                        try
                        {
                            toolResult = await handler.ExecuteAsync(toolCall.Arguments, userId, ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Tool execution failed: {ToolName}", toolCall.Name);
                            toolResult = JsonSerializer.Serialize(new { error = $"Tool execution error: {ex.Message}" });
                        }
                    }

                    // Add function_call_output to input for next iteration
                    inputMessages.Add(new
                    {
                        type = "function_call_output",
                        call_id = toolCall.CallId,
                        output = toolResult
                    });
                }

                // Continue loop to send tool results back to OpenAI
                continue;
            }

            // No more tool calls — yield usage and done
            if (streamResult.Usage != null)
            {
                LogUsage(streamResult.Usage.Value.InputTokens,
                    streamResult.Usage.Value.OutputTokens, "chat", 0);

                yield return new ChatStreamChunk
                {
                    Type = "usage",
                    Usage = new ChatUsageInfo
                    {
                        InputTokens = streamResult.Usage.Value.InputTokens,
                        OutputTokens = streamResult.Usage.Value.OutputTokens
                    }
                };
            }

            yield return new ChatStreamChunk { Type = "done" };
            yield break;
        }

        // Max iterations reached
        _logger.LogWarning("Max tool call iterations ({Max}) reached for chat request.", _options.MaxToolCallIterations);
        yield return new ChatStreamChunk
        {
            Type = "error",
            Content = "AI asistanı çok sayıda veri sorgusu yaptı. Lütfen sorunuzu daha net ifade edin."
        };
        yield return new ChatStreamChunk { Type = "done" };
    }

    #endregion

    #region SSE Stream Processing

    private async Task<StreamResult> ProcessStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var result = new StreamResult();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        // Accumulate function call arguments by call_id
        var functionCallArgs = new Dictionary<string, StringBuilder>();
        var functionCallNames = new Dictionary<string, string>();
        var functionCallIds = new Dictionary<string, string>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line.Substring(6);
            if (data == "[DONE]") break;

            System.Console.WriteLine($"[DEBUG SSE] {data}");

            try
            {
                var eventJson = JsonDocument.Parse(data);
                var root = eventJson.RootElement;

                // Check event type
                if (root.TryGetProperty("type", out var typeProp))
                {
                    var eventType = typeProp.GetString();

                    switch (eventType)
                    {
                        case "response.output_text.delta":
                            if (root.TryGetProperty("delta", out var textDelta))
                            {
                                result.TextChunks.Add(new ChatStreamChunk
                                {
                                    Type = "text_delta",
                                    Content = textDelta.GetString()
                                });
                            }
                            break;

                        case "response.function_call_arguments.delta":
                            if (root.TryGetProperty("call_id", out var callIdDelta) &&
                                root.TryGetProperty("delta", out var argDelta))
                            {
                                var cid = callIdDelta.GetString() ?? "";
                                if (!functionCallArgs.ContainsKey(cid))
                                    functionCallArgs[cid] = new StringBuilder();
                                functionCallArgs[cid].Append(argDelta.GetString());
                            }
                            break;

                        case "response.function_call_arguments.done":
                            if (root.TryGetProperty("call_id", out var callIdDone))
                            {
                                var cid = callIdDone.GetString() ?? "";
                                if (root.TryGetProperty("name", out var nameProp))
                                {
                                    functionCallNames[cid] = nameProp.GetString() ?? "";
                                }
                                if (root.TryGetProperty("arguments", out var argProp))
                                {
                                    functionCallArgs[cid] = new StringBuilder(argProp.GetString() ?? "{}");
                                }
                            }
                            break;

                        case "response.output_item.added":
                            // Capture function call metadata (name, call_id)
                            if (root.TryGetProperty("item", out var item) &&
                                item.TryGetProperty("type", out var itemType) &&
                                itemType.GetString() == "function_call")
                            {
                                var cid = item.TryGetProperty("call_id", out var cIdProp)
                                    ? cIdProp.GetString() ?? "" : "";
                                var name = item.TryGetProperty("name", out var nProp)
                                    ? nProp.GetString() ?? "" : "";
                                functionCallNames[cid] = name;
                                functionCallIds[cid] = cid;
                            }
                            break;

                        case "response.output_item.done":
                            if (root.TryGetProperty("item", out var itemDone) &&
                                itemDone.TryGetProperty("type", out var itemTypeDone) &&
                                itemTypeDone.GetString() == "function_call")
                            {
                                var cid = itemDone.TryGetProperty("call_id", out var cIdProp)
                                    ? cIdProp.GetString() ?? "" : "";
                                var name = itemDone.TryGetProperty("name", out var nProp)
                                    ? nProp.GetString() ?? "" : "";
                                var argsStr = itemDone.TryGetProperty("arguments", out var aProp)
                                    ? aProp.GetString() ?? "{}" : "{}";
                                    
                                if (!string.IsNullOrEmpty(cid))
                                {
                                    functionCallNames[cid] = name;
                                    functionCallIds[cid] = cid;
                                    functionCallArgs[cid] = new StringBuilder(argsStr);
                                }
                            }
                            break;

                        case "response.completed":
                            // Extract response ID and usage
                            if (root.TryGetProperty("response", out var respObj))
                            {
                                if (respObj.TryGetProperty("id", out var idProp))
                                    result.ResponseId = idProp.GetString();

                                if (respObj.TryGetProperty("usage", out var usageProp))
                                {
                                    var inputTokens = usageProp.TryGetProperty("input_tokens", out var it)
                                        ? it.GetInt32() : 0;
                                    var outputTokens = usageProp.TryGetProperty("output_tokens", out var ot)
                                        ? ot.GetInt32() : 0;
                                    result.Usage = (inputTokens, outputTokens);
                                }
                            }
                            break;
                    }
                }
            }
            catch (JsonException)
            {
                // Skip malformed SSE lines
            }
        }

        // Build tool call list from accumulated data
        foreach (var kvp in functionCallNames)
        {
            var callId = kvp.Key;
            var name = kvp.Value;
            var args = functionCallArgs.TryGetValue(callId, out var sb) ? sb.ToString() : "{}";
            result.ToolCalls.Add(new ToolCallInfo(callId, name, args));
        }

        return result;
    }

    private class StreamResult
    {
        public List<ChatStreamChunk> TextChunks { get; } = new();
        public List<ToolCallInfo> ToolCalls { get; } = new();
        public string? ResponseId { get; set; }
        public (int InputTokens, int OutputTokens)? Usage { get; set; }
    }

    private record ToolCallInfo(string CallId, string Name, string Arguments);

    #endregion

    #region Helpers

    private static string ExtractOutputText(JsonElement responseJson)
    {
        // Responses API output structure: { output: [ { type: "message", content: [ { type: "output_text", text: "..." } ] } ] }
        if (responseJson.TryGetProperty("output", out var outputArray) &&
            outputArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var outputItem in outputArray.EnumerateArray())
            {
                if (outputItem.TryGetProperty("content", out var contentArray) &&
                    contentArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contentItem in contentArray.EnumerateArray())
                    {
                        if (contentItem.TryGetProperty("type", out var t) &&
                            t.GetString() == "output_text" &&
                            contentItem.TryGetProperty("text", out var textProp))
                        {
                            return textProp.GetString() ?? "";
                        }
                    }
                }
                // Also handle direct text property
                if (outputItem.TryGetProperty("text", out var directText))
                {
                    return directText.GetString() ?? "";
                }
            }
        }

        // Fallback: try output_text directly
        if (responseJson.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? "";
        }

        return "";
    }

    private void LogUsage(JsonElement responseJson, string endpoint, long latencyMs)
    {
        if (responseJson.TryGetProperty("usage", out var usage))
        {
            var inputTokens = usage.TryGetProperty("input_tokens", out var it) ? it.GetInt32() : 0;
            var outputTokens = usage.TryGetProperty("output_tokens", out var ot) ? ot.GetInt32() : 0;
            LogUsage(inputTokens, outputTokens, endpoint, latencyMs);
        }
    }

    private void LogUsage(int inputTokens, int outputTokens, string endpoint, long latencyMs)
    {
        _logger.LogInformation(
            "OpenAI API call completed. Model={Model}, InputTokens={InputTokens}, OutputTokens={OutputTokens}, Endpoint={Endpoint}, Latency={Latency}ms",
            _options.Model, inputTokens, outputTokens, endpoint, latencyMs);
    }

    private static DashboardInsightDto BuildFallbackInsight()
    {
        return new DashboardInsightDto
        {
            Summary = "AI analizi şu an kullanılamıyor.",
            RiskExplanation = "OpenAI servisi erişilemez durumda. Lütfen daha sonra tekrar deneyin.",
            RecommendedAction = "Manuel olarak dashboard verilerini inceleyebilirsiniz.",
            Warnings = new List<string> { "OpenAI servisi erişilemez." }
        };
    }

    private static string BuildInsightsSystemPrompt()
    {
        return """
            Sen bir ERP asistanısın. Sana sağlanan gerçek ERP verilerini analiz et ve yorumla.
            Veriler:
            - Dashboard özet metrikleri (satış, alış, stok, nakit akışı)
            - Kritik stok ürünleri
            - ML talep tahminleri

            KURALLAR:
            1. Yalnızca sana sağlanan verileri kullan. Hiçbir rakamı kendinden üretme.
            2. Türkçe yanıt ver.
            3. Stok riski yüksek ürünleri vurgula.
            4. Satın alma önerilerini tahmin verisine dayandır.
            5. "Gerçek veri" ile "ML tahmini" arasındaki farkı açıkça belirt.
            6. Kısa ve öz yanıt ver. Gereksiz uzatma.
            """;
    }

    private static string BuildChatSystemPrompt()
    {
        return """
            Sen MikroProje ERP sisteminin AI asistanısın.
            Kullanıcının ERP ile ilgili sorularını gerçek backend verileri üzerinden cevaplıyorsun.

            KURALLAR:
            1. Verileri sana sağlanan tool'lar üzerinden al. Rakam uydurma.
            2. Yalnızca read-only sorgular yapabilirsin. Veri oluşturma/güncelleme/silme yapamazsın.
            3. SQL sorgusu oluşturma veya çalıştırma.
            4. Türkçe yanıt ver.
            5. "Gerçek veri" ile "ML tahmini" arasındaki farkı açıkça belirt. Veri yoksa tahmin uydurma.
            6. Eğer bir tool sonucu hata dönerse, bunu kullanıcıya nazikçe açıkla.
            7. Kısa ve anlaşılır yanıtlar ver. Tablo formatı kullanabilirsin.
            8. ML (Makine Öğrenimi) tool'u (get_product_forecast vb.) tarafından dönen sayısal değerleri, metric'leri (forecast7Days, vb.) ve metinleri (riskLevel) asla değiştirme ve aynen aktar.
            9. Tool'dan gelen recommendedPurchaseQuantity > 0 ise, "satın alma gerekmiyor" deme. Tool çıktısı ile doğal dil cevabın kesinlikle çelişemez.
            """;
    }

    private static JsonElement GetDashboardInsightSchema()
    {
        var schemaJson = """
        {
            "type": "object",
            "properties": {
                "summary": { "type": "string" },
                "risk_explanation": { "type": "string" },
                "recommended_action": { "type": "string" },
                "warnings": {
                    "type": "array",
                    "items": { "type": "string" }
                }
            },
            "required": ["summary", "risk_explanation", "recommended_action", "warnings"],
            "additionalProperties": false
        }
        """;
        return JsonSerializer.Deserialize<JsonElement>(schemaJson);
    }

    #endregion
}
