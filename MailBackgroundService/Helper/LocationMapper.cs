using MailBackgroundService.Models;
using System.Text.RegularExpressions;

namespace MailBackgroundService.Helper
{
    public class LocationMapper
    {
        private static Dictionary<string, string> Map = new Dictionary<string, string>();
        public static string GetAddress(string shipperZip)
        {
            if (Map.Count == 0)
            {
                string mapTxtFile = File.ReadAllText("Helper/Map.txt");
                int index = 0;
                string key = "";
                string value = "";
                foreach (var item in mapTxtFile.Split("\r\n").Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (index % 4 == 0)
                    {
                        if (index != 0)
                        {
                            Map.Add(key, value.Trim("\r\n".ToCharArray()));
                            value = "";
                        }
                        key = Regex.Replace(item, @"\s+", string.Empty);
                    }
                    else
                    {
                        value += item + "\r\n";
                    }
                    index++;
                }
            }
            var keyWithoutSpaces = Regex.Replace(shipperZip, @"\s+", string.Empty);
            return Map.ContainsKey(keyWithoutSpaces) ? Map[keyWithoutSpaces] : "Shipping address not found!";
        }
    }
}
