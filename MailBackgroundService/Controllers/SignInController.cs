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
        private readonly IGoogleUserCredentialsService _singletonService;

        public SignInController(ILogger<SignInController> logger, IGoogleUserCredentialsService singletonService)
        {
            _logger = logger;
            _singletonService = singletonService;
        }

        [HttpGet(Name = "SignIn")]
        public IActionResult Get()
        {
            
            if (_singletonService.IsKeyPresent())
            {
                _logger.LogInformation("test SignIn already logged in");
                return Content("already logged in");
            }
            var url = _singletonService.SetCredentials();            
            
            _logger.LogInformation("test SignIn ");
            return Redirect(url.Build().ToString());
        }
        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleResponse(string code, string state)
        {
            _logger.LogInformation("test GoogleResponse " + code);

            _singletonService.SetToken(code);

            return Content("All good");
        }
    }
}
