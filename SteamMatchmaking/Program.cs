using System;
using System.Linq;
using LiteDB;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting up...");

            Console.Write("Adding 76561197967963688...");
            var me = new Player {Id = 76561197967963688};

            var steamService = new SteamUserService();

            using (var db = Functions.GetDb())
            {
                BsonMapper.Global.Entity<Player>().DbRef(p => p.Games, "games");
                BsonMapper.Global.Entity<Player>().DbRef(p => p.Friends, "friends");


                var players = db.GetCollection<Player>("players").Include(p => p.Games);
                players.EnsureIndex(p => p.Id);
                var games = db.GetCollection<Game>("games").Include(g => g.Player);
                games.EnsureIndex(g => g.Id);
                players.Upsert(me);
                Console.WriteLine("Done");
                Console.Write("Adding friends...");

                me.Friends = steamService.GetFriends(me).ToList();

                Console.WriteLine("Done");
                //Console.WriteLine("Adding players from Valve group...");
                //var valvePlayers = steamService.GetPlayersFromGroup(103582791429521412).Take(50);

                //var newPlayers =
                //(from p1 in valvePlayers
                //    from p2 in players.FindAll()
                //    where p1.Id != p2.Id
                //    select p1).ToList();

                //var count = newPlayers.Count;
                //var i = 0;
                //foreach (var newPlayer in newPlayers)
                //{
                //if (i % 50 == 0)

                //Console.WriteLine($"Adding {i} of {count} ({newPlayer})..{players.Upsert(newPlayer)}");
                //players.Upsert(newPlayer);
                //i++;
                //}
                var i = 0;
                var count = me.Friends.Count;
                 foreach (var friend in me.Friends)
                {
                    //if (i % 50 == 0)

                        Console.WriteLine($"Adding {i} of {count} ({friend})..{players.Upsert(friend)}");
                    //players.Upsert(newPlayer);
                    i++;
                }
                Console.WriteLine($"New player count: {players.Count()}");
                Console.WriteLine("Done");

                var playerCount = players.Count();
                var playerIndex = 0;

                steamService.OnFillPlayerInfo.Subscribe(player => Console.WriteLine($"Filling in {playerIndex++} of {playerCount}: {player}"));
                steamService.OnFillPlayerInfo.Subscribe(player => players.Upsert(player));

                steamService.OnCalculatePlayerMetric.Subscribe(player => Console.WriteLine(
                    $"Calculating player {playerIndex++} of {playerCount}: {player.DisplayName()}"));
                steamService.OnCalculatePlayerMetric.Subscribe(player => players.Upsert(player));

                steamService.OnCalculateGameMetric.Subscribe(metric => Console.WriteLine(
                    $"Calculating... Player {metric.PlayerIndex} of {playerCount} :{metric.Player.Name}, Game {metric.GameIndex} of {metric.GameCount}: {metric.Game.Name}"));
                steamService.OnCalculateGameMetric.Subscribe(metric => games.Upsert(metric.Game));
                steamService.OnCalculateGameMetric.Subscribe(metric => players.Upsert(metric.Player));

                Console.WriteLine("Filling in player info...");

                steamService.FillInPlayerInfo(players.FindAll(), true);

                Console.WriteLine("Done");

                playerIndex = 0;


                Console.WriteLine("Calculating player indices...");
                steamService.CalculatePlayerMetrics(players.FindAll());
                Console.WriteLine("Done.");
                Console.WriteLine("Player table size:{0}", players.Count());
                Console.WriteLine("Game table size:{0}", games.Count());

                Console.WriteLine("FINISHED.");
                Console.Read();
            }
        }
    }
}