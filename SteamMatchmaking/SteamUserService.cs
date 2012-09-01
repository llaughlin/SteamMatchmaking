using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using CustomExtensions;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    public class SteamUserService
    {
        public const string FriendListQuery = "http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={0}&steamid={1}&relationship=friend&format=xml";
        public const string PlayerSummariesQuery = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}&format=xml";
        public const string PlayerGamesQuery = "http://steamcommunity.com/profiles/{0}/games?xml=1";

        public const string ApiKey = "4E2FDA54EFCA850A20E2B5FD7949ED47";

        private static IEnumerable<Game> GetGames(Player player)
        {
            return GetGames(player.SteamId);
        }

        public static IEnumerable<Game> GetGames(long steamId)
        {
            var url = String.Format(PlayerGamesQuery, steamId);
            var result = Functions.DownloadString(url).ToXDoc();
            var games = result.XPathSelectElements("//game");
            return games.Select(node => new Game
                                            {
                                                GameId = node.ElementValue("appID").To<long>(),
                                                Name = node.ElementValue("name"),
                                                LogoUrl = node.ElementValue("logo"),
                                                StoreLink = node.ElementValue("storeLink"),
                                                RecentHoursPlayed =
                                                    (node.ElementValue("hoursLast2Weeks") ?? "0").To<double>(),
                                                TotalHoursPlayed =
                                                    (node.ElementValue("hoursOnRecord") ?? "0").To<double>(),
                                            })
                .Where(g => g.TotalHoursPlayed > 0);
        }

        public static void FillInPlayerInfo(Player player, bool includeGames)
        {
            FillInPlayerInfo(new List<Player> { player }, includeGames);
        }

        public static void FillInPlayerInfo(IEnumerable<Player> players, bool includeGames)
        {
            var playerIdList = String.Join(",", players.Select(p => p.SteamId));
            var url = String.Format(PlayerSummariesQuery, ApiKey, playerIdList);
            var result = Functions.DownloadString(url).ToXDoc();
            var xPlayers = result.XPathSelectElements("//player");
            foreach (var xPlayer in xPlayers)
            {
                var steamId = xPlayer.ElementValue("steamid").To<long>();
                var name = xPlayer.ElementValue("personaname");
                var realName = xPlayer.ElementValue("realname") ?? "N/A";
                var player = players.First(u => u.SteamId == steamId);
                player.Name = name;
                player.RealName = realName;
                if (!includeGames) continue;
                var games = GetGames(player);
                foreach (var game in games)
                {
                    var matchingGame = player.Games.SingleOrDefault(g => g.GameId == game.GameId);
                    if (matchingGame == null)
                    {
                        player.Games.Add(game);
                    }
                    else
                    {
                        matchingGame.Name = game.Name;
                        matchingGame.RecentHoursPlayed = game.RecentHoursPlayed;
                        matchingGame.TotalHoursPlayed = game.TotalHoursPlayed;
                        matchingGame.LogoUrl = game.LogoUrl;
                        matchingGame.StoreLink = game.StoreLink;
                    }
                }
            }
        }

        public static IEnumerable<Player> GetFriends(long steamId)
        {
            var url = String.Format(FriendListQuery, ApiKey, steamId);
            var result = Functions.DownloadString(url);
            var friends = result.ToXDoc().XPathSelectElements("//friend");
            return friends.Select(node => new Player { SteamId = node.ElementValue("steamid").To<long>() });
        }



    }
}