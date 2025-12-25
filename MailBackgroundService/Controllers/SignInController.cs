using Google.Apis.Auth.OAuth2;
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
        public IActionResult Get()
        {
            var asd = _singletonService.setCredentials();

            return Redirect(asd.Build().ToString());
        }
        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleResponse(string code, string state)
        {
            Console.WriteLine("test");

            _singletonService.setToken(code);

            return null;
        }
    }
}
