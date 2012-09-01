using System.Data.Entity;

namespace SteamMatchmaking.Infrastructure
{
    public class SteamMatchmakingContextInitializer : DropCreateDatabaseAlways<SteamMatchmakingContext>
    {
       
    }
}