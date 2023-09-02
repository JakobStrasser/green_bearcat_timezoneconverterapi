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


app.MapPost("/convert", [Consumes("application/x-www-form-urlencoded")] IResult ([FromForm] string sourcedatetime, [FromForm] string sourcetimezone, [FromForm] string targettimezone) =>
{
    DateTime date;
    if (!DateTime.TryParse(sourcedatetime, out date))
    {
        return Results.BadRequest($"Could not parse date parameter {sourcedatetime}");
    }
    if (System.String.IsNullOrEmpty(targettimezone))
    {
        return Results.BadRequest($"Missing target timezone!");
    }
    if (System.String.IsNullOrEmpty(sourcetimezone))
    {
        return Results.BadRequest($"Missing source timezone!");
    }
    TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(targettimezone.Replace("_", " "));
    TimeZoneInfo sourceZone = TimeZoneInfo.FindSystemTimeZoneById(sourcetimezone.Replace("_", " "));
    DateTime targetDate;
    if (targetZone is null)
        return Results.BadRequest($"Could not parse target timezone");
    if (sourceZone is null)
        return Results.BadRequest($"Could not parse source timezone");

    if (targetZone is not null && sourceZone is not null)
    {
        targetDate = TimeZoneInfo.ConvertTime(date, sourceZone, targetZone);
        string html = $" <span id=\"targetdatetime\">{targetDate.ToString()}</span>";
        return Results.Ok(html);

    }

    return Results.Empty;
}).WithName("Convert");


app.Run();

