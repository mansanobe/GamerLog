namespace GamerLog.Services;
using Microsoft.Extensions.DependencyInjection;

public class TaskSchedulerService : BackgroundService
{
    private readonly ILogger<TaskSchedulerService> _logger;
    private readonly TimeSpan _mondayTimeOfDay;
    private readonly IServiceProvider _serviceProvider;
    
    public TaskSchedulerService(ILogger<TaskSchedulerService> logger, TimeSpan mondayTimeOfDay, IServiceProvider provider)
    {
        _logger = logger;
        _mondayTimeOfDay = mondayTimeOfDay;
        _serviceProvider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayToNextMonday(_mondayTimeOfDay);
            _logger.LogInformation("Waiting {Delay} until next Monday run", delay);
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException) { break; }
            
            await RunJobAsync(stoppingToken);
        }
    }
    
    private TimeSpan GetDelayToNextMonday(TimeSpan timeOfDay)
    {
        var now = DateTime.Now;
        int daysToAdd = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        var candidate = now.Date.AddDays(daysToAdd).Add(timeOfDay);
        if (candidate <= now)
            candidate = candidate.AddDays(7);

        return candidate - now;
    }

    private async Task RunJobAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Weekly job started at {Now}", DateTime.Now);
            using var scope = _serviceProvider.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<GameSyncService>();
            await sync.SyncGenresFromApiAsync();
            await sync.SyncGamesFromApiAsync();
            await Task.CompletedTask;
            _logger.LogInformation("Weekly job finished at {Now}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running weekly job");
        }
    }
}