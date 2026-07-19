namespace Core.Helpers
{
    public static class Validator
    {
        // check whether a string is a valid positive integer
        public static bool TryParsePositiveInt(string input, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!int.TryParse(input, out value)) return false;
            return value > 0;
        }

        // check whether a string contains actual text (not null, empty, or whitespace)
        public static bool IsNotEmpty(string input) =>
            !string.IsNullOrWhiteSpace(input);

        // check whether a string is a valid date/ convertable to date
        public static bool TryParseDate(string input, out System.DateTime date) =>
            System.DateTime.TryParse(input, out date);



        // used when registering, so SMTP doesn't get handed garbage
        public static bool IsValidEmail(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(input);
                return addr.Address == input;
            }
            catch
            {
                return false;
            }
        }
        // username must start with a letter, then only letters/digits/underscore
        public static bool IsValidUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!char.IsLetter(input[0])) return false;

            foreach (char c in input)
                if (!char.IsLetterOrDigit(c) && c != '_') return false;

            return true;
        }

        // person's name — must start with a letter, and only contain
        // letters, spaces, hyphens, or apostrophes 
        public static bool IsValidPersonName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var trimmed = input.Trim();
            if (!char.IsLetter(trimmed[0])) return false;

            foreach (char c in trimmed)
                if (!char.IsLetter(c) && c != ' ' && c != '-' && c != '\'') return false;

            return true;
        }

        // strong password check.
        // Requires: at least 8 characters, one uppercase, one lowercase,
        // one digit, one symbol (anything that's not a letter or digit)
        public static bool IsStrongPassword(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 8) return false;

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSymbol = false;

            foreach (char c in input)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSymbol = true;
            }

            return hasUpper && hasLower && hasDigit && hasSymbol;
        }

        // used for fine payments — must be a positive amount, not just any parseable number
        public static bool TryParsePositiveDecimal(string input, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!decimal.TryParse(input, out value)) return false;
            return value > 0;
        }
    }
}