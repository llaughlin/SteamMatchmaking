using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    public class SteamUserService
    {
        public const string FriendListQuery =
                "http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={0}&steamid={1}&relationship=friend&format=xml"
            ;

        public const string PlayerSummariesQuery =
            "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}&format=xml";

        public const string PlayerGamesQuery = "http://steamcommunity.com/profiles/{0}/games?xml=1";
        public const string GroupQuery = "http://steamcommunity.com/gid/{0}/memberslistxml/?xml=1";
        public const string ApiKey = "4E2FDA54EFCA850A20E2B5FD7949ED47";
        public Subject<GameMetric> OnCalculateGameMetric = new Subject<GameMetric>();
        public Subject<Player> OnCalculatePlayerMetric = new Subject<Player>();

        public Subject<Player> OnFillPlayerInfo = new Subject<Player>();


        private IEnumerable<Game> GetGames(Player player)
        {
            return GetGames(player.Id);
        }

        public IEnumerable<Game> GetGames(long steamId)
        {
            var url = string.Format(PlayerGamesQuery, steamId);
            var result = XDocument.Parse(Functions.DownloadString(url));
            var games = result.XPathSelectElements("//game");
            return games.Select(node => new Game
                {
                    Id = long.Parse(node.Element("appID").Value),
                    Name = node.Element("name").Value,
                    LogoUrl = node.Element("logo").Value,
                    StoreLink = node.Element("storeLink").Value,
                    RecentHoursPlayed =
                        double.Parse(node.Element("hoursLast2Weeks")?.Value ?? "0"),
                    TotalHoursPlayed =
                        double.Parse(node.Element("hoursOnRecord")?.Value ?? "0")
                })
                .Where(g => g.TotalHoursPlayed > 0);
        }

        public void FillInPlayerInfo(IEnumerable<Player> players, bool includeGames)
        {
            var playerIdList = string.Join(",", players.Select(p => p.Id));
            var url = string.Format(PlayerSummariesQuery, ApiKey, playerIdList);
            var result = XDocument.Parse(Functions.DownloadString(url));
            var xPlayers = result.XPathSelectElements("//player").ToList();
            var index = 0;

            void Fill(XElement xPlayer)
            {
                var steamId = long.Parse(xPlayer.Element("steamid").Value);
                var name = xPlayer.Element("personaname").Value;
                var realName = xPlayer.Element("realname")?.Value ?? "N/A";
                var player = players.First(u => u.Id == steamId);
                player.Name = name;
                player.RealName = realName;
                if (includeGames)
                {
                    var games = GetGames(player);
                    foreach (var game in games)
                    {
                        var matchingGame = player.Games.SingleOrDefault(g => g.Id == game.Id);
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

                OnFillPlayerInfo.OnNext(player);
                Interlocked.Increment(ref index);
            }

            ;

            Parallel.ForEach(xPlayers, Fill);
        }

        public void CalculatePlayerMetrics(IEnumerable<Player> source)
        {
            using (var db = Functions.GetDb())
            {
                var playerIndex = 0;
                foreach (var thisPlayer in source)
                {
                    var otherPlayers = source.Where(p => p.Id != thisPlayer.Id).ToList();
                    var games = thisPlayer.Games.ToList();

                    foreach (var otherPlayer in otherPlayers)
                    {
                        var existingPlayerIndex = db.GetCollection<PlayerIndex>("rankings")
                                                      .FindOne(
                                                          r => r.Player1.Id == thisPlayer.Id &&
                                                               r.Player2.Id == otherPlayer.Id)
                                                  ?? new PlayerIndex {Player1 = thisPlayer, Player2 = otherPlayer};

                        var gameIndex = 0;
                        foreach (var thisPlayersGame in games)
                        {
                            var matchingOtherPlayersGame =
                                otherPlayer.Games.FirstOrDefault(g => g.Id == thisPlayersGame.Id);
                            if (matchingOtherPlayersGame == null)
                            {
                                // other player doesn't own this game
                                gameIndex++;
                                continue;
                            }
                            var existingGameIndex =
                                existingPlayerIndex.GameIndexes.FirstOrDefault(
                                    gi => gi.GameId == thisPlayersGame.Id)
                                ?? new GameIndex {GameId = thisPlayersGame.Id};
                            existingGameIndex.Playtime1 = thisPlayersGame.TotalHoursPlayed;
                            existingGameIndex.Playtime2 = matchingOtherPlayersGame.TotalHoursPlayed;

                            OnCalculateGameMetric.OnNext(new GameMetric(playerIndex, gameIndex, games.Count, thisPlayer,
                                thisPlayersGame));
                            gameIndex++;
                        }
                    }
                    playerIndex++;
                }
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
            var friends = XDocument.Parse(result).XPathSelectElements("//friend");
            return friends.Select(node => new Player {Id = long.Parse(node.Element("steamid").Value)});
        }

        public IEnumerable<Player> GetPlayersFromGroup(long groupId)
        {
            var url = string.Format(GroupQuery, groupId);
            var result = XDocument.Parse(Functions.DownloadString(url));

            var xPlayers = result.XPathSelectElements("//steamID64");

            return xPlayers.Select(node => new Player {Id = long.Parse(node.Value)});
        }

        public class GameMetric
        {
            public Game Game;
            public int GameCount;
            public long GameIndex;
            public Player Player;
            public long PlayerIndex;

            public GameMetric(long playerIndex, long gameIndex, int gameCount, Player player, Game game)
            {
                PlayerIndex = playerIndex;
                GameIndex = gameIndex;
                GameCount = gameCount;
                Player = player;
                Game = game;
            }
        }
    }
}