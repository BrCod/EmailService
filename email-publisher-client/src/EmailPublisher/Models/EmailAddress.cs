using System.Text.RegularExpressions;

namespace EmailPublisher.Models
{
    public readonly record struct EmailAddress(string Value)
    {
        private static readonly Regex Pattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && Pattern.IsMatch(value);
        public static EmailAddress Create(string value)
        {
            if (!IsValid(value)) throw new ArgumentException($"Invalid email: {value}");
            return new EmailAddress(value);
        }
        public override string ToString() => Value;
    }
}
