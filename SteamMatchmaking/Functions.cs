using System.Net;
using SteamMatchmaking.Models;

namespace SteamMatchmaking
{
    public static class Functions
    {
        public static string DownloadString(string url)
        {
            try
            {
                return new WebClient().DownloadString(url);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                    return null;
                throw;
            }
        }

        public static string DisplayName(this Player player)
        {
            return $"{player.Name} ({player.RealName})";
        }
    }
}