using System.Net;

namespace WebApp.Controllers
{
    public static class Helpers
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
                {
                    return null;
                }
                throw;
            }
        }
    }
}