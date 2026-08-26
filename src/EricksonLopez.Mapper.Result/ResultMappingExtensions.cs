// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Mapper.Result;

/// <summary>
/// Provides functional mapping extension methods for <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultMappingExtensions
{
    /// <summary>
    /// Projects the success value of a <see cref="Result{T}"/> using the specified mapping delegate.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TDest">The destination value type.</typeparam>
    /// <param name="result">The source result instance</param>
    /// <param name="mapFunc">The delegate used to transform the success value</param>
    /// <returns>A new <see cref="Result{T}"/> containing the projected value if successful; otherwise, the original error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapFunc"/> is <see langword="null"/></exception>
    public static Result<TDest> Map<TSource, TDest>(
        this Result<TSource> result,
        Func<TSource, TDest> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(mapFunc);

        if (result.IsFailure)
        {
            return Result<TDest>.Failure(result.Error);
        }

        return Result<TDest>.Success(mapFunc(result.Value));
    }

    /// <summary>
    /// Projects the success value of an asynchronous <see cref="Result{T}"/> task using the specified mapping delegate.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TDest">The destination value type.</typeparam>
    /// <param name="resultTask">The asynchronous task returning the source result</param>
    /// <param name="mapFunc">The delegate used to transform the success value</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the projected <see cref="Result{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="resultTask"/> or <paramref name="mapFunc"/> is <see langword="null"/></exception>
    public static async Task<Result<TDest>> MapAsync<TSource, TDest>(
        this Task<Result<TSource>> resultTask,
        Func<TSource, TDest> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapFunc);

        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapFunc);
    }

    /// <summary>
    /// Projects the success value of an asynchronous <see cref="Result{T}"/> value task using the specified mapping delegate.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TDest">The destination value type.</typeparam>
    /// <param name="resultTask">The asynchronous value task returning the source result</param>
    /// <param name="mapFunc">The delegate used to transform the success value</param>
    /// <returns>
    /// A value task representing the asynchronous operation.
    /// The task result contains the projected <see cref="Result{T}"/>.
    /// </returns>
    /// <remarks>
    /// Because <see cref="ValueTask{TResult}"/> is a value type (struct), <paramref name="resultTask"/> cannot be
    /// <see langword="null"/> and no null check is performed for it. Only <paramref name="mapFunc"/> is validated.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="mapFunc"/> is <see langword="null"/></exception>
    public static async ValueTask<Result<TDest>> MapAsync<TSource, TDest>(
        this ValueTask<Result<TSource>> resultTask,
        Func<TSource, TDest> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(mapFunc);

        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapFunc);
    }

    /// <summary>
    /// Projects a sequence of items contained in a successful <see cref="Result{T}"/> into a read-only list of <typeparamref name="TDest"/>.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TDest">The destination element type.</typeparam>
    /// <param name="result">The source result containing the collection</param>
    /// <param name="mapFunc">The delegate used to transform each item</param>
    /// <returns>A new <see cref="Result{T}"/> containing the mapped list if successful; otherwise, the original error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapFunc"/> is <see langword="null"/></exception>
    public static Result<IReadOnlyList<TDest>> MapList<TSource, TDest>(
        this Result<IEnumerable<TSource>> result,
        Func<TSource, TDest> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(mapFunc);

        if (result.IsFailure)
        {
            return Result<IReadOnlyList<TDest>>.Failure(result.Error);
        }

        var list = result.Value.Select(mapFunc).ToList();
        return Result<IReadOnlyList<TDest>>.Success(list);
    }
}
