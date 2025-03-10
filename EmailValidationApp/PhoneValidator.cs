using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ValidationTool
{
    public class PhoneValidator
    {
        public Dictionary<string, bool> ValidationResults { get; private set; } = new Dictionary<string, bool>();

        public Task<bool> ValidatePhoneAsync(string phoneNumber)
        {
            ValidationResults.Clear();

            // Format the phone number (remove spaces and other formatting characters)
            string formattedNumber = FormatPhoneNumber(phoneNumber);

            // Basic format validation
            bool formatValid = ValidateFormat(formattedNumber);
            ValidationResults.Add("Basic Format", formatValid);
            if (!formatValid)
                return Task.FromResult(false);

            // Check if it's a valid South African number
            bool isValidSANumber = ValidateSouthAfricanNumber(formattedNumber);
            ValidationResults.Add("Valid South African Number", isValidSANumber);

            // Check number length
            bool properLength = ValidateLength(formattedNumber);
            ValidationResults.Add("Proper Length", properLength);

            // Overall validation
            return Task.FromResult(formatValid && isValidSANumber && properLength);
        }

        private bool ValidateFormat(string phoneNumber)
        {
            // Check if it's in one of the two valid formats: 0XXXXXXXXX or +27XXXXXXXXX
            return Regex.IsMatch(phoneNumber, @"^(0\d{9}|\+27\d{9})$");
        }

        private bool ValidateSouthAfricanNumber(string phoneNumber)
        {
            // Normalize the number to the 0XX format for validation
            string normalizedNumber = NormalizeNumber(phoneNumber);

            // Check if it starts with valid SA mobile prefixes
            // Major SA mobile prefixes: 06, 07, 08
            return Regex.IsMatch(normalizedNumber, @"^0(6|7|8)\d{8}$");
        }

        private bool ValidateLength(string phoneNumber)
        {
            // South African numbers should be either 10 digits (0XXXXXXXXX) 
            // or 12 characters (+27XXXXXXXXX)
            return (phoneNumber.StartsWith("0") && phoneNumber.Length == 10) ||
                   (phoneNumber.StartsWith("+27") && phoneNumber.Length == 12);
        }

        private string NormalizeNumber(string phoneNumber)
        {
            // Convert +27 format to 0 format
            if (phoneNumber.StartsWith("+27"))
            {
                return "0" + phoneNumber.Substring(3);
            }
            return phoneNumber;
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Clean the input of spaces, dashes, parentheses and other formatting characters
            string cleaned = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[\s\-\(\)\.]", "");

            // Special case: if the user entered the country code without +, fix it
            // First check if it starts with "27" followed by a digit
            if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^27\d"))
            {
                return "+" + cleaned;
            }

            return cleaned;
        }
    }
}