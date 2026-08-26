// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Mapper;

[assembly: GenerateMapperRegistration]

namespace EricksonLopez.Mapper.IntegrationTests.Fixtures;

public enum SourceRole { User = 1, Admin = 2, Manager = 3 }
public enum TargetRole { User = 100, Admin = 200, Manager = 300 }

public readonly record struct CustomerId(Guid Value);

public class AddressEntity
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class OrderEntity
{
    public Guid Id { get; set; }
    public SourceRole Role { get; set; }
    public AddressEntity? ShippingAddress { get; set; }
    public List<int> ItemIds { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class OrderDto
{
    public CustomerId Id { get; set; }
    public TargetRole Role { get; set; }
    public AddressDto? ShippingAddress { get; set; }
    public List<int> ItemIds { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(OrderEntity source);
    public partial AddressDto MapAddress(AddressEntity source);
}
