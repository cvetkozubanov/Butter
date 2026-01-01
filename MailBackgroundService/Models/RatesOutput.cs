namespace MailBackgroundService.Models
{
    public class RatesOutput
    {
        public string Name { get; set; }
        public string SCAC { get; set; }
        public float Billed { get; set; }
        public string TransitTime { get; set; }
        public string? BillToCode { get; set; }
        public string ServiceType { get; set; }
        public string ServiceDescription { get; set; }
    }
}
