namespace SteamMatchmaking.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class PlayerIndex
    {
        public PlayerIndex()
        {
            GameIndexes = new List<GameIndex>();
        }

        public virtual long Id { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }

        public double TotalLikeness
        {
            get
            {
                return Player1.Games.Sum(g => g.TotalHoursPlayed)
                    - Player2.Games.Sum(g => g.TotalHoursPlayed);
            }
        }

        public double AverageGameLikeness { get { return GameIndexes.Average(gi => gi.Likeness); } }
        public List<GameIndex> GameIndexes { get; set; }
    }
}