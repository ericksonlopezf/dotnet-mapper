# Level 04: Domain Primitives & Strongly-Typed IDs Integration

## 1. Domain Primitives & Value Object Mappings

When integrating with `EricksonLopez.DomainPrimitives` and `EricksonLopez.ValueObjects`, mapping between primitive CLR types and encapsulated domain types is automatic:

```csharp
using EricksonLopez.Mapper.DomainPrimitives;

public readonly record struct UserId(Guid Value) : IStrongId<Guid>;
public readonly record struct Money(decimal Amount, string Currency);

public sealed record CreateAccountCommand(
    Guid UserId,
    decimal InitialBalance,
    string Currency);

public sealed record Account(
    UserId Id,
    Money Balance);

[Mapper]
public static partial class DomainAccountMapper
{
    [MapProperty(nameof(Account.Id), "new UserId(command.UserId)")]
    [MapProperty(nameof(Account.Balance), "new Money(command.InitialBalance, command.Currency)")]
    public static partial Account ToDomain(CreateAccountCommand command);
}
```

---

## 2. Zero-Allocation Strongly-Typed ID Unwrapping

To map domain models back to flat DTOs:

```csharp
public sealed record AccountResponse(
    Guid UserId,
    decimal Balance,
    string Currency);

[Mapper]
public static partial class DomainAccountMapper
{
    [MapProperty(nameof(AccountResponse.UserId), "account.Id.Value")]
    [MapProperty(nameof(AccountResponse.Balance), "account.Balance.Amount")]
    [MapProperty(nameof(AccountResponse.Currency), "account.Balance.Currency")]
    public static partial AccountResponse ToResponse(Account account);
}
```

Both mappings compile into direct value transfers without boxing value types.
