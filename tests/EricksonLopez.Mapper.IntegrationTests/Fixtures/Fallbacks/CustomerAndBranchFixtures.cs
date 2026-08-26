// Copyright © Erickson Lopez. MIT License.
using System.Collections.Generic;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.IntegrationTests.Fixtures;

public class CustomerProfileEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class CustomerProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Mapper]
public partial class CustomerProfileMapper
{
    [MapNullFallback(nameof(CustomerProfileDto.Email), "\"no-reply@domain.com\"")]
    public partial CustomerProfileDto MapProfile(CustomerProfileEntity source);
}

public class BranchDirectoryEntity
{
    public string BranchName { get; set; } = string.Empty;
    public Dictionary<string, AddressEntity>? Locations { get; set; }
}

public class BranchDirectoryDto
{
    public string BranchName { get; set; } = string.Empty;
    public Dictionary<string, AddressDto>? Locations { get; set; }
}

[Mapper]
public partial class BranchDirectoryMapper
{
    public partial BranchDirectoryDto MapBranch(BranchDirectoryEntity source);
    public partial AddressDto MapAddress(AddressEntity source);
}
