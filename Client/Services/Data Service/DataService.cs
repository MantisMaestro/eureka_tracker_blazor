﻿using Client.Models;
using EurekaDb.Context;
using EurekaDb.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Client.Services.Data_Service;

public class DataService(EurekaContext eurekaContext) : IDataService
{
    public async Task<List<Player>> GetOnlinePlayers()
    {
        return await eurekaContext
            .Players
            .Where(x => x.LastOnline == "now")
            .ToListAsync();
    }

    public async Task<List<PlayerPlaytime>> GetDayTopPlayers(int limit)
    {
        return await GetTopPlayers(limit, DateOnly.FromDateTime(DateTime.Today), null);
    }

    public async Task<List<PlayerPlaytime>> GetWeekTopPlayers(int limit = 10)
    {
        // get the date of the start of this week
        var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
        var weekStartDate = DateOnly.FromDateTime(weekStart);

        return await GetTopPlayers(limit, weekStartDate, null);
    }

    public async Task<List<PlayerPlaytime>> GetMonthTopPlayers(int limit)
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthStartDate = DateOnly.FromDateTime(monthStart);
        
        return await GetTopPlayers(limit, monthStartDate, null);
    }

    public async Task<List<PlayerPlaytime>> GetMapTopPlayers(int limit)
    {
        // TODO: Have this based on a config file or the database!
        var mapStart = new DateOnly(2024, 07, 26);
        return await GetTopPlayers(limit, mapStart, null);
    }

    public async Task<PlayerQuery> GetPlayerSessions(string playerName)
    {
        var playerId = eurekaContext.Players
            .First(x => x.Name == playerName)
            .Id;

        var startDate = DateTime.Today.AddMonths(-1);
        var startDateOnly = DateOnly.FromDateTime(startDate);
        
        var sessions = await eurekaContext.PlayerSessions
            .Where(x => x.PlayerId == playerId && x.Date >= startDateOnly)
            .Include(x => x.Player)
            .ToListAsync();
        
        var totalPlaytime = sessions.Sum(x => x.TimePlayedInSession ?? 0);

        var dates = sessions.Select(x => x.Date).ToList();
        
        var today = DateOnly.FromDateTime(DateTime.Today);
        for (var date = startDateOnly; date < today; date = date.AddDays(1))
        {
            if (!dates.Contains(date))
                sessions.Add(new PlayerSession
                {
                    Date = date,
                    TimePlayedInSession = 0
                });
        }
        
        sessions = sessions.OrderBy(x => x.Date).ToList();
        
        return new PlayerQuery
        {
            PlayerSessions = sessions,
            TotalPlaytime = totalPlaytime
        };
    }

    private async Task<List<PlayerPlaytime>> GetTopPlayers(int limit, DateOnly startDate, DateOnly? endDate)
    {
        endDate ??= DateOnly.FromDateTime(DateTime.Today);
        
        var sessionsThisWeek = await eurekaContext
            .PlayerSessions
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .Include(x => x.Player)
            .ToListAsync();
        
        var groupedPlayerSessions = sessionsThisWeek.GroupBy(x => x.PlayerId);
        
        var result = new List<PlayerPlaytime>();
        foreach (var groupedPlayerSession in groupedPlayerSessions)
            result.Add(new PlayerPlaytime
            {
                PlayerId = groupedPlayerSession.Key,
                PlayerName = groupedPlayerSession.First().Player.Name,
                Playtime = groupedPlayerSession.Sum(x => x.TimePlayedInSession ?? 0)
            });
        
        result = result.OrderByDescending(x => x.Playtime).ToList();
        return result.Take(limit).ToList();
    }
}