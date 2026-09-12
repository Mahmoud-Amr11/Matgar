namespace Matgar.Api.Requests.Addresses
{
    public sealed record CreateAddressRequest(
        string FullAddress,
        string City,
        string Governorate,
        string PhoneNumber,
        bool IsDefault);

    public sealed record UpdateAddressRequest(
        string FullAddress,
        string City,
        string Governorate,
        string PhoneNumber);
}