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
                var address = mail.Split("to the address listed below:")[1].Split("\r\n")[4];
                ConsigneeZip = address.Split(" ")[address.Split(" ").Length - 2];
                ConsigneeAddress = address;
                ShipperAddress = "TODO fix from map";
                PickupDate = DateTime.Now.AddDays(1).ToString();

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
                    int weight = dimensions.Count == lbs.Count ? int.Parse(lbs[i]) : int.Parse(lbs[0]);
                    int width = dim[0];
                    int height = dim[1];
                    int length = dim[2];
                    float density = weight / ((float)(width * height * length) / 1728);
                    Items.Add(new Item()
                    {
                        Width = width,
                        Height = height,
                        Length = length,
                        Pieces = dimensions.Count == lbs.Count ? int.Parse(pcs[i]) : int.Parse(pcs[0]),
                        Weight = weight,
                        Density = density.ToString(),
                        Class = ClassMapper(density),
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
        public string ConsigneeAddress { get; set; }
        public string ShipperAddress { get; set; }
        public string CustomerId { get; set; }
        public string PickupDate { get; set; }
        private string ClassMapper (float density) {
            if (density < 1)
            {
                return "400";
            }
            else if (density < 2)
            {
                return "300";
            }
            else if (density < 4)
            {
                return "250";
            }
            else if (density < 6)
            {
                return "175";
            }
            else if (density < 8)
            {
                return "125";
            }
            else if (density < 10)
            {
                return "100";
            }
            else if (density < 12)
            {
                return "92.5";
            }
            else if (density < 15)
            {
                return "85";
            }
            else if (density < 22.5)
            {
                return "70";
            }
            else if (density < 30)
            {
                return "65";
            }
            else if (density < 35)
            {
                return "60";
            }
            else if (density < 50)
            {
                return "55";
            }
            else
            {
                return "50";
            }
        }
    }
}
