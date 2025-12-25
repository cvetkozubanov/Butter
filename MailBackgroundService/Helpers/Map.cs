namespace MailBackgroundService.Helpers
{
    public class Map
    {
        public Dictionary<string, string> List = new Dictionary<string, string>();
        public static Map Instace { get { return instance; } }

        private static Map instance = new Map();
        private Map() {
            var items = File.ReadAllText("Map.txt").Split("\r\n");
            foreach (var item in items)
            {
                var keyValue = item.Split(" � ");
                List.Add(keyValue[0], keyValue[1]);
            }
        }
    }
}
