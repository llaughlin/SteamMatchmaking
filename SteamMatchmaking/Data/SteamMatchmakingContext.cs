using LiteDB;
using SteamMatchmaking.Models;

namespace SteamMatchmaking.Data
{
    public class SteamMatchmakingContext
    {
        public SteamMatchmakingContext(LiteDatabase db)
        {
            Db = db;
            Players = db.GetCollection<Player>("players");
            Games = db.GetCollection<Game>("games");
            Rankings = db.GetCollection<PlayerIndex>("rankings");
        }

        public LiteCollection<Player> Players { get; set; }
        public LiteCollection<Game> Games { get; set; }
        public LiteCollection<PlayerIndex> Rankings { get; set; }

        public LiteDatabase Db { get; }
    }
}