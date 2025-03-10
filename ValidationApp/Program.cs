using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                Console.WriteLine("1. Email (Single)");
                Console.WriteLine("2. Phone Number (Single)");
                Console.WriteLine("3. Bulk Validation (Email & Phone)");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice (1-4): ");
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
                        await BulkValidateContacts();
                        break;
                    case "4":
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
            UIHelper.DisplaySingleValidationResult(isValid, validator.ValidationResults);
        }

        private static async Task ValidatePhoneInput()
        {
            Console.Write("\nEnter South African phone number to validate (e.g., 072 338 9999 or +27 72 338 9999): ");
            string phoneNumber = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;
            var validator = new PhoneValidator();
            bool isValid = await validator.ValidatePhoneAsync(phoneNumber);
            UIHelper.DisplaySingleValidationResult(isValid, validator.ValidationResults);
        }

        private static async Task BulkValidateContacts()
        {
            Console.WriteLine("\nBulk Contact Validation");
            Console.WriteLine("----------------------");
            Console.WriteLine("1. Enter contact pairs (email,phone) directly (one pair per line)");
            Console.WriteLine("2. Read from CSV file (format: email,phone)");
            Console.WriteLine("3. Read from separate columns in CSV file");
            Console.Write("Enter your choice (1-3): ");

            string choice = Console.ReadLine() ?? string.Empty;
            List<ContactPair> contacts = new List<ContactPair>();
            ContactReader reader = new ContactReader();

            try
            {
                if (choice == "1")
                {
                    contacts = reader.ReadContactsFromConsole();
                }
                else if (choice == "2")
                {
                    Console.Write("\nEnter CSV file path: ");
                    string filePath = Console.ReadLine() ?? string.Empty;

                    bool hasHeader = UIHelper.AskYesNo("Does the file have a header row? (y/n): ");
                    contacts = reader.ReadContactsFromCsv(filePath, hasHeader);
                }
                else if (choice == "3")
                {
                    Console.Write("\nEnter CSV file path: ");
                    string filePath = Console.ReadLine() ?? string.Empty;

                    bool hasHeader = UIHelper.AskYesNo("Does the file have a header row? (y/n): ");

                    // Get column indices
                    int emailColumnIndex, phoneColumnIndex;

                    if (hasHeader)
                    {
                        // Display headers
                        string[] headers = File.ReadAllLines(filePath)[0].Split(',');
                        Console.WriteLine("\nAvailable columns:");
                        for (int i = 0; i < headers.Length; i++)
                        {
                            Console.WriteLine($"{i + 1}. {headers[i]}");
                        }

                        Console.Write("Enter email column number: ");
                        emailColumnIndex = int.Parse(Console.ReadLine() ?? "1") - 1;

                        Console.Write("Enter phone column number: ");
                        phoneColumnIndex = int.Parse(Console.ReadLine() ?? "2") - 1;
                    }
                    else
                    {
                        Console.Write("Enter email column index (0-based): ");
                        emailColumnIndex = int.Parse(Console.ReadLine() ?? "0");

                        Console.Write("Enter phone column index (0-based): ");
                        phoneColumnIndex = int.Parse(Console.ReadLine() ?? "1");
                    }

                    contacts = reader.ReadContactsFromCsvWithColumns(filePath, hasHeader, emailColumnIndex, phoneColumnIndex);
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return;
            }

            if (contacts.Count == 0)
            {
                Console.WriteLine("No contacts to validate.");
                return;
            }

            await ProcessBulkValidation(contacts);
        }

        private static async Task ProcessBulkValidation(List<ContactPair> contacts)
        {
            Console.WriteLine($"\nValidating {contacts.Count} contact(s)...");

            var bulkValidator = new BulkValidator();
            var results = await bulkValidator.ValidateContactsAsync(contacts);
            var summary = bulkValidator.GetValidationSummary(contacts, results);

            // Display summary
            UIHelper.DisplayValidationSummary(summary);

            // Ask if user wants detailed results
            if (UIHelper.AskYesNo("\nShow detailed results? (y/n): "))
            {
                UIHelper.DisplayDetailedResults(results);
            }

            // Ask if user wants to export results
            if (UIHelper.AskYesNo("\nExport results to file? (y/n): "))
            {
                Console.Write("Enter output file path: ");
                string filePath = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                bool success = false;
                string errorMessage = "";


                // Try to export the results
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        // Write simplified header with fewer columns
                        writer.WriteLine("Email,Email Status,Phone,Phone Status,Overall Status");

                        // Write results with fewer columns
                        foreach (var result in results)
                        {
                            // Get status values
                            string emailStatus = result.EmailValid ? "Valid" : "Invalid";
                            string phoneStatus = result.PhoneValid ? "Valid" : "Invalid";
                            string overallStatus = (result.EmailValid && result.PhoneValid) ? "Valid" : "Invalid";

                            // Quote email and phone fields to handle special characters
                            string safeEmail = $"\"{result.Contact.Email.Replace("\"", "\"\"")}\"";
                            string safePhone = $"\"{result.Contact.Phone.Replace("\"", "\"\"")}\"";

                            // Write the simplified line
                            writer.WriteLine($"{safeEmail},{emailStatus},{safePhone},{phoneStatus},{overallStatus}");
                        }
                    }
                    success = true;
                }
                catch (UnauthorizedAccessException)
                {
                    errorMessage = "Access denied. Try running as administrator or using a different location.";
                }
                catch (DirectoryNotFoundException)
                {
                    errorMessage = "Directory not found. Please specify a valid directory path.";
                }
                catch (IOException ex)
                {
                    errorMessage = $"IO Error: {ex.Message}. The file might be in use by another process.";
                }
                catch (Exception ex)
                {
                    errorMessage = $"Unexpected error: {ex.Message}";
                }

                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Results exported to {filePath}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error exporting results: {errorMessage}");
                    Console.WriteLine("\nTry one of these alternatives:");
                    Console.WriteLine("1. Use a different location (e.g., your Documents folder)");
                    Console.WriteLine("2. Make sure the path exists and is writable");
                    Console.WriteLine("3. Use a simple path like 'results.csv' to save in the application directory");
                    Console.ResetColor();
                }
            }
        }

        // Helper method to properly escape CSV fields
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            // If the field contains quotes, commas, or newlines, it needs to be quoted
            bool needsQuotes = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");

            if (needsQuotes)
            {
                // Double up any quotes in the field
                string escapedField = field.Replace("\"", "\"\"");
                return $"\"{escapedField}\"";
            }

            return $"\"{field}\"";
        }
    }
}