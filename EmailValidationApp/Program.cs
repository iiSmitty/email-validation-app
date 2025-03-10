using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailValidationApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Email Validation Tool");
            Console.WriteLine("---------------------");

            while (true)
            {
                Console.Write("\nEnter email address to validate (or 'exit' to quit): ");
                string email = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email) || email.ToLower() == "exit")
                    break;

                var validator = new EmailValidator();
                bool isValid = await validator.ValidateEmailAsync(email);

                if (isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Email appears to be valid.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Email appears to be invalid or from a bot.");
                }

                Console.ResetColor();
                Console.WriteLine("Validation details:");
                foreach (var check in validator.ValidationResults)
                {
                    Console.WriteLine($"- {check.Key}: {(check.Value ? "Passed" : "Failed")}");
                }
            }

            Console.WriteLine("\nThank you for using the Email Validation Tool.");
        }
    }
}