using System.Text.RegularExpressions;

namespace ValidationTool
{
    public class NameValidator
    {
        public Dictionary<string, bool> ValidationResults { get; private set; } = new Dictionary<string, bool>();

        public Task<bool> ValidateNameAsync(string fullName)
        {
            ValidationResults.Clear();

            // Check if name is not empty
            bool isNotEmpty = !string.IsNullOrWhiteSpace(fullName);
            ValidationResults.Add("Not Empty", isNotEmpty);
            if (!isNotEmpty)
                return Task.FromResult(false);

            // Check if name contains at least two parts (first name and surname)
            string[] nameParts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool hasMultipleParts = nameParts.Length >= 2;
            ValidationResults.Add("Has First Name and Surname", hasMultipleParts);

            // Check for digits (not allowed in South African names on ID)
            bool noDigits = !Regex.IsMatch(fullName, @"\d");
            ValidationResults.Add("No Digits", noDigits);

            // Check for special characters (allowing only apostrophes and hyphens for names like O'Connor or Mary-Jane)
            bool validCharacters = Regex.IsMatch(fullName, @"^[a-zA-ZÀ-ÖØ-öø-ÿ\s'\-]+$");
            ValidationResults.Add("Valid Characters", validCharacters);

            // Check minimum/maximum length
            bool validLength = fullName.Trim().Length >= 3 && fullName.Trim().Length <= 100;
            ValidationResults.Add("Valid Length", validLength);

            // Check that each name part has a minimum length
            bool validNameParts = nameParts.All(part => part.Length >= 2);
            ValidationResults.Add("Valid Name Parts", validNameParts);

            // Special check for South African naming conventions (optional)
            // This is a simple check - could be expanded with more specific cultural rules
            bool culturallyValid = ValidateSouthAfricanNameConventions(fullName);
            ValidationResults.Add("Culturally Valid", culturallyValid);

            // Overall validation result
            bool isValid = isNotEmpty && hasMultipleParts && noDigits && validCharacters && validLength && validNameParts;
            return Task.FromResult(isValid);
        }

        private bool ValidateSouthAfricanNameConventions(string fullName)
        {
            // This method could be expanded with specific validation rules for
            // South African names from different cultural backgrounds

            // Check for common prefixes that shouldn't be standalone names
            string[] nameParts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] notStandaloneWords = { "van", "de", "der", "du", "den", "von" };

            // Ensure prefixes aren't used alone
            for (int i = 0; i < nameParts.Length; i++)
            {
                if (i < nameParts.Length - 1)
                {
                    continue; // Skip checking if this isn't the last part
                }

                if (notStandaloneWords.Contains(nameParts[i].ToLower()))
                {
                    return false; // Last part can't be just a prefix
                }
            }

            return true;
        }
    }
}