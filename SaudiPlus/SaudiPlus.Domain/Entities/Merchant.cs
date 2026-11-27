namespace SaudiPlus.Domain.Entities;
public sealed class Merchant
{
    private Merchant() { }
    public Merchant(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            throw new Exceptions.DomainValidationException("Merchant name must be between 1 and 200 characters.");
        Id = Guid.NewGuid();
        Name = name;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
}
public sealed class MerchantMembership
{
    private MerchantMembership() { }
    public MerchantMembership(Guid userId, Guid merchantId)
    {
        if (userId == Guid.Empty || merchantId == Guid.Empty)
            throw new Exceptions.DomainValidationException("User and merchant are required.");
        UserId = userId;
        MerchantId = merchantId;
    }
    public Guid UserId { get; private set; }
    public Guid MerchantId { get; private set; }
}
