using MailBackgroundService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MailBackgroundService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignInController : ControllerBase
    {
        private readonly ILogger<SignInController> _logger;
        private readonly IUserCredentialsService _singletonService;

        public SignInController(ILogger<SignInController> logger, IUserCredentialsService singletonService)
        {
            _logger = logger;
            _singletonService = singletonService;
        }

        [HttpGet(Name = "SignIn")]
        public void Get()
        {
            _singletonService.setCredentials();
        }
    }
}
