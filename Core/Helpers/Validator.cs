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
    }
}