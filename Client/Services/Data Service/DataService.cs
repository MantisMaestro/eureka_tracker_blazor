using Client.Extensions;
using Client.Models;
using EurekaDb.Context;
using EurekaDb.Models;
using Microsoft.EntityFrameworkCore;

namespace Client.Services.Data_Service;

public class DataService(EurekaContext eurekaContext) : IDataService
{
    public async Task<List<Player>> GetOnlinePlayers()
    {
        var threshold = DateTime.UtcNow.AddSeconds(-90);
        return await eurekaContext
            .Players
            .Where(x => x.LastOnline >= threshold)
            .ToListAsync();
    }

    public async Task<int> GetTodayPlayerCount()
    {
        var date = DateOnly.FromDateTime(DateTime.Today - TimeSpan.FromDays(7));

        return await eurekaContext
            .PlayerSessions
            .Where(x => x.Date >= date)
            .GroupBy(x => x.PlayerId)
            .CountAsync();
    }

    public async Task<List<PlayerPlaytime>> GetDayTopPlayers(int limit)
    {
        return await GetTopPlayers(limit, DateOnly.FromDateTime(DateTime.Today), null);
    }

    public async Task<List<PlayerPlaytime>> GetWeekTopPlayers(int limit = 10)
    {
        var weekStart = DateOnly.FromDateTime(DateTime.Today).StartOfWeek(DayOfWeek.Monday);

        return await GetTopPlayers(limit, weekStart, null);
    }

    public async Task<List<PlayerPlaytime>> GetMonthTopPlayers(int limit)
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthStartDate = DateOnly.FromDateTime(monthStart);

        return await GetTopPlayers(limit, monthStartDate, null);
    }

    public async Task<List<PlayerPlaytime>> GetMapTopPlayers(int limit, DateOnly currentMapStartDate)
    {
        return await GetTopPlayers(limit, currentMapStartDate, null);
    }

    public async Task UpdateLedger(MCStatus.Player[] playerData, int elapsedSeconds)
    {
        foreach (var player in playerData)
        {
            await UpdatePlayers(player.Name, player.Uuid.ToString(), elapsedSeconds);
            await UpdateSessions(player.Name, player.Uuid.ToString(), elapsedSeconds);
        }

        await eurekaContext.SaveChangesAsync();
    }

    public async Task UpdatePlayers(string playerName, string playerId, int elapsedSeconds)
    {
        var player = await eurekaContext.Players
            .FirstOrDefaultAsync(x => x.Id == playerId);

        if (player is null)
        {
            await eurekaContext.AddAsync(new Player
            {
                Id = playerId,
                Name = playerName,
                LastOnline = DateTime.UtcNow,
                TotalPlayTime = elapsedSeconds
            });
        }
        else
        {
            player.LastOnline = DateTime.UtcNow;
            player.Name = playerName;
            player.TotalPlayTime = (player.TotalPlayTime ?? 0) + elapsedSeconds;
        }
    }

    public async Task UpdateSessions(string playerName, string playerId, int elapsedSeconds)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var session = await eurekaContext
            .PlayerSessions
            .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Date == today);

        if (session is null)
        {
            await eurekaContext.PlayerSessions.AddAsync(new PlayerSession
            {
                PlayerId = playerId,
                Date = today,
                TimePlayedInSession = elapsedSeconds
            });
        }
        else
        {
            session.TimePlayedInSession = (session.TimePlayedInSession ?? 0) + elapsedSeconds;
        }
    }

    private async Task<List<PlayerPlaytime>> GetTopPlayers(int limit, DateOnly startDate, DateOnly? endDate)
    {
        endDate ??= DateOnly.FromDateTime(DateTime.Today);

        return await eurekaContext.PlayerSessions
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .GroupBy(x => new { x.PlayerId, x.Player.Name })
            .Select(g => new PlayerPlaytime
            {
                PlayerId = g.Key.PlayerId,
                PlayerName = g.Key.Name,
                Playtime = g.Sum(x => x.TimePlayedInSession ?? 0)
            })
            .OrderByDescending(x => x.Playtime)
            .Take(limit)
            .ToListAsync();
    }
}