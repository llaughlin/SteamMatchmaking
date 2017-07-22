using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Serilog;
using SteamMatchmaking.Data;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    public static class XmlExtensions
    {
        public static XDocument ToXDoc(this string source) => XDocument.Parse(source);

        public static string ElementValue(this XElement element, string elementName) => element.Element(elementName)?.Value;
    }

    public class SteamUserService
    {
        public const string FriendListQuery =
            "http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={0}&steamid={1}&relationship=friend&format=xml";

        public const string PlayerSummariesQuery =
            "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}&format=xml";

        public const string PlayerGamesQuery = "http://steamcommunity.com/profiles/{0}/games?xml=1";
        public const string GroupQuery = "http://steamcommunity.com/gid/{0}/memberslistxml/?xml=1";
        public const string ApiKey = "4E2FDA54EFCA850A20E2B5FD7949ED47";
        private readonly ILogger _Log;

        public SteamUserService(SteamMatchmakingContext context)
        {
            Context = context;
            _Log = Log.ForContext("SourceContext", nameof(SteamUserService));
        }

        protected SteamMatchmakingContext Context { get; set; }

        private IEnumerable<Game> GetGames(Player player)
        {
            return GetGames(player.Id);
        }

        public IEnumerable<Game> GetGames(long steamId)
        {
            var url = string.Format(PlayerGamesQuery, steamId);
            var result = Functions.DownloadString(url).ToXDoc();
            var games = result.XPathSelectElements("//game");
            return games.Select(node => new Game
            {
                GameId = long.Parse(node.ElementValue("appID")),
                Name = node.ElementValue("name"),
                LogoUrl = node.ElementValue("logo"),
                StoreLink = node.ElementValue("storeLink"),
                RecentHoursPlayed = double.Parse(node.ElementValue("hoursLast2Weeks") ?? "0"),
                TotalHoursPlayed = double.Parse(node.ElementValue("hoursOnRecord") ?? "0")
            })
                .Where(g => g.TotalHoursPlayed > 0);
        }

        public void FillInPlayerInfo(IEnumerable<Player> players, bool includeGames)
        {
            var playerIdList = string.Join(",", players.Select(p => p.Id));
            var url = string.Format(PlayerSummariesQuery, ApiKey, playerIdList);
            var result = Functions.DownloadString(url).ToXDoc();
            var xPlayers = result.XPathSelectElements("//player").ToList();
            var index = 0;
            Action<XElement> action = xPlayer =>
            {
                var steamId = long.Parse(xPlayer.ElementValue("steamid"));
                var name = xPlayer.ElementValue("personaname");
                var realName = xPlayer.ElementValue("realname") ?? "N/A";
                var player = players.First(u => u.Id == steamId);
                player.Name = name;
                player.RealName = realName;
                if (includeGames)
                {
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

                OnFillPlayerInfo(index, player);
                Interlocked.Increment(ref index);
            };

            Parallel.ForEach(xPlayers, action);
        }

        public void CalculatePlayerMetrics(IEnumerable<Player> source)
        {
            var i = 0;
            foreach (var thisPlayer in source)
            {
                var otherPlayers = source.Where(p => p.Id != thisPlayer.Id).ToList();
                var games = thisPlayer.Games.ToList();

                foreach (var otherPlayer in otherPlayers)
                {
                    var existingPlayerIndex = Context.Rankings.FindOne(
                                                  r => r.Player1.Id == thisPlayer.Id && r.Player2.Id == otherPlayer.Id)
                                              ?? new PlayerIndex { Player1 = thisPlayer, Player2 = otherPlayer };

                    var j = 0;
                    foreach (var thisPlayersGame in games)
                    {
                        var matchingOtherPlayersGame =
                            otherPlayer.Games.FirstOrDefault(g => g.GameId == thisPlayersGame.GameId);
                        if (matchingOtherPlayersGame == null)
                        {
                            // other player doesn't own this game
                            j++;
                            continue;
                        }
                        var existingGameIndex = existingPlayerIndex.GameIndexes.FirstOrDefault(
                                                    gi => gi.GameId == thisPlayersGame.GameId)
                                                ?? new GameIndex { GameId = thisPlayersGame.GameId };
                        existingGameIndex.Playtime1 = thisPlayersGame.TotalHoursPlayed;
                        existingGameIndex.Playtime2 = matchingOtherPlayersGame.TotalHoursPlayed;

                        OnCalculateGameMetric(i, j, games.Count, thisPlayer, thisPlayersGame);
                        j++;
                    }
                }
                i++;
            }
        }

        public IEnumerable<Player> GetFriends(Player player)
        {
            return GetFriends(player.Id);
        }


        public IEnumerable<Player> GetFriends(long steamId)
        {
            var url = string.Format(FriendListQuery, ApiKey, steamId);
            var result = Functions.DownloadString(url);
            var friends = result.ToXDoc().XPathSelectElements("//friend");
            return friends.Select(node => new Player { Id = long.Parse(node.ElementValue("steamid")) });
        }

        public IEnumerable<Player> GetPlayersFromGroup(long groupId)
        {
            var url = string.Format(GroupQuery, groupId);
            var result = Functions.DownloadString(url).ToXDoc();

            var xPlayers = result.XPathSelectElements("//steamID64");

            return xPlayers.Select(node => new Player { Id = long.Parse(node.Value) });
        }

        public event Action<long, Player> OnFillPlayerInfo;
        public event Action<long, Player> OnCalculatePlayerMetric;
        public event Action<long, long, int, Player, Game> OnCalculateGameMetric;
    }
}