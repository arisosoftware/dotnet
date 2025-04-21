using WebSeriLogApi.Contacts;
using System.Net.Mail;

namespace WebSeriLogApi.Services
{
    public class MaskService : IMaskService
    {
        public string MaskEmail(string email)
        {
            // Validate if the email is in a correct format
            if (IsValidEmail(email))
            {
                var parts = email.Split('@');
                return $"{parts[0][0]}****@{parts[1]}"; // Mask the email
            }
            else
            {
                throw new ArgumentException("Invalid email format");
            }
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true; // Valid email format
            }
            catch (FormatException)
            {
                return false; // Invalid email format
            }
        }
        public string MaskPhoneNumber(string phoneNumber)
        {
            // Validate if the phone number is in a correct format
            if (IsValidPhoneNumber(phoneNumber))
            {
                return $"{phoneNumber.Substring(0, 2)}****{phoneNumber.Substring(phoneNumber.Length - 2)}"; // Mask the phone number
            }
            else
            {
                throw new ArgumentException("Invalid phone number format");
            }
        }

        // Helper method to validate phone number format    
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Example validation: check if the phone number is numeric and has a length of 10
            return phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10;
        }
        public string MaskCreditCard(string creditCardNumber)
        {
            // Validate if the credit card number is in a correct format
            if (IsValidCreditCard(creditCardNumber))
            {
                return $"{creditCardNumber.Substring(0, 4)}****{creditCardNumber.Substring(creditCardNumber.Length - 4)}"; // Mask the credit card number
            }
            else
            {
                throw new ArgumentException("Invalid credit card number format");
            }
        }
        private bool IsValidCreditCard(string creditCardNumber)
        {
            // Example validation: check if the credit card number is numeric and has a length of 16
            return creditCardNumber.All(char.IsDigit) && creditCardNumber.Length == 16;
        }
        public string MaskPassword(string password)
        {
            // Example masking: replace all characters with asterisks
            return new string('*', password.Length);
        }
        public string MaskSSN(string ssn)
        {
            // Validate if the SSN is in a correct format
            if (IsValidSSN(ssn))
            {
                return $"{ssn.Substring(0, 3)}-**-****"; // Mask the SSN
            }
            else
            {
                throw new ArgumentException("Invalid SSN format");
            }
        }
        private bool IsValidSSN(string ssn)
        {
            // Example validation: check if the SSN is numeric and has a length of 9
            return ssn.All(char.IsDigit) && ssn.Length == 9;
        }
        public string MaskAddress(string address)
        {
            // Example masking: replace all characters with asterisks
            return new string('*', address.Length);
        }
    }
}
