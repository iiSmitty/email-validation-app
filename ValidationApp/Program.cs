using System;
using System.Threading.Tasks;

namespace ValidationTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Validation Tool");
            Console.WriteLine("---------------");

            while (true)
            {
                Console.WriteLine("\nSelect validation type:");
                Console.WriteLine("1. Email");
                Console.WriteLine("2. Phone Number");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice (1-3): ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)
                {
                    case "1":
                        await ValidateEmailInput();
                        break;
                    case "2":
                        await ValidatePhoneInput();
                        break;
                    case "3":
                        Console.WriteLine("\nThank you for using the Validation Tool.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static async Task ValidateEmailInput()
        {
            Console.Write("\nEnter email address to validate: ");
            string email = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
                return;

            var validator = new EmailValidator();
            bool isValid = await validator.ValidateEmailAsync(email);

            DisplayValidationResults(isValid, validator.ValidationResults);
        }

        private static async Task ValidatePhoneInput()
        {
            Console.Write("\nEnter South African phone number to validate (e.g., 072 338 9999 or +27 72 338 9999): ");
            string phoneNumber = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;

            var validator = new PhoneValidator();
            bool isValid = await validator.ValidatePhoneAsync(phoneNumber);

            DisplayValidationResults(isValid, validator.ValidationResults);
        }

        private static void DisplayValidationResults(bool isValid, System.Collections.Generic.Dictionary<string, bool> validationResults)
        {
            if (isValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Input appears to be valid.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input appears to be invalid.");
            }

            Console.ResetColor();
            Console.WriteLine("Validation details:");
            foreach (var check in validationResults)
            {
                Console.WriteLine($"- {check.Key}: {(check.Value ? "Passed" : "Failed")}");
            }
        }
    }
}