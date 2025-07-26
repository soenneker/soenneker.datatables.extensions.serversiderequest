using Soenneker.DataTables.Dtos.ServerSideRequest;
using Soenneker.Dtos.Options.OrderBy;
using Soenneker.Dtos.RequestDataOptions;
using Soenneker.Enums.SortDirections;
using Soenneker.Extensions.String;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    public static RequestDataOptions ToRequestDataOptions(this DataTableServerSideRequest request)
    {
        var options = new RequestDataOptions
        {
            Skip = request.Start,
            Take = request.Length,
            Search = request.Search?.Value
        };

        // Populate SearchFields
        if (request.Columns is {Count: > 0})
        {
            List<string>? searchFields = null;

            for (var i = 0; i < request.Columns.Count; i++)
            {
                DataTableColumnRequest col = request.Columns[i];

                if (col.Searchable && col.Data.HasContent())
                {
                    searchFields ??= new List<string>(capacity: request.Columns.Count);
                    searchFields.Add(col.Data);
                }
            }

            options.SearchFields = searchFields;
        }

        // Populate OrderBy
        if (request is {Order.Count: > 0, Columns.Count: > 0})
        {
            List<OrderByOption>? orderBy = null;

            for (var i = 0; i < request.Order.Count; i++)
            {
                DataTableOrderRequest order = request.Order[i];

                if (order.Column < 0 || order.Column >= request.Columns.Count) 
                    continue;

                DataTableColumnRequest column = request.Columns[order.Column];

                if (column.Data.HasContent())
                {
                    orderBy ??= new List<OrderByOption>(capacity: request.Order.Count);
                    orderBy.Add(new OrderByOption
                    {
                        Field = column.Data,
                        Direction = ParseDirection(order.Dir)
                    });
                }
            }

            options.OrderBy = orderBy;
        }

        return options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SortDirection ParseDirection(string? dir)
    {
        return dir is not null && dir.EqualsIgnoreCase("desc") ? SortDirection.Desc : SortDirection.Asc;
    }
}