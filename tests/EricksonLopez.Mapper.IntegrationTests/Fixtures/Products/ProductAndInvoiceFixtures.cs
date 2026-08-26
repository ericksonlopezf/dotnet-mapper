// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.IntegrationTests.Fixtures;

public class ProductEntity
{
    public string Sku { get; }
    public decimal Price { get; }

    private ProductEntity(string sku, decimal price)
    {
        Sku = sku;
        Price = price;
    }

    public static ProductEntity Create(string sku, decimal price) => new ProductEntity(sku, price);
}

public class ProductDto
{
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class InvoiceEntity
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
}

public class CurrencyConverter : IConverter<InvoiceEntity, InvoiceDto>
{
    public InvoiceDto Convert(InvoiceEntity source) => new InvoiceDto
    {
        Id = source.Id,
        FormattedAmount = $"${source.Amount:F2}"
    };
}

[Mapper]
public partial class ProductMapper
{
    [MapFactory(nameof(ProductEntity.Create))]
    public partial ProductEntity MapToEntity(ProductDto source);
}

[Mapper]
public partial class InvoiceMapper
{
    [UseConverter(typeof(CurrencyConverter))]
    public partial InvoiceDto MapInvoice(InvoiceEntity source);
}
