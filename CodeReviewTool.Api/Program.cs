using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine;

var builder = WebApplication.CreateBuilder(args);

// create a fresh engine for each request to avoid stale state
builder.Services.AddScoped<RulesEngine.RulesEngine>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string GetConfigPath(HttpRequest request)
{
    var config = request.Query["config"].ToString();
    if (string.IsNullOrWhiteSpace(config))
    {
        return "rulesConfig.json";
    }
    return config;
}

async Task<IResult> ValidateFile(IFormFile file, HttpRequest request, RulesEngine.RulesEngine engine)
{
    if (file == null)
    {
        return Results.BadRequest("File not provided");
    }

    var tempPath = Path.GetTempFileName();
    using (var stream = File.Create(tempPath))
    {
        await file.CopyToAsync(stream);
    }

    engine.LoadRuleConfig(GetConfigPath(request));
    engine.AddRulesFromConfig();
    engine.Initialize(tempPath);

    var results = engine.ValidateAllWithResults();
    File.Delete(tempPath);
    return Results.Json(results);
}

app.MapPost("/api/validate/process", async (HttpRequest request, RulesEngine.RulesEngine engine) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data");
    }

    var file = request.Form.Files["process"];
    return await ValidateFile(file, request, engine);
});

app.MapPost("/api/validate/object", async (HttpRequest request, RulesEngine.RulesEngine engine) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data");
    }

    var file = request.Form.Files["object"];
    return await ValidateFile(file, request, engine);
});

app.Run();
