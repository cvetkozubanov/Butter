using System.Text.RegularExpressions;

namespace MailBackgroundService.Models
{
    public class RatesInput
    {
        public RatesInput () { }
        public RatesInput(string mail) {

            try
            {
                var shipperZip = mail.Split("We have an order shipping from ")[1].Split(" to the address listed below:")[0];
                string pattern = @"\d+";
                ShipperZip = Regex.Matches(shipperZip, pattern)[0].Value;

                ConsigneeZip = mail.Split("\r\n")[5].Split(" ")[mail.Split("\r\n")[5].Split(" ").Length - 2];

                pattern = @"\d+[xX]\d+[xX]\d+";
                var dims = Regex.Matches(mail, pattern);
                List<List<int>> dimensions = new List<List<int>>();
                foreach (var dim in dims.ToList())
                {
                    pattern = @"\d+";
                    var d = Regex.Matches(dim.Value, pattern, RegexOptions.IgnoreCase).ToList();
                    dimensions.Add(d.Select(d1 => int.Parse(d1.Value)).ToList());
                };

                pattern = @"\d+\s?(pcs|pieces)";
                var pcs = Regex.Matches(mail, pattern, RegexOptions.IgnoreCase).Select(v => new string(v.Value.Where(char.IsDigit).ToArray())).ToList();

                pattern = @"(\d+)[\s/]*lbs";
                var lbs = Regex.Matches(mail, pattern, RegexOptions.IgnoreCase).Select(v => new string(v.Value.Where(char.IsDigit).ToArray())).ToList();
                int i = 0;
                foreach (var dim in dimensions)
                {
                    if (dim.Count != 3)
                    {
                        continue;
                    }
                    Items.Add(new Item()
                    {
                        Width = dim[0],
                        Height = dim[1],
                        Length = dim[2],
                        Pieces = dimensions.Count == lbs.Count ? int.Parse(pcs[i]) : int.Parse(pcs[0]),
                        Weight = dimensions.Count == lbs.Count ? int.Parse(lbs[i]) : int.Parse(lbs[0]),
                    });
                    i++;
                }
            }
            catch (Exception)
            {
                Valid = false;
            }
        }
        public List<Item> Items { get; set; } = new List<Item>();
        public string ConsigneeZip { get; set; }
        public string ShipmentMode { get; set; } = "LTL";
        public string ShipperZip { get; set; }
        public string Miles { get; set; } = "0";
        public string ShipperCountry { get; set; } = "USA";
        public string ConsigneeCountry { get; set; } = "USA";
        public string EquipmentType { get; set; } = "StraightVan";
        public bool Valid { get; set; } = true;
        public List<string> Accessorials { get; set; } = new List<string>();
    }
}
