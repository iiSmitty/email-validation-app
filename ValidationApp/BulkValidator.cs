using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ValidationTool
{
    public class ContactPair
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class ContactValidationResult
    {
        public ContactPair Contact { get; set; } = new ContactPair();
        public bool EmailValid { get; set; }
        public bool PhoneValid { get; set; }
        public Dictionary<string, bool> EmailDetails { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> PhoneDetails { get; set; } = new Dictionary<string, bool>();
    }

    public class BulkValidator
    {
        private readonly EmailValidator _emailValidator;
        private readonly PhoneValidator _phoneValidator;

        public BulkValidator()
        {
            _emailValidator = new EmailValidator();
            _phoneValidator = new PhoneValidator();
        }

        public async Task<List<ContactValidationResult>> ValidateContactsAsync(List<ContactPair> contacts)
        {
            var results = new List<ContactValidationResult>();

            foreach (var contact in contacts)
            {
                bool emailValid = false;
                bool phoneValid = false;
                Dictionary<string, bool> emailDetails = new Dictionary<string, bool>();
                Dictionary<string, bool> phoneDetails = new Dictionary<string, bool>();

                // Validate email if provided
                if (!string.IsNullOrWhiteSpace(contact.Email))
                {
                    emailValid = await _emailValidator.ValidateEmailAsync(contact.Email);
                    emailDetails = new Dictionary<string, bool>(_emailValidator.ValidationResults);
                }

                // Validate phone if provided
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                {
                    phoneValid = await _phoneValidator.ValidatePhoneAsync(contact.Phone);
                    phoneDetails = new Dictionary<string, bool>(_phoneValidator.ValidationResults);
                }

                results.Add(new ContactValidationResult
                {
                    Contact = contact,
                    EmailValid = emailValid,
                    PhoneValid = phoneValid,
                    EmailDetails = emailDetails,
                    PhoneDetails = phoneDetails
                });
            }

            return results;
        }

        public ValidationSummary GetValidationSummary(List<ContactPair> contacts, List<ContactValidationResult> results)
        {
            return new ValidationSummary
            {
                TotalContacts = contacts.Count,
                TotalEmailsProvided = contacts.Count(c => !string.IsNullOrWhiteSpace(c.Email)),
                TotalPhonesProvided = contacts.Count(c => !string.IsNullOrWhiteSpace(c.Phone)),
                ValidEmails = results.Count(r => r.EmailValid),
                ValidPhones = results.Count(r => r.PhoneValid),
                ValidBoth = results.Count(r => r.EmailValid && r.PhoneValid)
            };
        }

        public bool ExportResultsToCsv(List<ContactValidationResult> results, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write header
                    writer.WriteLine("Email,EmailValid,Phone,PhoneValid,BothValid");

                    // Write results
                    foreach (var result in results)
                    {
                        writer.WriteLine($"\"{result.Contact.Email}\",{result.EmailValid},\"{result.Contact.Phone}\",{result.PhoneValid},{result.EmailValid && result.PhoneValid}");
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ContactReader
    {
        public List<ContactPair> ReadContactsFromConsole()
        {
            var contacts = new List<ContactPair>();

            Console.WriteLine("\nEnter contact information as 'email,phone' (one pair per line)");
            Console.WriteLine("Enter a blank line when done:");

            while (true)
            {
                string line = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(line))
                    break;

                contacts.Add(ParseContactPair(line));
            }

            return contacts;
        }

        public List<ContactPair> ReadContactsFromCsv(string filePath, bool hasHeader)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            string[] lines = File.ReadAllLines(filePath);
            int startIndex = hasHeader ? 1 : 0;
            var contacts = new List<ContactPair>();

            for (int i = startIndex; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    contacts.Add(ParseContactPair(lines[i]));
                }
            }

            return contacts;
        }

        public List<ContactPair> ReadContactsFromCsvWithColumns(string filePath, bool hasHeader, int emailColumnIndex, int phoneColumnIndex)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            string[] lines = File.ReadAllLines(filePath);
            int startIndex = hasHeader ? 1 : 0;
            var contacts = new List<ContactPair>();

            for (int i = startIndex; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length > Math.Max(emailColumnIndex, phoneColumnIndex))
                    {
                        string email = emailColumnIndex < parts.Length ? parts[emailColumnIndex].Trim() : string.Empty;
                        string phone = phoneColumnIndex < parts.Length ? parts[phoneColumnIndex].Trim() : string.Empty;

                        contacts.Add(new ContactPair { Email = email, Phone = phone });
                    }
                }
            }

            return contacts;
        }

        private ContactPair ParseContactPair(string line)
        {
            string[] parts = line.Split(',');
            string email = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            string phone = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            return new ContactPair { Email = email, Phone = phone };
        }
    }

    public class ValidationSummary
    {
        public int TotalContacts { get; set; }
        public int TotalEmailsProvided { get; set; }
        public int TotalPhonesProvided { get; set; }
        public int ValidEmails { get; set; }
        public int ValidPhones { get; set; }
        public int ValidBoth { get; set; }
    }

    public static class UIHelper
    {
        public static void DisplayValidationSummary(ValidationSummary summary)
        {
            Console.WriteLine("\nValidation Summary:");
            Console.WriteLine($"Total contacts: {summary.TotalContacts}");
            Console.WriteLine($"Valid emails: {summary.ValidEmails} ({summary.TotalEmailsProvided} provided)");
            Console.WriteLine($"Valid phones: {summary.ValidPhones} ({summary.TotalPhonesProvided} provided)");
            Console.WriteLine($"Valid both: {summary.ValidBoth}");
        }

        public static void DisplayDetailedResults(List<ContactValidationResult> results)
        {
            Console.WriteLine("\nDetailed Results:");
            foreach (var result in results)
            {
                Console.WriteLine("\n--------------------------------------------------");
                Console.WriteLine($"Email: {result.Contact.Email}");
                if (string.IsNullOrWhiteSpace(result.Contact.Email))
                {
                    Console.WriteLine("(No email provided)");
                }
                else
                {
                    if (result.EmailValid)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("VALID");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("INVALID");
                    }
                    Console.ResetColor();

                    foreach (var check in result.EmailDetails)
                    {
                        Console.WriteLine($"- {check.Key}: {(check.Value ? "Passed" : "Failed")}");
                    }
                }

                Console.WriteLine($"\nPhone: {result.Contact.Phone}");
                if (string.IsNullOrWhiteSpace(result.Contact.Phone))
                {
                    Console.WriteLine("(No phone provided)");
                }
                else
                {
                    if (result.PhoneValid)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("VALID");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("INVALID");
                    }
                    Console.ResetColor();

                    foreach (var check in result.PhoneDetails)
                    {
                        Console.WriteLine($"- {check.Key}: {(check.Value ? "Passed" : "Failed")}");
                    }
                }
            }
        }

        public static void DisplaySingleValidationResult(bool isValid, Dictionary<string, bool> validationResults)
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

        public static bool AskYesNo(string question)
        {
            Console.Write(question);
            string response = Console.ReadLine() ?? string.Empty;
            return response.ToLower() == "y" || response.ToLower() == "yes";
        }
    }
}