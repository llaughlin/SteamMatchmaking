using MongoDB.Driver;

namespace WebApp.Infrastructure
{
    public class MongoSteamMatchmaking
    {
        public static string ConnectionString = "mongodb://localhost";

        private static MongoServer Server
        {
            get { return MongoServer.Create(ConnectionString); }
        }

        public static MongoDatabase Database
        {
            get { return Server.GetDatabase("SteamMatchmaking"); }
        }

        public MongoCollection Players
        {
            get { return Database.GetCollection("Players"); }
        }

        public MongoCollection Games
        {
            get { return Database.GetCollection("Games"); }
        }


    }
}