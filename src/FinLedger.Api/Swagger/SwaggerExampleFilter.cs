using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinLedger.Api.Swagger;

/// <summary>
/// Adds request/response examples to Swagger for FinLedger endpoints.
/// </summary>
public class SwaggerExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var operationId = operation.OperationId ?? "";
        var path = context.ApiDescription.RelativePath ?? "";

        if (operationId.Contains("CreateAccount") || path.Contains("accounts") && context.ApiDescription.HttpMethod == "POST")
        {
            operation.Summary = "Create a new ledger account";
            operation.Description = "Creates an account (e.g. Cash, Revenue, Merchant). Code must be unique.";
            if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) == true)
            {
                mediaType.Example = new OpenApiObject
                {
                    ["name"] = new OpenApiString("Cash"),
                    ["code"] = new OpenApiString("CASH"),
                    ["type"] = new OpenApiInteger(0)
                };
            }
        }
        else if (operationId.Contains("GetBalance") || path.Contains("balance"))
        {
            operation.Summary = "Get account balance";
            operation.Description = "Returns the current balance for an account. Asset/Expense: debits - credits. Liability/Equity/Revenue: credits - debits.";
        }
        else if (operationId.Contains("CreateTransaction") || path.Contains("transactions") && context.ApiDescription.HttpMethod == "POST")
        {
            operation.Summary = "Create a transaction (idempotent)";
            operation.Description = "Creates a double-entry transaction. Entries must balance (sum of debits = sum of credits). Use Idempotency-Key header to prevent duplicates on retry.";
            if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) == true)
            {
                mediaType.Example = new OpenApiObject
                {
                    ["idempotencyKey"] = new OpenApiString("tx-001"),
                    ["description"] = new OpenApiString("Sale - payment received"),
                    ["entries"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["accountId"] = new OpenApiString("00000000-0000-0000-0000-000000000001"),
                            ["amount"] = new OpenApiDouble(100),
                            ["side"] = new OpenApiInteger(0)
                        },
                        new OpenApiObject
                        {
                            ["accountId"] = new OpenApiString("00000000-0000-0000-0000-000000000002"),
                            ["amount"] = new OpenApiDouble(100),
                            ["side"] = new OpenApiInteger(1)
                        }
                    }
                };
            }
        }
    }
}
