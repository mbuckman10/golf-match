using GolfMatchPro.Api.Hubs;
using GolfMatchPro.Data;
using GolfMatchPro.Engine.BestBall;
using GolfMatchPro.Engine.GrandTotals;
using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Individual;
using GolfMatchPro.Engine.Investments;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Engine.RoundRobin;
using GolfMatchPro.Engine.Skins;
using GolfMatchPro.Engine.Teams;
using GolfMatchPro.Engine.Tournament;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<GolfMatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GolfMatchDb")));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Engine services
builder.Services.AddSingleton<IHandicapCalculator, HandicapCalculator>();
builder.Services.AddSingleton<INassauCalculator, NassauCalculator>();
builder.Services.AddSingleton<ITeamBetCalculator, TeamBetCalculator>();
builder.Services.AddSingleton<IIndividualBetCalculator, IndividualBetCalculator>();
builder.Services.AddSingleton<IBestBallCalculator, BestBallCalculator>();
builder.Services.AddSingleton<ISkinsCalculator, SkinsCalculator>();
builder.Services.AddSingleton<ITournamentCalculator, TournamentCalculator>();
builder.Services.AddSingleton<IRoundRobinCalculator, RoundRobinCalculator>();
builder.Services.AddSingleton<IGrandTotalCalculator, GrandTotalCalculator>();

// SignalR
builder.Services.AddSignalR();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");
app.MapControllers();
app.MapHub<MatchHub>("/hubs/match");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
