using MailBackgroundService.Models;

namespace MailBackgroundService.Services.Interfaces
{
    public interface IRatesService
    {
        public RatesOutput[] GetRates(RatesInput input, bool firstTime);
    }
}
