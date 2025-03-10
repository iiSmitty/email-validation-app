# Validation Tool

A C# console application for validating email addresses and phone numbers to detect potentially fake inputs often used by bots.

## Features

- **Email Validation**: Format checking, MX record verification, disposable email detection, suspicious domain analysis
- **Phone Number Validation**: South African number support with flexible formatting (0723389999, +27723389999, 072 338 9999)
- **User-Friendly Interface**: Simple menu-based console application

## Requirements
- .NET 5.0 or higher
- Internet connection (for email MX record verification)

## Usage

1. Run the application with `dotnet run`
2. Select validation type (email or phone number)
3. Enter input to validate
4. Review detailed validation results

## Customization

- Add trusted email providers to whitelist
- Update list of disposable email domains
- Extend phone validation for additional countries

## License
MIT License
