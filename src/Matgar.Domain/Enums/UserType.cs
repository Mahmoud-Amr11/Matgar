namespace Matgar.Application.Common.Enums
{
    public sealed record UserType(string Value)
    {
        public static readonly UserType Admin = new("Admin");
        public static readonly UserType Customer = new("Customer");
        public static readonly UserType Vendor = new("Vendor");
        public static readonly UserType User = new("User");
    }
}
