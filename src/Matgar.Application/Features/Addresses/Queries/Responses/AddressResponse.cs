namespace Matgar.Application.Features.Addresses.Queries.Responses
{
    public sealed record AddressResponse(
        Guid AddressId,
        string FullAddress,
        string City,
        string Governorate,
        string PhoneNumber,
        bool IsDefault);
}