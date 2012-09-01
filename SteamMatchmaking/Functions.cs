using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using SteamMatchmaking.Infrastructure;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting up...");
            Database.SetInitializer(new SteamMatchmakingContextInitializer());
            var context = new SteamMatchmakingContext();

            Console.Write("Adding 76561197967963688...");
            context.Players.Add(new Player() { SteamId = 76561197967963688 });
            context.SaveChanges();
            Console.WriteLine("Done");
            Console.Write("Filling in Player info...");
            context.Players.First().FillInPlayerInfo(true);
            context.SaveChanges();
            Console.WriteLine("Done");
            Console.Write("Syncing friends...");
            context.Players.First().SyncFriends();
            context.SaveChanges();
            Console.WriteLine("Done");
            Console.WriteLine("Player table size:{0}", context.Players.Count());
            Console.WriteLine("Game table size:{0}", context.Games.Count());
            Console.Read();
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

    }
}
