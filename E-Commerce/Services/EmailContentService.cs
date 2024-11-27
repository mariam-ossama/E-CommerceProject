
namespace E_Commerce.Services
{
    public class EmailContentService
    {
        public string GetConfirmationEmailContent(string confirmationLink)
        {
            return $@"
        <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #4CAF50;'>Welcome to Our Platform!</h2>
                    <p>Hi there,</p>
                    <p>Thank you for registering with us. To confirm your email, please click the button below:</p>
                    <p style='text-align: center;'>
                        <a href='{confirmationLink}' 
                           style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                           Confirm Email
                        </a>
                    </p>
                    <p>If the button above doesn't work, you can copy and paste the following link into your browser:</p>
                    <p><a href='{confirmationLink}'>{confirmationLink}</a></p>
                    <p>Thank you,<br>The Team</p>
                </div>
            </body>
        </html>";
        }

        public string GetResetEmailContent(string resetPasswordLink)
        {
            return $@"
<html>
    <body style='font-family: Arial, sans-serif; color: #333;'>
        <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
            <h2 style='color: #4CAF50;'>Password Reset Request</h2>
            <p>Hi there,</p>
            <p>We received a request to reset your password. Please click the button below to reset it:</p>
            <p style='text-align: center;'>
                <a href='{resetPasswordLink}' 
                   style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                   Reset Password
                </a>
            </p>
            <p>If the button above doesn't work, you can copy and paste the following link into your browser:</p>
            <p><a href='{resetPasswordLink}'>{resetPasswordLink}</a></p>
            <p>Thank you,<br>The Team</p>
        </div>
    </body>
</html>";
        }
    }
}
