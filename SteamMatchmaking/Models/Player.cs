using System.Collections.Generic;

namespace SteamMatchmaking.Models
{
    public class Player
    {
        public Player()
        {
            Friends = new List<Player>();
            Games = new List<Game>();
        }

        //public int Id { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }

        //[BsonRef("players")]
        public List<Player> Friends { get; set; }

        //[BsonRef("games")]
        public List<Game> Games { get; set; }

        public List<PlayerIndex> PlayerIndices { get; set; }
        public List<GameIndex> GameIndices { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name} ({RealName})";
        }
    }
}