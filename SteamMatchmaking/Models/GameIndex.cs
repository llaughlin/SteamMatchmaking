
using SteamMatchmaking.Models;

public class GameIndex
{
    public long Id { get; set; }

    public virtual Player Player1 { get; set; }
    public virtual Player Player2 { get; set; }

    public long GameId { get; set; }
    public double Playtime1 { get; set; }
    public double Playtime2 { get; set; }

    public double Likeness { get; set; }
}