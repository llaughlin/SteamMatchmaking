namespace SteamMatchmaking.Models
{
    public class PlayerRank
    {
        public virtual long Id { get; set; }
        public virtual Player Left { get; set; }
        public virtual Player Right { get; set; }
        public virtual short Value { get; set; }
    }
}