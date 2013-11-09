using System.Xml.Linq;

namespace SteamMatchmaking.Extensions
{
    public static class XDocumentExtensions
    {
        public static XDocument ToXDoc(this string root)
        {
            return XDocument.Parse(root);
        }
        public static string ElementValue(this XElement element, string key)
        {
            var xElement = element.Element(key);
            return xElement != null ? xElement.Value : null;
        }
    }
}
