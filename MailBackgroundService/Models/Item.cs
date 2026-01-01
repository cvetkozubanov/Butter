namespace MailBackgroundService.Models
{
    public class Item
    {
        public string Class { get; set; } = "70";
        public bool IsHazardous { get; set; }
        public int Pieces { get; set; }
        public int Weight { get; set; }
        public string Packaging { get; set; } = "Pieces";
        public int nmfc { get; set; } = 79300;
        public string ProductDescription { get; set; }
        public string Density { get; set; } = "0";
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Billed { get; set; } = 0;
        public int Cost { get; set; } = 0;
        public string? UnitsWeight { get; set; }
        public string? UnitsDensity { get; set; }
        public string? UnitsDimension { get; set; }
    }
}
