using Soenneker.DataTables.Dtos.ServerSideRequest;
using Soenneker.Dtos.Options.OrderBy;
using Soenneker.Dtos.RequestDataOptions;
using Soenneker.Enums.SortDirections;
using Soenneker.Extensions.String;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Soenneker.DataTables.Extensions.ServerSideRequest;

/// <summary>
/// A collection of helpful DataTableServerSideRequest extension methods
/// </summary>
public static class DataTableServerSideRequestsExtension
{
    [Pure]
    public static RequestDataOptions ToRequestDataOptions<T>(this DataTableServerSideRequest request)
    {
        Dictionary<string, string> map = MapCache<T>.ExternalToInternal;
        var allowed = new HashSet<string>(map.Keys, StringComparer.OrdinalIgnoreCase);

        var options = new RequestDataOptions
        {
            Skip = Math.Max(request.Start, 0),
            Take = request.Length > 0 ? request.Length : 0,
            Search = EmptyToNull(request.Search?.Value)
        };

        // ---------- Searchable fields ----------
        if (options.Search != null && request.Columns is {Count: > 0})
        {
            List<string>? searchFields = null;

            for (var i = 0; i < request.Columns.Count; i++)
            {
                DataTableColumnRequest col = request.Columns[i];

                if (!col.Searchable || string.IsNullOrWhiteSpace(col.Data))
                    continue;

                if (!allowed.Contains(col.Data))
                    continue;

                searchFields ??= new List<string>(4);
                searchFields.Add(map[col.Data]); // translate via MapTo
            }

            options.SearchFields = searchFields;
        }

        // ---------- Sorting ----------
        if (request is {Order: {Count: > 0}, Columns.Count: > 0})
        {
            List<OrderByOption>? orderBy = null;

            for (var i = 0; i < request.Order.Count; i++)
            {
                DataTableOrderRequest ord = request.Order[i];

                if (ord.Column < 0 || ord.Column >= request.Columns.Count)
                    continue;

                DataTableColumnRequest col = request.Columns[ord.Column];

                if (!col.Orderable || col.Data.IsNullOrWhiteSpace())
                    continue;

                if (!allowed.Contains(col.Data))
                    continue;

                orderBy ??= new List<OrderByOption>(4);
                orderBy.Add(new OrderByOption
                {
                    Field = map[col.Data], // translate via MapTo
                    Direction = ParseDirection(ord.Dir)
                });
            }

            options.OrderBy = orderBy;
        }

        return options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? EmptyToNull(string? s) =>
        s.IsNullOrWhiteSpace() ? null : s;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SortDirection ParseDirection(string? dir) =>
        dir is not null && dir.EqualsIgnoreCase("desc") ? SortDirection.Desc : SortDirection.Asc;
}