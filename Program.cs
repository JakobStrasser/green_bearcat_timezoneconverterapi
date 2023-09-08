using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System.Text.Json;
using System.Text.Json.Serialization;
//using System.Web.Http.Results;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
var AllowAnyHost = "_AllowAnyHost";
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Timezone converter",
        Version = "v1"
    });
});
builder.Services.AddCors(options =>
options.AddPolicy(name: AllowAnyHost, policy => policy.AllowAnyOrigin().AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(AllowAnyHost);

var timezones = TimeZoneInfo.GetSystemTimeZones();

app.MapGet("/timezones", () =>
{
    string html = "";
    foreach (TimeZoneInfo t in timezones.ToList().OrderBy(t => t.Id))
    {
        html += $"<option value={t.Id.Replace(" ", "_")}>{t.Id} ({t.BaseUtcOffset})</option>";
    }
    return html;
})
.WithName("GetTimezones")
.WithOpenApi();



app.MapPost("/convert", ([FromBody] ConverterRequestBody parameters) =>
{
   

    if (System.String.IsNullOrEmpty(parameters.Targettimezone))
    {
        return Results.BadRequest($"Missing target timezone!");
    }
    if (System.String.IsNullOrEmpty(parameters.Sourcetimezone))
    {
        return Results.BadRequest($"Missing source timezone!");
    }
    TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(parameters.Targettimezone.Replace("_", " "));
    TimeZoneInfo sourceZone = TimeZoneInfo.FindSystemTimeZoneById(parameters.Sourcetimezone.Replace("_", " "));
    DateTime targetDate;
    if (targetZone is null)
        return Results.BadRequest($"Could not parse target timezone");
    if (sourceZone is null)
        return Results.BadRequest($"Could not parse source timezone");

    if (targetZone is not null && sourceZone is not null)
    {
        targetDate = TimeZoneInfo.ConvertTime(parameters.Sourcedatetime, sourceZone, targetZone);
        string html = $" <span id=\"targetdatetime\">{targetDate.ToString()}</span>";
        return Results.Ok(html);

    }

    return Results.Empty;
}).WithName("convert");


app.Run();

public class ConverterRequestBody
{
    public required DateTime Sourcedatetime { get; set; }
    public required string Sourcetimezone { get; set; }
    public required string Targettimezone { get; set; }
}
