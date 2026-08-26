// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Mapper.Result.Tests;

public sealed class ResultMappingExtensionsTests
{
    private sealed record CustomerDto(int Id, string FullName);
    private sealed record CustomerEntity(int Id, string FirstName, string LastName);

    private static CustomerDto ToDto(CustomerEntity e) => new(e.Id, $"{e.FirstName} {e.LastName}");

    [Fact]
    public void Map_WhenSuccess_ShouldTransformPayload()
    {
        var entity = new CustomerEntity(1, "Erickson", "Lopez");
        var result = Result<CustomerEntity>.Success(entity);

        var mapped = result.Map(ToDto);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Id.Should().Be(1);
        mapped.Value.FullName.Should().Be("Erickson Lopez");
    }

    [Fact]
    public void Map_WhenFailure_ShouldPropagateErrorAndNotInvokeMapFunc()
    {
        var error = Error.Validation("Customer.Invalid", "Invalid customer ID");
        var result = Result<CustomerEntity>.Failure(error);
        var invoked = false;

        var mapped = result.Map<CustomerEntity, CustomerDto>(e =>
        {
            invoked = true;
            return ToDto(e);
        });

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Map_WhenSuccessAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var result = Result<CustomerEntity>.Success(new CustomerEntity(1, "A", "B"));
        var act = () => result.Map<CustomerEntity, CustomerDto>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public void Map_WhenFailureAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var error = Error.Failure("Code", "Desc");
        var result = Result<CustomerEntity>.Failure(error);
        var act = () => result.Map<CustomerEntity, CustomerDto>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public async Task MapAsync_Task_WhenSuccess_ShouldTransformPayload()
    {
        var entity = new CustomerEntity(2, "Jane", "Doe");
        var resultTask = Task.FromResult(Result<CustomerEntity>.Success(entity));

        var mapped = await resultTask.MapAsync(ToDto);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task MapAsync_Task_WhenFailure_ShouldPropagateErrorAndNotInvokeMapFunc()
    {
        var error = Error.Validation("Customer.Invalid", "Invalid customer ID");
        var resultTask = Task.FromResult(Result<CustomerEntity>.Failure(error));
        var invoked = false;

        var mapped = await resultTask.MapAsync<CustomerEntity, CustomerDto>(e =>
        {
            invoked = true;
            return ToDto(e);
        });

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_Task_WhenResultTaskNull_ShouldThrowArgumentNullException()
    {
        Task<Result<CustomerEntity>> nullTask = null!;
        Func<Task> act = async () => await nullTask.MapAsync(ToDto);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("resultTask");
    }

    [Fact]
    public async Task MapAsync_Task_WhenSuccessAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var resultTask = Task.FromResult(Result<CustomerEntity>.Success(new CustomerEntity(1, "A", "B")));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public async Task MapAsync_Task_WhenFailureAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var error = Error.Failure("Code", "Desc");
        var resultTask = Task.FromResult(Result<CustomerEntity>.Failure(error));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public async Task MapAsync_ValueTask_WhenSuccess_ShouldTransformPayload()
    {
        var entity = new CustomerEntity(3, "John", "Smith");
        var resultTask = ValueTask.FromResult(Result<CustomerEntity>.Success(entity));

        var mapped = await resultTask.MapAsync(ToDto);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.FullName.Should().Be("John Smith");
    }

    [Fact]
    public async Task MapAsync_ValueTask_WhenFailure_ShouldPropagateErrorAndNotInvokeMapFunc()
    {
        var error = Error.Conflict("Customer.Conflict", "Conflict occurred");
        var resultTask = ValueTask.FromResult(Result<CustomerEntity>.Failure(error));
        var invoked = false;

        var mapped = await resultTask.MapAsync<CustomerEntity, CustomerDto>(e =>
        {
            invoked = true;
            return ToDto(e);
        });

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_ValueTask_WhenSuccessAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var resultTask = ValueTask.FromResult(Result<CustomerEntity>.Success(new CustomerEntity(1, "A", "B")));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public async Task MapAsync_ValueTask_WhenFailureAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var error = Error.Failure("Code", "Desc");
        var resultTask = ValueTask.FromResult(Result<CustomerEntity>.Failure(error));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(null!);
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public void MapList_WhenSuccess_ShouldTransformAllItems()
    {
        var entities = new List<CustomerEntity>
        {
            new(1, "A", "B"),
            new(2, "C", "D")
        };
        var result = Result<IEnumerable<CustomerEntity>>.Success(entities);

        var mapped = result.MapList(ToDto);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().HaveCount(2);
        mapped.Value[0].FullName.Should().Be("A B");
        mapped.Value[1].FullName.Should().Be("C D");
    }

    [Fact]
    public void MapList_WhenSuccessAndEmpty_ShouldReturnEmptyReadOnlyList()
    {
        var entities = Enumerable.Empty<CustomerEntity>();
        var result = Result<IEnumerable<CustomerEntity>>.Success(entities);

        var mapped = result.MapList(ToDto);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().BeEmpty();
    }

    [Fact]
    public void MapList_WhenFailure_ShouldPropagateErrorAndNotInvokeMapFunc()
    {
        var error = Error.NotFound("Customer.NotFound", "None found");
        var result = Result<IEnumerable<CustomerEntity>>.Failure(error);
        var invoked = false;

        var mapped = result.MapList<CustomerEntity, CustomerDto>(e =>
        {
            invoked = true;
            return ToDto(e);
        });

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void MapList_WhenSuccessAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var result = Result<IEnumerable<CustomerEntity>>.Success(new List<CustomerEntity>());
        var act = () => result.MapList<CustomerEntity, CustomerDto>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public void MapList_WhenFailureAndMapFuncNull_ShouldThrowArgumentNullException()
    {
        var error = Error.Failure("Code", "Desc");
        var result = Result<IEnumerable<CustomerEntity>>.Failure(error);
        var act = () => result.MapList<CustomerEntity, CustomerDto>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapFunc");
    }

    [Fact]
    public void Map_WhenMapFuncThrows_ShouldPropagateException()
    {
        var entity = new CustomerEntity(1, "A", "B");
        var result = Result<CustomerEntity>.Success(entity);
        var act = () => result.Map<CustomerEntity, CustomerDto>(_ => throw new InvalidOperationException("Map failed"));
        act.Should().Throw<InvalidOperationException>().WithMessage("Map failed");
    }

    [Fact]
    public async Task MapAsync_Task_WhenMapFuncThrows_ShouldPropagateException()
    {
        var entity = new CustomerEntity(1, "A", "B");
        var resultTask = Task.FromResult(Result<CustomerEntity>.Success(entity));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(_ => throw new InvalidOperationException("Async map failed"));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Async map failed");
    }

    [Fact]
    public async Task MapAsync_ValueTask_WhenMapFuncThrows_ShouldPropagateException()
    {
        var entity = new CustomerEntity(1, "A", "B");
        var resultTask = ValueTask.FromResult(Result<CustomerEntity>.Success(entity));
        Func<Task> act = async () => await resultTask.MapAsync<CustomerEntity, CustomerDto>(_ => throw new InvalidOperationException("ValueTask map failed"));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("ValueTask map failed");
    }

    [Fact]
    public void MapList_WhenMapFuncThrows_ShouldPropagateException()
    {
        var entities = new List<CustomerEntity> { new(1, "A", "B") };
        var result = Result<IEnumerable<CustomerEntity>>.Success(entities);
        var act = () => result.MapList<CustomerEntity, CustomerDto>(_ => throw new InvalidOperationException("List map failed"));
        act.Should().Throw<InvalidOperationException>().WithMessage("List map failed");
    }
}
