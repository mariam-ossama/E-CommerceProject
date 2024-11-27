using E_Commerce.Data;
using E_Commerce.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using E_Commerce.DTOs;
using Microsoft.EntityFrameworkCore;
using E_Commerce.Services;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!new EmailAddressAttribute().IsValid(registerDto.Email))
                return BadRequest("Invalid email format.");

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest("Email already exists.");

            var validRoles = new[] { "Admin", "Customer", "Employee" };
            if (!validRoles.Contains(registerDto.Role))
                return BadRequest("Invalid role.");

            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Gender = registerDto.Gender,
                Role = registerDto.Role,
                Password = hashedPassword,
                EmailConfirmed = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate and store token
            var token = _jwtService.GenerateToken(user.UserId, user.Role);
            user.Token = token; // Save token to User table
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7090/api/Auth/confirm-email?token={token}";

            await _emailService.SendConfirmationEmailAsync(user.Email, confirmationLink);

            return Ok(new { user.UserId, token });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token.");

            try
            {
                var userId = _jwtService.ValidateToken(token);
                var user = await _context.Users.SingleOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found.");

                user.EmailConfirmed = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Email confirmed successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error confirming email: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                return Unauthorized("Invalid email or password.");

            if (!user.EmailConfirmed)
                return Unauthorized("Email is not confirmed.");

            // Generate new token and store it
            var token = _jwtService.GenerateToken(user.UserId, user.Role);
            user.Token = token; // Update token in the database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.UserId, token });
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequest([FromQuery] string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
                return BadRequest("Invalid email format.");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");

            // Generate token and store it
            var token = _jwtService.GenerateToken(user.UserId, user.Role);
            user.Token = token; // Update token in the database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var resetPasswordLink = $"https://localhost:7090/api/Auth/reset-password?token={token}";

            await _emailService.SendResetPasswordEmail(user.Email, resetPasswordLink);

            return Ok("Password reset email sent.");
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPasswordPage(string token)
        {
            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Reset Password</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: white;
            border: 1px solid #ddd;
            border-radius: 10px;
        }}

        h2 {{
            color: #4CAF50;
        }}

        .form-group {{
            margin-bottom: 20px;
        }}

        label {{
            font-size: 14px;
            font-weight: bold;
        }}

        input[type=""password""] {{
            width: 100%;
            padding: 10px;
            margin-top: 5px;
            border: 1px solid #ddd;
            border-radius: 5px;
        }}

        .btn {{
            background-color: #4CAF50;
            color: white;
            padding: 10px 20px;
            text-decoration: none;
            border-radius: 5px;
            font-size: 16px;
            display: block;
            width: 100%;
            text-align: center;
        }}

        .error {{
            color: red;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h2>Password Reset</h2>
        <form id=""resetPasswordForm"">
            <input type=""hidden"" id=""token"" name=""token"" value=""{token}"" />
            <div class=""form-group"">
                <label for=""newPassword"">New Password</label>
                <input type=""password"" id=""newPassword"" name=""newPassword"" required />
                <div id=""newPasswordError"" class=""error""></div>
            </div>
            <div class=""form-group"">
                <label for=""confirmPassword"">Confirm New Password</label>
                <input type=""password"" id=""confirmPassword"" name=""confirmPassword"" required />
                <div id=""confirmPasswordError"" class=""error""></div>
            </div>
            <div class=""form-group"">
                <label for=""showPassword"">Show Password</label>
                <input type=""checkbox"" id=""showPassword"" onclick=""togglePasswordVisibility()"" />
            </div>
            <button type=""submit"" class=""btn"">Reset Password</button>
        </form>
    </div>
</body>

    <script>
    function togglePasswordVisibility() {{
        var passwordField = document.getElementById('newPassword');
        var confirmPasswordField = document.getElementById('confirmPassword');
        if (passwordField.type === 'password') {{
            passwordField.type = 'text';
            confirmPasswordField.type = 'text';
        }} else {{
            passwordField.type = 'password';
            confirmPasswordField.type = 'password';
        }}
    }}

    document.getElementById('resetPasswordForm').addEventListener('submit', function(event) {{
        event.preventDefault();

        var token = document.getElementById('token').value;
        var newPassword = document.getElementById('newPassword').value;
        var confirmPassword = document.getElementById('confirmPassword').value;

        // Check if passwords match
        if (newPassword !== confirmPassword) {{
            document.getElementById('confirmPasswordError').textContent = 'Passwords do not match.';
            return;
        }}

        // Prepare the data for submission
        var data = {{
            token: token,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        }};

        // Make the API call using fetch (AJAX)
        fetch(""https://localhost:7090/api/Auth/reset-password"", {{
    method: ""POST"",
    headers: {{
        ""Content-Type"": ""application/json"",
    }},
    body: JSON.stringify(data),
}})
    .then((response) => {{
        if (response.ok) {{
            return response.json().then((data) => {{
                alert(data.message || ""Password reset successfully."");
            }});
        }}
        return response.json().then((err) => Promise.reject(err));
    }})
    .catch((error) => {{
        alert(""Error resetting password: "" + (error.message || ""An unexpected error occurred.""));
    }});
    }});
</script>
</html>";
            return Content(htmlContent, "text/html");
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            try
            {
                var userId = _jwtService.ValidateToken(resetPasswordDto.Token);
                var user = await _context.Users.SingleOrDefaultAsync(u => u.UserId == userId);

                if (user == null || user.Token != resetPasswordDto.Token) // Validate token
                    return NotFound("Invalid token or user not found.");

                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                user.Token = null; // Invalidate the token after password reset
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Password reset successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error resetting password: {ex.Message}");
            }
        }
    }

}
