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

    engine.LoadRuleConfig("rulesConfig.json");
    engine.AddRulesFromConfig();
    engine.Initialize(tempPath);

    var results = engine.ValidateAllWithResults();
    File.Delete(tempPath);
    return Results.Json(results);
});

app.Run();
