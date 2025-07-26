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
/// A collection of helpful DataTablesServerSideRequest extension methods
/// </summary>
public static class DataTablesServerSideRequestsExtension
{
    [Pure]
    public static RequestDataOptions ToRequestDataOptions(this DataTablesServerSideRequest request)
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
                var col = request.Columns[i];
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
                var order = request.Order[i];
                if (order.Column >= 0 && order.Column < request.Columns.Count)
                {
                    var column = request.Columns[order.Column];
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
            }

            options.OrderBy = orderBy;
        }

        return options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SortDirection ParseDirection(string? dir)
    {
        return dir is not null && dir.Equals("desc", StringComparison.OrdinalIgnoreCase) ? SortDirection.Desc : SortDirection.Asc;
    }
}