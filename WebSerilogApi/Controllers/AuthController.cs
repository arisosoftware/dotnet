using Microsoft.AspNetCore.Mvc;
using WebSeriLogApi.Contacts;
using WebSeriLogApi.Models; 

namespace WebSeriLogApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IMaskService _maskService;

        public AuthController(ILogger<AuthController> logger, IMaskService maskService)
        {
            _logger = logger;
            _maskService = maskService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] UserDto model)
        {
             if (!ModelState.IsValid)
            {
                // Log the model state errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError("Model error: {Error}", error.ErrorMessage);
                }

                // Return a detailed error response
                return BadRequest(ModelState);
            }

            // Log user input without sensitive data
            _logger.LogInformation("Received input from user: {@UserInput}", model);

            // Avoid logging sensitive data
            if (model.Password != null)
            {
                _logger.LogWarning("Password provided for user: {UserEmail}", model.Email);
                // Do not log the actual password
            }

            // Mask logging sensitive data
            if (model.Email != null)
            {
                var vemail = _maskService.MaskEmail(model.Email);
                _logger.LogWarning("Mask Email provided for user: {UserEmail}", vemail);
                // Do not log the actual Email
            }

            // Handle login logic here
            return Ok(new { message = "Login successful!" });

        }
    }
}
