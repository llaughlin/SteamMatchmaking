using System;
using System.Linq;
using LiteDB;
using Serilog;
using SteamMatchmaking.Data;
using SteamMatchmaking.Models;
using Logger = Serilog.Core.Logger;

namespace SteamMatchmaking.Console
{
    internal class Program
    {
        private static Logger _Log;

        public Program()
        {
            Log.Logger = _Log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Log.Fatal(eventArgs.ExceptionObject as Exception, "Fatal error in application");
                Log.CloseAndFlush();
            };
            new Program().Run();

            Log.CloseAndFlush();

            System.Console.WriteLine("Finished");
        }

        private void Run()
        {
            _Log.Information("Starting up...");
            var context = new SteamMatchmakingContext(new LiteDatabase("steammatchmaking.db"));
            var steamService = new SteamUserService(context);

            _Log.Information("Adding 76561197967963688...");
            var me = new Player { Id = 76561197967963688 };
            context.Players.Update(me);
            _Log.Information("Adding friends...");
            me.Friends = steamService.GetFriends(me).ToList();
            _Log.Information("Adding players from Valve group...");
            var valvePlayers = steamService.GetPlayersFromGroup(103582791429521412).Take(50);
            _Log.Debug("Found {Count} valve players", valvePlayers.Count());
            var newPlayers =
            (from p1 in valvePlayers
             from p2 in context.Players.FindAll()
             where p1.Id != p2.Id
             select p1).ToList();

            var count = newPlayers.Count();
            var i = 0;
            foreach (var newPlayer in newPlayers)
            {
                if (i % 50 == 0) _Log.Debug("Adding {Index} of {Total}..", i, count);
                context.Players.Update(newPlayer);
                i++;
            }

            _Log.Information("Filling in player info...");
            var playerCount = context.Players.Count();
            steamService.OnFillPlayerInfo += (index, p) =>
                _Log.Information("Filling in {Index} of {PlayerCount}: {PlayerName}", index, playerCount,
                    p.DisplayName());
            steamService.FillInPlayerInfo(context.Players.FindAll(), true);


            steamService.OnCalculatePlayerMetric += (index, player) =>
                _Log.Information("Calculating player {Index} of {PlayerCount}: {PlayerName}", index, playerCount,
                    player.DisplayName());

            steamService.OnCalculateGameMetric += (playerIndex, gameIndex, gameCount, player, game) =>
                _Log.Information(
                    "Calculating... Player {PlayerIndex} of {PlayerCount} :{PlayerName}, Game {GameIndex} of {GameCount}: {GameName}",
                    playerIndex, playerCount, player.Name, gameIndex, gameCount, game.Name);

            _Log.Information("Calculating player indices...");
            steamService.CalculatePlayerMetrics(context.Players.FindAll());
            _Log.Information("Player table size:{PlayerCount}", context.Players.Count());
            _Log.Information("Game table size:{GameCount}", context.Games.Count());

            _Log.Information("Finished.");
        }
    }
}