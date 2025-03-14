using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ValidationTool
{
    public class PhoneValidator
    {
        // Define constants for better maintainability
        private const string SA_DIALING_CODE = "+27";
        private const string LOCAL_PREFIX = "0";

        // Valid SA mobile prefixes as a set for clearer intent
        private static readonly HashSet<string> ValidSAPrefixes = new HashSet<string> { "06", "07", "08" };

        public Dictionary<string, bool> ValidationResults { get; private set; } = new Dictionary<string, bool>();

        public Task<bool> ValidatePhoneAsync(string phoneNumber)
        {
            ValidationResults.Clear();

            // Format the phone number
            string formattedNumber = FormatPhoneNumber(phoneNumber);

            // Perform validations and store results
            bool isValidFormat = IsValidFormat(formattedNumber);
            ValidationResults.Add("Basic Format", isValidFormat);

            // Early exit if format is invalid
            if (!isValidFormat)
                return Task.FromResult(false);

            bool isValidSANumber = IsValidSouthAfricanNumber(formattedNumber);
            ValidationResults.Add("Valid South African Number", isValidSANumber);

            // Overall validation result
            return Task.FromResult(isValidFormat && isValidSANumber);
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove formatting characters
            string cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)\.]", "");

            // Convert "27" prefix to "+27" if needed
            if (Regex.IsMatch(cleaned, @"^27\d"))
            {
                return "+" + cleaned;
            }

            return cleaned;
        }

        private bool IsValidFormat(string phoneNumber)
        {
            // Check format and length in one step
            return (phoneNumber.StartsWith(LOCAL_PREFIX) && phoneNumber.Length == 10) ||
                   (phoneNumber.StartsWith(SA_DIALING_CODE) && phoneNumber.Length == 12);
        }

        private bool IsValidSouthAfricanNumber(string phoneNumber)
        {
            // Normalize to local format
            string normalizedNumber = NormalizeToLocalFormat(phoneNumber);

            // Get the prefix (first two digits after 0)
            string prefix = normalizedNumber.Substring(0, 2);

            // Check if prefix is in the valid set
            return ValidSAPrefixes.Contains(prefix);
        }

        private string NormalizeToLocalFormat(string phoneNumber)
        {
            // Convert +27 format to 0 format
            if (phoneNumber.StartsWith(SA_DIALING_CODE))
            {
                return LOCAL_PREFIX + phoneNumber.Substring(3);
            }

            return phoneNumber;
        }
    }
}