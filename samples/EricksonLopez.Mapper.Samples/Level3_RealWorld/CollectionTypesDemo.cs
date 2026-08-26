// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level3_RealWorld;

// =============================================================================
// Level 3 - Advanced Collections (Array, ImmutableArray, HashSet, IEnumerable)
//
// As TARGET, the generator supports:
//   T[]              - Uses a for-loop with index-based assignment
//   List<T>          - Uses a foreach loop with pre-sizing
//   IEnumerable<T>   - Resolved as List<T>
//   IReadOnlyList<T> - Resolved as List<T>
//   ImmutableArray<T>- Uses ImmutableArray.CreateBuilder with pre-sizing
//   HashSet<T>       - Uses HashSet with pre-sizing if the source has .Count
//   Dictionary<K,V>  - With key and value conversion (demonstrated in CollectionsDemo)
//
// As SOURCE, the generator accepts:
//   T[]              - Uses .Length for pre-sizing the target
//   List<T>          - Uses .Count for pre-sizing the target
//   Any IEnumerable<T> - TryGetNonEnumeratedCount for optimization
//
// Note: In ALL cases the generator emits loops (for/foreach), NEVER LINQ.
//       This guarantees zero closure allocations and AOT compatibility.
// =============================================================================

/// <summary>Represents a tag associated with a content item.</summary>
public class TagEntity
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the display priority of the tag. Lower values appear first.</summary>
    public int Priority { get; set; }
}

/// <summary>Represents a data transfer object for a content tag.</summary>
public class TagDto
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the display priority of the tag.</summary>
    public int Priority { get; set; }
}

// --- Case 1: Array -> Array with complex type mapping ---

/// <summary>Represents a content article with an array of tags and a list of keywords.</summary>
public class ArticleEntity
{
    /// <summary>Gets or sets the unique identifier of the article.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the title of the article.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the tags associated with the article as an array of entities.</summary>
    public TagEntity[] Tags { get; set; } = Array.Empty<TagEntity>();

    /// <summary>Gets or sets the keywords associated with the article.</summary>
    public List<string> Keywords { get; set; } = new();
}

/// <summary>Represents a data transfer object for a content article.</summary>
public class ArticleDto
{
    /// <summary>Gets or sets the unique identifier of the article.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the title of the article.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the mapped tags as an array of DTOs.</summary>
    public TagDto[] Tags { get; set; } = Array.Empty<TagDto>();

    /// <summary>Gets or sets the keywords as a read-only list.</summary>
    public IReadOnlyList<string> Keywords { get; set; } = new List<string>();
}

// --- Case 2: ImmutableArray<T> as target ---

/// <summary>Represents a permission granted within a specific scope.</summary>
public class PermissionEntity
{
    /// <summary>Gets or sets the name of the permission.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the scope within which the permission applies.</summary>
    public string Scope { get; set; } = string.Empty;
}

/// <summary>Represents a data transfer object for a permission.</summary>
public class PermissionDto
{
    /// <summary>Gets or sets the name of the permission.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the scope within which the permission applies.</summary>
    public string Scope { get; set; } = string.Empty;
}

/// <summary>Represents a role entity with an associated list of permissions.</summary>
public class RoleEntity
{
    /// <summary>Gets or sets the name of the role.</summary>
    public string RoleName { get; set; } = string.Empty;
    /// <summary>Gets or sets the permissions granted by this role.</summary>
    public List<PermissionEntity> Permissions { get; set; } = new();
}

/// <summary>Represents a data transfer object for a role with an immutable set of permissions.</summary>
public class RoleDto
{
    /// <summary>Gets or sets the name of the role.</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Gets or sets the permissions granted by this role as an immutable array.</summary>
    public ImmutableArray<PermissionDto> Permissions { get; set; }
}

// --- Case 3: HashSet<T> as target ---

/// <summary>Represents a user with an associated list of tags that may contain duplicates.</summary>
public class UserTagsEntity
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>Gets or sets the list of tags associated with the user, which may contain duplicates.</summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>Represents a data transfer object for user tags with automatic deduplication.</summary>
public class UserTagsDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the deduplicated set of tags associated with the user.</summary>
    public HashSet<string> Tags { get; set; } = new();
}

// --- Case 4: IEnumerable<T> source -> List<T> target ---

/// <summary>Represents a section of a report with a lazily evaluated sequence of text lines.</summary>
public class ReportSectionEntity
{
    /// <summary>Gets or sets the section title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the lines of text in this section as a lazy sequence.</summary>
    public IEnumerable<string> Lines { get; set; } = Array.Empty<string>();
}

/// <summary>Represents a data transfer object for a report section with materialized lines.</summary>
public class ReportSectionDto
{
    /// <summary>Gets or sets the section title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the lines of text in this section as a materialized list.</summary>
    public List<string> Lines { get; set; } = new();
}

/// <summary>
/// Provides compile-time-generated mapping for articles, roles, user tags, and report sections,
/// demonstrating array, <see cref="ImmutableArray{T}"/>, <see cref="HashSet{T}"/>,
/// and <see cref="IEnumerable{T}"/> collection type mappings.
/// </summary>
[Mapper]
public partial class CollectionTypesMapper
{
    /// <summary>
    /// Maps an <see cref="ArticleEntity"/> to an <see cref="ArticleDto"/>,
    /// converting <c>TagEntity[]</c> to <c>TagDto[]</c> and <c>List&lt;string&gt;</c> to <c>IReadOnlyList&lt;string&gt;</c>.
    /// </summary>
    /// <param name="source">The article entity to map from.</param>
    /// <returns>A new <see cref="ArticleDto"/> with all collection properties mapped.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial ArticleDto MapArticle(ArticleEntity source);

    /// <summary>Maps a <see cref="TagEntity"/> to a <see cref="TagDto"/>.</summary>
    /// <param name="source">The tag entity to map from.</param>
    /// <returns>A new <see cref="TagDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial TagDto MapTag(TagEntity source);

    /// <summary>
    /// Maps a <see cref="RoleEntity"/> to a <see cref="RoleDto"/>,
    /// converting the <c>List&lt;PermissionEntity&gt;</c> to an <see cref="ImmutableArray{T}"/> of <see cref="PermissionDto"/>.
    /// </summary>
    /// <param name="source">The role entity to map from.</param>
    /// <returns>A new <see cref="RoleDto"/> with permissions stored in an immutable array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial RoleDto MapRole(RoleEntity source);

    /// <summary>Maps a <see cref="PermissionEntity"/> to a <see cref="PermissionDto"/>.</summary>
    /// <param name="source">The permission entity to map from.</param>
    /// <returns>A new <see cref="PermissionDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial PermissionDto MapPermission(PermissionEntity source);

    /// <summary>
    /// Maps a <see cref="UserTagsEntity"/> to a <see cref="UserTagsDto"/>,
    /// converting the <c>List&lt;string&gt;</c> of tags to a <see cref="HashSet{T}"/> that eliminates duplicates.
    /// </summary>
    /// <param name="source">The user tags entity to map from.</param>
    /// <returns>A new <see cref="UserTagsDto"/> with a deduplicated tag set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial UserTagsDto MapUserTags(UserTagsEntity source);

    /// <summary>
    /// Maps a <see cref="ReportSectionEntity"/> to a <see cref="ReportSectionDto"/>,
    /// materializing the lazy <see cref="IEnumerable{T}"/> source into a <c>List&lt;string&gt;</c>.
    /// </summary>
    /// <param name="source">The report section entity to map from.</param>
    /// <returns>A new <see cref="ReportSectionDto"/> with a materialized list of lines.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial ReportSectionDto MapSection(ReportSectionEntity source);
}

/// <summary>Demonstrates advanced collection type mappings including arrays, immutable arrays, hash sets, and lazy sequences.</summary>
public static class CollectionTypesDemo
{
    /// <summary>Runs the advanced collection types demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 3: Advanced Collection Types ===");
        Console.WriteLine("  Array, IReadOnlyList, ImmutableArray, HashSet - all generated without LINQ.");
        Console.WriteLine();

        var mapper = new CollectionTypesMapper();

        // Case 1: Array -> Array with complex type + IReadOnlyList
        var article = new ArticleEntity
        {
            Id = Guid.NewGuid(),
            Title = "Source Generators in .NET",
            Tags = new[]
            {
                new TagEntity { Name = "dotnet",   Priority = 1 },
                new TagEntity { Name = "roslyn",   Priority = 2 },
                new TagEntity { Name = "aot",      Priority = 3 },
            },
            Keywords = new List<string> { "generator", "compiletime", "mapping" }
        };

        var articleDto = mapper.MapArticle(article);
        Console.WriteLine($"  Article: '{articleDto.Title}'");
        Console.WriteLine($"    Tags (TagDto[]):            {articleDto.Tags.Length} elements - type: {articleDto.Tags.GetType().Name}");
        Console.WriteLine($"    Tags[0].Name:               '{articleDto.Tags[0].Name}'");
        Console.WriteLine($"    Keywords (IReadOnlyList):   {articleDto.Keywords.Count} elements");
        Console.WriteLine();

        // Case 2: ImmutableArray<T>
        var role = new RoleEntity
        {
            RoleName = "SystemAdmin",
            Permissions = new List<PermissionEntity>
            {
                new() { Name = "users.read",  Scope = "global" },
                new() { Name = "users.write", Scope = "global" },
                new() { Name = "audit.read",  Scope = "global" },
            }
        };

        var roleDto = mapper.MapRole(role);
        Console.WriteLine($"  Role: '{roleDto.RoleName}'");
        Console.WriteLine($"    Permissions (ImmutableArray<PermissionDto>): {roleDto.Permissions.Length} elements");
        Console.WriteLine($"    IsDefaultOrEmpty: {roleDto.Permissions.IsDefaultOrEmpty}  (correctly initialized)");
        Console.WriteLine();

        // Case 3: HashSet<T>
        var userTags = new UserTagsEntity
        {
            UserId = "user-001",
            Tags = new List<string> { "dotnet", "csharp", "dotnet", "aot", "csharp" } // contains duplicates
        };

        var userTagsDto = mapper.MapUserTags(userTags);
        Console.WriteLine($"  UserTags: userId='{userTagsDto.UserId}'");
        Console.WriteLine($"    Source Tags (List):    {userTags.Tags.Count} elements (with duplicates)");
        Console.WriteLine($"    Target Tags (HashSet): {userTagsDto.Tags.Count} unique elements");
        Console.WriteLine();

        // Case 4: IEnumerable<T> source
        var section = new ReportSectionEntity
        {
            Title = "Executive Summary",
            Lines = new[] { "Revenue grew 20%", "Costs reduced by 15%", "Net margin improved" }
        };

        var sectionDto = mapper.MapSection(section);
        Console.WriteLine($"  ReportSection: '{sectionDto.Title}'");
        Console.WriteLine($"    Lines (List<string>): {sectionDto.Lines.Count} lines  [source was IEnumerable<string>]");
        Console.WriteLine();
    }
}
