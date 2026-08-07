namespace LamisaMart.Shared.Domain.ValueObjects;

public record Address
{
    public string RecipientName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string StreetAddress { get; init; } = string.Empty;
    public string ThanaUpazila { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Division { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Bangladesh";

    public Address() { }

    public Address(
        string recipientName,
        string phoneNumber,
        string streetAddress,
        string thanaUpazila,
        string district,
        string division,
        string postalCode = "",
        string country = "Bangladesh")
    {
        RecipientName = recipientName;
        PhoneNumber = phoneNumber;
        StreetAddress = streetAddress;
        ThanaUpazila = thanaUpazila;
        District = district;
        Division = division;
        PostalCode = postalCode;
        Country = country;
    }

    public string FormattedAddress =>
        $"{RecipientName}, {StreetAddress}, {ThanaUpazila}, {District}, {Division} {PostalCode}, {Country}";
}
