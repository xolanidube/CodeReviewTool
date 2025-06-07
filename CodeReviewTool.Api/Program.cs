using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RulesEngine.RulesEngine>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/validate", async (HttpRequest request, RulesEngine.RulesEngine engine) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data");
    }

    var target = request.Form["target"].ToString();
    if (string.IsNullOrEmpty(target)) target = "BluePrism";

    var file = request.Form.Files["process"];
    if (file == null)
    {
        return Results.BadRequest("Process file not provided");
    }

    var tempPath = Path.GetTempFileName();
    using (var stream = File.Create(tempPath))
    {
        await file.CopyToAsync(stream);
    }

    var configPath = $"rulesConfig.{target.ToLower()}.json";
    try
    {
        engine.LoadRuleConfig(configPath);
        engine.AddRulesFromConfig();

        if (target.Equals("BluePrism", StringComparison.OrdinalIgnoreCase))
            engine.Initialize(tempPath);
        else if (target.Equals("Python", StringComparison.OrdinalIgnoreCase))
            engine.LoadPythonFile(tempPath);
        else
            return Results.BadRequest($"Unsupported target {target}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }

    var results = engine.ValidateAllWithResults();
    File.Delete(tempPath);
    return Results.Json(results);
});

app.Run();
