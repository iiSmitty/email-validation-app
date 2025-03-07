# Email Validation Tool

A C# console application designed to validate email addresses and detect potentially fake ones often used by bots. This tool was created to help mitigate issues with bot submissions on web forms, particularly for platforms that don't offer built-in "Are you a human?" verification.

## Features

- **Basic Format Validation**: Ensures email follows proper syntax
- **MX Record Verification**: Confirms domain has valid mail exchange servers
- **Disposable Email Detection**: Identifies common temporary email services
- **Suspicious Domain Analysis**: Flags unusual or randomly generated domains
- **Length and Pattern Checks**: Detects unnaturally long or random character sequences
- **Character Ratio Analysis**: Identifies emails with excessive numbers or special characters
- **Trusted Provider Whitelist**: Recognizes legitimate email providers including older domains (Yahoo, Hotmail, etc.)

## Requirements

- .NET 5.0 or higher
- Internet connection (for MX record verification)

## Installation

1. Clone this repository:
   ```
   git clone https://github.com/yourusername/email-validation-tool.git
   ```

2. Navigate to the project directory:
   ```
   cd email-validation-tool
   ```

3. Build the application:
   ```
   dotnet build
   ```

## Usage

1. Run the application:
   ```
   dotnet run
   ```

2. Enter email addresses when prompted:
   ```
   Enter email address to validate (or 'exit' to quit): example@email.com
   ```

3. Review validation results:
   ```
   Email appears to be valid.
   Validation details:
   - Basic Format: Passed
   - Not Disposable Domain: Passed
   - Valid Mail Server: Passed
   - Not Suspicious Domain: Passed
   - Proper Length: Passed
   - No Excessive Randomization: Passed
   ```

4. Type 'exit' to quit the application.

## Customization

### Adding Known Email Providers

To add more trusted email providers to the whitelist, modify the `knownProviders` array in the `IsSuspiciousDomain` method:

```csharp
string[] knownProviders = {
    "gmail.com", "yahoo.com", "hotmail.com",
    // Add your trusted domains here
};
```

### Updating Disposable Email Domains

To expand the list of disposable email domains, modify the `disposableDomains` array in the `IsDisposableEmail` method:

```csharp
string[] disposableDomains = {
    "10minutemail.com", "mailinator.com",
    // Add more disposable domains here
};
```

## Integration Ideas

- **Batch Processing**: Modify to accept a CSV file with multiple email addresses
- **API Integration**: Convert to a web service that can be called from your forms
- **Database Storage**: Add functionality to log validation results for analysis
- **Scoring System**: Implement a weighted scoring system instead of pass/fail
- **Email Verification**: Add SMTP verification by attempting to send a test email

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Acknowledgments

- Created to address the issue of bot submissions on Facebook leads
- Inspired by the need for form validation where native "Are you a human?" checks are unavailable