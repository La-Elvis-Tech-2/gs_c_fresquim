using System.Text.Json.Serialization;
using Fresquim.Api.Infrastructure;
using Fresquim.Application;
using Fresquim.Infrastructure;
using Fresquim.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Camadas da aplicação (injeção de dependência).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Fresquim API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Backend da CitizenTech Fresquim (GS 2026.2): pontos de água e sombra, avisos do cidadão " +
            "e risco de calor na região da Av. Paulista, em São Paulo.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Cria/atualiza o banco SQLite pelas migrations e cadastra os pontos iniciais.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FresquimDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

// Documentação dos endpoints: /openapi/v1.json e interface Scalar em /scalar.
app.MapOpenApi();
app.MapScalarApiReference(o => o
    .WithTitle("Fresquim API")
    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
app.MapControllers();

await app.RunAsync();

/// <summary>Exposto para testes de integração.</summary>
public partial class Program;
