namespace MikroProje.Application.Interfaces;

/// <summary>
/// Read-only ERP tool handler interface for OpenAI function calling.
/// Each tool must be whitelisted and execute only read operations.
/// </summary>
public interface IErpToolHandler
{
    /// <summary>Unique tool name matching the OpenAI function name.</summary>
    string ToolName { get; }
    
    /// <summary>Human-readable description sent to OpenAI.</summary>
    string Description { get; }
    
    /// <summary>JSON Schema object describing function parameters.</summary>
    object ParametersSchema { get; }
    
    /// <summary>
    /// Execute the tool with provided arguments JSON. 
    /// userId is provided by the backend JWT claims for authorization limits.
    /// Returns result as JSON string.
    /// </summary>
    Task<string> ExecuteAsync(string argumentsJson, string userId, CancellationToken ct);
}
