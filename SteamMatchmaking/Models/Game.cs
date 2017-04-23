using LiteDB;

namespace SteamMatchmaking.Models
{
    public class Game
    {
        //public int Id { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public double RecentHoursPlayed { get; set; }
        public double TotalHoursPlayed { get; set; }
        public string LogoUrl { get; set; }
        public string StoreLink { get; set; }

        [BsonRef("players")]
        public Player Player { get; set; }
    }
}