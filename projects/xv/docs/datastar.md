# Datastar Snippets

 SSE Patch, would want this to render a blazor component instead
```
// Adds Datastar as a service
builder.Services.AddDatastar();

app.MapGet("/", async (IDatastarService datastarService) =>
{
    // Patches elements into the DOM.
    await datastarService.PatchElementsAsync(@"<div id=""question"">What do you put in a toaster?</div>");

    // Patches signals.
    await datastarService.PatchSignalsAsync(new { response = "", answer = "bread" });
});
```

KV Response with Content Negotiation
```
app.MapGet("/kv/{vault}/{key}", async (HttpContext ctx, string vault, string key) =>
{
    // Simulate retrieving value from vault
    var value = $"value-for-{key}-in-{vault}";

    if (ctx.Request.Headers.Accept.ToString().Contains("text/html"))
    {
        // Return HTML for browser requests
        return Results.Content($@"
            <div class='kv-entry'>
                <h3>Vault: {vault}</h3>
                <p>Key: {key}</p>
                <p>Value: {value}</p>
            </div>
        ", "text/html");
    }
    else
    {
        // Return JSON for API requests
        return Results.Json(new { vault, key, value });
    }
});
```
