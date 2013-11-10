using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp.Infrastructure;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Process()
        {
            Console.WriteLine("Starting up...");
            Database.SetInitializer(new SteamMatchmakingContextInitializer());
            var context = new SteamMatchmakingContext();
            var steamService = new SteamUserService(context);

            Console.Write("Adding 76561197967963688...");
            var me = new Player() { SteamId = 76561197967963688 };
            context.Players.Add(me);
            context.SaveChanges();
            Console.WriteLine("Done");
            Console.Write("Adding friends...");
            me.Friends = steamService.GetFriends(me).ToList();
            context.SaveChanges();
            Console.WriteLine("Done");
            Console.WriteLine("Adding players from Valve group...");
            var valvePlayers = steamService.GetPlayersFromGroup(103582791429521412).Take(50);
            var newPlayers =
                (from p1 in valvePlayers
                 from p2 in context.Players
                 where p1.SteamId != p2.SteamId
                 select p1).ToList();
            var count = newPlayers.Count;
            int i = 0;
            foreach (var newPlayer in newPlayers)
            {
                if (i % 50 == 0) Console.WriteLine("Adding {0} of {1}..", i, count);
                context.Players.Add(newPlayer);
                i++;
            }
            context.SaveChanges();
            Console.WriteLine("Done");

            Console.WriteLine("Filling in player info...");
            var playerCount = context.Players.Count();
            steamService.OnFillPlayerInfo += ((index, p) =>
                                              Console.WriteLine("Filling in {0} of {1}: {2}", index, playerCount, p.DisplayName()));
            steamService.FillInPlayerInfo(context.Players.ToList(), true);

            context.SaveChanges();
            Console.WriteLine("Done");

            steamService.OnCalculatePlayerMetric += (index, player) =>
                                                    Console.WriteLine("Calculating player {0} of {1}: {2}", index, playerCount, player.DisplayName());

            steamService.OnCalculateGameMetric += (playerIndex, gameIndex, gameCount, player, game) =>
                                                  Console.WriteLine("Calculating... Player {0} of {1} :{2}, Game {3} of {4}: {5}",
                                                                    playerIndex, playerCount, player.Name, gameIndex, gameCount, game.Name);

            Console.WriteLine("Calculating player indices...");
            steamService.CalculatePlayerMetrics(context.Players.ToList());
            context.SaveChanges();
            Console.WriteLine("Done.");
            Console.WriteLine("Player table size:{0}", context.Players.Count());
            Console.WriteLine("Game table size:{0}", context.Games.Count());

            Console.WriteLine("FINISHED.");
            Console.Read();

            return
                Json(
                    new
                        {
                            Players = context.Players.ToList(),
                            Games = context.Games.ToList(),
                            Randings = context.Rankings.ToList()
                        });
        }

    }

    public static class Functions
    {
        public static string DownloadString(string url)
        {
            try
            {
                return new WebClient().DownloadString(url);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    return null;
                }
                throw;
            }
        }

        public static string DisplayName(this Player player)
        {
            return string.Format("{0} ({1})", player.Name, player.RealName);
        }

    }
}