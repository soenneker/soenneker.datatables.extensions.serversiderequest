using System;
using Soenneker.DataTables.Dtos.ServerSideRequest;
using Soenneker.Dtos.Options.OrderBy;
using Soenneker.Dtos.RequestDataOptions;
using Soenneker.Enums.SortDirections;
using Soenneker.Extensions.String;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Soenneker.DataTables.Extensions.ServerSideRequest;

/// <summary>
/// A collection of helpful DataTableServerSideRequest extension methods
/// </summary>
public static class DataTableServerSideRequestsExtension
{
    /// <summary>
    /// Converts a <see cref="DataTableServerSideRequest"/> object into a <see cref="RequestDataOptions"/> 
    /// structure suitable for server-side querying, including pagination, global search, searchable fields, and sort order.
    /// </summary>
    /// <param name="request">The incoming DataTables request containing pagination, search, and sorting parameters.</param>
    /// <returns>
    /// A populated <see cref="RequestDataOptions"/> object that includes:
    /// <list type="bullet">
    /// <item><description><c>Skip</c>: the starting index of records (from <c>Start</c>).</description></item>
    /// <item><description><c>Take</c>: the number of records to fetch (from <c>Length</c>).</description></item>
    /// <item><description><c>Search</c>: the global search value (if any).</description></item>
    /// <item><description><c>SearchFields</c>: list of searchable field names.</description></item>
    /// <item><description><c>OrderBy</c>: list of sorting instructions mapped from column indexes and directions.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method supports filtering out non-searchable or empty columns when building search and sort fields. 
    /// It also guards against invalid column indexes in sort instructions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = new DataTablesServerSideRequest
    /// {
    ///     Start = 0,
    ///     Length = 25,
    ///     Search = new DataTablesSearchRequest { Value = "smith" },
    ///     Columns = new List&lt;DataTablesColumnRequest&gt;
    ///     {
    ///         new() { Data = "name", Searchable = true },
    ///         new() { Data = "email", Searchable = false }
    ///     },
    ///     Order = new List&lt;DataTablesOrderRequest&gt;
    ///     {
    ///         new() { Column = 0, Dir = "asc" }
    ///     }
    /// };
    ///
    /// var options = request.ToRequestDataOptions();
    /// // options.Skip == 0
    /// // options.Take == 25
    /// // options.Search == "smith"
    /// // options.SearchFields == ["name"]
    /// // options.OrderBy[0].Field == "name"
    /// </code>
    /// </example>
    [Pure]
    public static RequestDataOptions ToRequestDataOptions<T>(this DataTableServerSideRequest request)
    {
        HashSet<string> allowed = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Select(p => p.Name)
                                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Normalise the search term first
        string? search = request.Search?.Value;

        if (search.IsNullOrWhiteSpace())
            search = null;

        var options = new RequestDataOptions
        {
            Skip = Math.Max(request.Start, 0),
            Take = request.Length > 0 ? request.Length : 0,
            Search = search
        };

        if (search is not null && request.Columns is {Count: > 0})
        {
            List<string>? searchFields = null;

            foreach (DataTableColumnRequest col in request.Columns)
            {
                if (col.Searchable && col.Data.HasContent() && allowed.Contains(col.Data))
                {
                    searchFields ??= new List<string>(4);
                    searchFields.Add(col.Data);
                }
            }

            options.SearchFields = searchFields;
        }

        if (request is {Order.Count: > 0, Columns.Count: > 0})
        {
            List<OrderByOption>? orderBy = null;

            foreach (DataTableOrderRequest ord in request.Order)
            {
                if (ord.Column < 0 || ord.Column >= request.Columns.Count)
                    continue;

                DataTableColumnRequest column = request.Columns[ord.Column];

                if (!column.Orderable || !column.Data.HasContent() || !allowed.Contains(column.Data))
                    continue;

                orderBy ??= new List<OrderByOption>(4);
                orderBy.Add(new OrderByOption
                {
                    Field = column.Data,
                    Direction = ParseDirection(ord.Dir)
                });
            }

            if (orderBy?.Count > 0)
                options.OrderBy = orderBy;
        }

        return options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SortDirection ParseDirection(string? dir) =>
        dir is not null && dir.EqualsIgnoreCase("desc") ? SortDirection.Desc : SortDirection.Asc;
}