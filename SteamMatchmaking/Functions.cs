using System.Data.Entity;
using System.Net;
using LiteDB;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    public static class Functions
    {
        public static LiteDatabase GetDb()
        {
            return new LiteDatabase("steam.db");
        }
        public static string DownloadString(string url)
        {
            try
            {
                return new WebClient().DownloadString(url);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    return null;
                }
                throw;
            }
        }

        public static string DisplayName(this Player player)
        {
            return string.Format("{0} ({1})", player.Name, player.RealName);
        }

    }
}
