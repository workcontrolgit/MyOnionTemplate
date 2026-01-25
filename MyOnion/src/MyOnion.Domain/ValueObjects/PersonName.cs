namespace MyOnion.Domain.ValueObjects
{
    public sealed class PersonName
    {
        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }

        private PersonName()
        {
        }

        public PersonName(string firstName, string middleName, string lastName)
        {
            FirstName = Normalize(firstName);
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : Normalize(middleName);
            LastName = Normalize(lastName);

            if (FirstName.Length == 0 || LastName.Length == 0)
            {
                throw new ArgumentException("First and last name are required.");
            }

            if (FirstName.Length > 100 || LastName.Length > 100 || (MiddleName?.Length ?? 0) > 100)
            {
                throw new ArgumentException("Name parts must not exceed 100 characters.");
            }
        }

        public string FullName =>
            string.IsNullOrWhiteSpace(MiddleName)
                ? $"{FirstName} {LastName}"
                : $"{FirstName} {MiddleName} {LastName}";

        public static string Normalize(string value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
