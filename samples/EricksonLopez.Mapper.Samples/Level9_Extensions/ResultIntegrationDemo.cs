// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

namespace EricksonLopez.Mapper.Sample.Level9_Extensions;

// =============================================================================
// Level 9 - EricksonLopez.Mapper.Result Integration
//
// Package: EricksonLopez.Mapper.Result
//
// Purpose:
//   Provides functional Railway-Oriented Programming mapping extension methods
//   for Result<TSource>, Task<Result<TSource>>, ValueTask<Result<TSource>>, and collections.
//
// Methods (all in ResultMappingExtensions):
//   1. result.Map(mapFunc)                       → Result<TDest>
//   2. resultTask.MapAsync(mapFunc)              → Task<Result<TDest>>
//   3. resultValueTask.MapAsync(mapFunc)         → ValueTask<Result<TDest>>
//   4. result.MapList(mapFunc)                   → Result<IReadOnlyList<TDest>>
// =============================================================================

/// <summary>Source account model.</summary>
public sealed record SourceAccount(int Id, string OwnerName, decimal Balance);

/// <summary>Destination account DTO.</summary>
public sealed record TargetAccountDto(int Id, string DisplayName, string FormattedBalance);

/// <summary>Demonstrates the functional extensions provided by EricksonLopez.Mapper.Result.</summary>
public static class ResultIntegrationDemo
{
    /// <summary>Pure mapping projection function.</summary>
    private static TargetAccountDto ProjectToDto(SourceAccount src)
        => new(src.Id, src.OwnerName.ToUpperInvariant(), $"{src.Balance:C}");

    /// <summary>Runs the Result extension demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 9: EricksonLopez.Mapper.Result Extension ===");
        Console.WriteLine("Demonstrating functional Result<T> mapping (Railway-Oriented Programming).\n");

        // 1. Synchronous Result<T>.Map — success path
        var successResult = Result<SourceAccount>.Success(new SourceAccount(1, "Alice Doe", 1500.50m));
        var mappedSuccess = successResult.Map(ProjectToDto);

        Console.WriteLine($"  1. Result<T>.Map (Success):");
        Console.WriteLine($"     Result.IsSuccess: {mappedSuccess.IsSuccess}");
        Console.WriteLine($"     DTO: Id={mappedSuccess.Value.Id}, DisplayName='{mappedSuccess.Value.DisplayName}', Balance={mappedSuccess.Value.FormattedBalance}");

        // 2. Synchronous Result<T>.Map — error propagation without invoking mapFunc
        var failureResult = Result<SourceAccount>.Failure(Error.Validation("Account.Locked", "Account is locked"));
        var mappedFailure = failureResult.Map(ProjectToDto);

        Console.WriteLine($"\n  2. Result<T>.Map (Failure — error preserved without invoking mapping):");
        Console.WriteLine($"     Result.IsFailure: {mappedFailure.IsFailure}");
        Console.WriteLine($"     Error Code: '{mappedFailure.Error.Code}', Description: '{mappedFailure.Error.Description}'");

        // 3. Asynchronous Task<Result<T>>.MapAsync
        Task<Result<SourceAccount>> asyncTask = Task.FromResult(Result<SourceAccount>.Success(new SourceAccount(2, "Bob Smith", 3200m)));
        var mappedAsync = asyncTask.MapAsync(ProjectToDto).GetAwaiter().GetResult();

        Console.WriteLine($"\n  3. Task<Result<T>>.MapAsync (async overload for Task):");
        Console.WriteLine($"     DTO: Id={mappedAsync.Value.Id}, DisplayName='{mappedAsync.Value.DisplayName}'");

        // 4. Asynchronous ValueTask<Result<T>>.MapAsync
        //    Third overload — ValueTask variant for allocation-free async paths.
        //    Ideal for high-throughput scenarios where Task allocation overhead matters.
        ValueTask<Result<SourceAccount>> valueTask = ValueTask.FromResult(
            Result<SourceAccount>.Success(new SourceAccount(3, "Carol White", 7800.25m)));
        var mappedValueTask = valueTask.MapAsync(ProjectToDto).GetAwaiter().GetResult();

        Console.WriteLine($"\n  4. ValueTask<Result<T>>.MapAsync (allocation-free async overload):");
        Console.WriteLine($"     DTO: Id={mappedValueTask.Value.Id}, DisplayName='{mappedValueTask.Value.DisplayName}', Balance={mappedValueTask.Value.FormattedBalance}");

        // 5. Collection Result<IEnumerable<T>>.MapList
        var accountsResult = Result<IEnumerable<SourceAccount>>.Success(new[]
        {
            new SourceAccount(10, "User 10", 100m),
            new SourceAccount(20, "User 20", 200m)
        });
        var mappedListResult = accountsResult.MapList(ProjectToDto);

        Console.WriteLine($"\n  5. Result<IEnumerable<T>>.MapList:");
        Console.WriteLine($"     Mapped items count: {mappedListResult.Value.Count}");
        foreach (var item in mappedListResult.Value)
        {
            Console.WriteLine($"       - {item.DisplayName} ({item.FormattedBalance})");
        }
        Console.WriteLine();
    }
}
