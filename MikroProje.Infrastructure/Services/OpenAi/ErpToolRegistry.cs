using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi;

/// <summary>
/// Manages the whitelist of allowed read-only ERP tools for OpenAI function calling.
/// </summary>
public class ErpToolRegistry
{
    private readonly Dictionary<string, IErpToolHandler> _tools;

    public ErpToolRegistry(IEnumerable<IErpToolHandler> handlers)
    {
        _tools = handlers.ToDictionary(h => h.ToolName, h => h);
    }

    /// <summary>Get a tool handler by name. Returns null if not whitelisted.</summary>
    public IErpToolHandler? GetTool(string toolName)
    {
        _tools.TryGetValue(toolName, out var handler);
        return handler;
    }

    public List<object> GetToolDefinitions()
    {
        return _tools.Values.Select(t => (object)new
        {
            type = "function",
            name = t.ToolName,
            description = t.Description,
            parameters = t.ParametersSchema,
            strict = true
        }).ToList();
    }

    /// <summary>Get all registered tool names.</summary>
    public IReadOnlyCollection<string> GetRegisteredToolNames()
    {
        return _tools.Keys.ToList().AsReadOnly();
    }
}
