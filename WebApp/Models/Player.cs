using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;

namespace WebApp.Models
{
    public class Player
    {
        public long Id { get; set; }
        public long SteamId { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public virtual ICollection<Player> Friends { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual ICollection<PlayerIndex> PlayerIndices { get; set; }
        public virtual ICollection<GameIndex> GameIndices { get; set; }

        public Player()
        {
            Friends = new List<Player>();
            Games = new List<Game>();
        }
    }
    public class PlayerConfiguration : EntityTypeConfiguration<Player>
    {
        public PlayerConfiguration()
        {
            HasMany(p => p.Friends)
                .WithMany()
                .Map(x => x.MapLeftKey("Player")
                           .MapRightKey("Friend")
                           .ToTable("PlayerFriends"));
        }
    }

}