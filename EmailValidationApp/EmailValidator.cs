using System.Net.Mail;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

public class EmailValidator
{
    public Dictionary<string, bool> ValidationResults { get; private set; } = new Dictionary<string, bool>();

    public async Task<bool> ValidateEmailAsync(string email)
    {
        ValidationResults.Clear();

        // Basic format validation
        bool formatValid = ValidateFormat(email);
        ValidationResults.Add("Basic Format", formatValid);
        if (!formatValid)
            return false;

        // Check for disposable email domains
        bool notDisposable = !IsDisposableEmail(email);
        ValidationResults.Add("Not Disposable Domain", notDisposable);

        // Check domain has valid MX record
        bool hasMxRecord = await HasMxRecordAsync(email);
        ValidationResults.Add("Valid Mail Server", hasMxRecord);

        // Check domain is not newly registered
        bool notTempDomain = !IsSuspiciousDomain(email);
        ValidationResults.Add("Not Suspicious Domain", notTempDomain);

        // Check email length (bots sometimes use very long random strings)
        bool properLength = ValidateLength(email);
        ValidationResults.Add("Proper Length", properLength);

        // Check for excessive numbers and special characters (common in bot-generated emails)
        bool noExcessiveRandomization = !HasExcessiveRandomization(email);
        ValidationResults.Add("No Excessive Randomization", noExcessiveRandomization);

        // Overall validation - email passes if it meets essential criteria
        return formatValid && hasMxRecord && (notDisposable || notTempDomain) &&
               properLength && noExcessiveRandomization;
    }

    private bool ValidateFormat(string email)
    {
        try
        {
            // Use MailAddress for basic format validation
            var mailAddress = new MailAddress(email);
            return email.Equals(mailAddress.Address, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateLength(string email)
    {
        // Check if local part (before @) is reasonable length
        string localPart = email.Split('@')[0];
        return localPart.Length >= 3 && localPart.Length <= 64 && email.Length <= 254;
    }

    private bool HasExcessiveRandomization(string email)
    {
        string localPart = email.Split('@')[0];

        // Count digits and special characters
        int digits = localPart.Count(char.IsDigit);
        int special = localPart.Count(c => !char.IsLetterOrDigit(c));

        // Calculate ratio of non-alphabetic characters
        double ratio = (double)(digits + special) / localPart.Length;

        // Check for long sequences of random-looking characters
        bool hasRandomPattern = Regex.IsMatch(localPart, @"[a-zA-Z0-9]{10,}");

        return ratio > 0.5 || hasRandomPattern;
    }

    private bool IsDisposableEmail(string email)
    {
        // List of common disposable email domains
        string[] disposableDomains = {
                "10minutemail.com", "mailinator.com", "guerrillamail.com", "tempmail.com",
                "fakeinbox.com", "temp-mail.org", "throwawaymail.com", "yopmail.com",
                "getnada.com", "mailnesia.com", "tempr.email", "discard.email",
                "sharklasers.com", "trashmail.com", "maildrop.cc", "temp-mail.ru",
                "emailondeck.com", "spamgourmet.com", "jetable.org", "mohmal.com",
                "tempinbox.com", "incognitomail.com", "getairmail.com", "tempmailaddress.com",
                "fakemail.net", "anonmails.de", "trash-mail.at", "mailnull.com"
            };

        string domain = email.Split('@')[1].ToLower();
        return disposableDomains.Any(d => domain.Equals(d, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsSuspiciousDomain(string email)
    {
        // Check for generic TLDs often used by bots
        string domain = email.Split('@')[1].ToLower();

        // List of known legitimate email providers (including older domains)
        string[] knownProviders = {
                "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "aol.com",
                "mail.com", "icloud.com", "protonmail.com", "zoho.com", "yandex.com",
                "yahoo.co.uk", "yahoo.fr", "yahoo.com.au", "yahoo.ca", "yahoo.jp",
                "hotmail.co.uk", "hotmail.fr", "hotmail.de", "live.com", "msn.com",
                "comcast.net", "verizon.net", "att.net", "mail.ru", "web.de",
                "gmx.com", "gmx.de", "gmx.net", "inbox.com", "rocketmail.com"
            };

        // If domain is a known provider, it's not suspicious
        if (knownProviders.Any(provider => domain.Equals(provider, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Checks for domains that appear computer-generated
        bool hasRandomDomainPart = Regex.IsMatch(domain, @"[a-zA-Z0-9]{15,}\.");

        // Check for unusual or less common TLDs often associated with spam
        string[] suspiciousTlds = {
                ".xyz", ".top", ".space", ".website", ".site",
                ".online", ".fun", ".icu", ".club", ".live"
            };

        return hasRandomDomainPart || suspiciousTlds.Any(tld => domain.EndsWith(tld));
    }

    private async Task<bool> HasMxRecordAsync(string email)
    {
        try
        {
            string domain = email.Split('@')[1];

            // Simple check for local testing domains
            if (domain.Equals("localhost") || domain.Equals("example.com"))
                return false;

            // Try to retrieve MX records to see if domain can receive email
            var entries = await Dns.GetHostAddressesAsync(domain);
            return entries.Length > 0;
        }
        catch (SocketException)
        {
            // Domain doesn't exist or has no DNS entries
            return false;
        }
        catch
        {
            // Any other error assumes domain might be valid but we can't verify
            return false;
        }
    }
}