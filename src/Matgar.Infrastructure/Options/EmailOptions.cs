namespace Matgar.Infrastructure.Otions
{
    internal class EmailOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Password { get; set; } = default!;

    }
}
