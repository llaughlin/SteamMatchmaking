namespace SteamMatchmaking.Models
{
    public class Game
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public string Name { get; set; }
        public double RecentHoursPlayed { get; set; }
        public double TotalHoursPlayed { get; set; }
        public string LogoUrl { get; set; }
        public string StoreLink { get; set; }
        public virtual Player Player { get; set; }

        public Game()
        {
        }
    }
}