using GamerLog.Client;
using GamerLog.Settings;
using Microsoft.EntityFrameworkCore;
using GamerLog.Configuration;
using Microsoft.Extensions.Options;
using GamerLog.Services;

// ----------Create builder ---------- \\
var builder = WebApplication.CreateBuilder(args);
// -------------------------------------- \\

// ---------- Create db connection ---------- \\
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GamerLog.Data.GamerLogContext>(options =>
    options.UseNpgsql(connectionString));
// -------------------------------------- \\

// ---------- INJECT SECRETS ---------- \\
// Icdb Api Settings
builder.Services.Configure<IcdbApiSettings>(builder.Configuration.GetSection("IcdbApiSettings"));
// Twitch Api Settings
builder.Services.Configure<TwitchApiSettings>(builder.Configuration.GetSection("TwitchApiSettings"));
//-------------------------------------- \\

// ---------- Add Services ---------- \\

// inject http clients and dependencies{
builder.Services.AddHttpClient<IcbdTokenConfig>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<TwitchApiSettings>>().Value;
    if (!string.IsNullOrEmpty(settings.BaseUrl)) 
    {
        client.BaseAddress = new Uri(settings.BaseUrl);
    }
});

builder.Services.AddHttpClient<IGameClient, IcdbClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<IcdbApiSettings>>().Value;
    var url = settings.BaseUrl.EndsWith("/") ? settings.BaseUrl : $"{settings.BaseUrl}/";
    client.BaseAddress = new Uri(settings.BaseUrl);
});

builder.Services.AddHttpClient<IcbdTokenConfig>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<TwitchApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

// }
builder.Services.AddScoped<GameSyncService>();
// register taskscheduler service{
builder.Services.AddHostedService<TaskSchedulerService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<TaskSchedulerService>>();
    var executionCalendar = new TimeSpan(9, 0, 0);
    return new TaskSchedulerService(logger, executionCalendar, provider);
});

// }

// ---------- Baisc Configuration ---------- \\
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
// -------------------------------------- \\

app.Run();
