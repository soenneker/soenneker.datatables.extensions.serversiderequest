using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Soenneker.Attributes.MapTo;
using Soenneker.DataTables.Attributes.Orderable;
using Soenneker.DataTables.Attributes.Searchable;
using Soenneker.DataTables.Dtos.ServerSideRequest;
using Soenneker.Dtos.RequestDataOptions;
using Soenneker.Tests.Unit;

namespace Soenneker.DataTables.Extensions.ServerSideRequest.Tests;

public sealed class DataTablesServerSideRequestsExtensionTests : UnitTest
{
    [Test]
    public void Conversion_uses_server_owned_field_permissions_and_bounds_page_size()
    {
        var request = new DataTableServerSideRequest
        {
            Length = 10_000,
            Search = new DataTableSearchRequest {Value = "ada"},
            Columns = new List<DataTableColumnRequest>
            {
                new() {Data = "name", Searchable = true, Orderable = true, Search = new DataTableSearchRequest {Value = "Ada"}},
                new() {Data = "salary", Searchable = true, Orderable = true},
                new() {Data = "secret", Searchable = true, Orderable = true}
            },
            Order = new List<DataTableOrderRequest>
            {
                new() {Column = 0, Dir = "desc"},
                new() {Column = 1, Dir = "asc"},
                new() {Column = 2, Dir = "sideways"}
            }
        };

        RequestDataOptions options = request.ToRequestDataOptions<Row>();

        if (options.PageSize != 250 ||
            options.SearchFields is not [{ } searchField] || searchField != "contact.name" ||
            options.Filters is not [{Field: "contact.name", Value: "Ada"}] ||
            options.OrderBy is not [{Field: "contact.name"}])
        {
            throw new InvalidOperationException("The converted request did not enforce the expected field and page-size restrictions.");
        }
    }

    private sealed class Row
    {
        [JsonPropertyName("name")]
        [MapTo("contact.name")]
        [DataTableSearchable]
        [DataTableOrderable]
        public string? Name { get; init; }

        [JsonPropertyName("salary")]
        public decimal Salary { get; init; }

        [JsonIgnore]
        [JsonPropertyName("secret")]
        [DataTableSearchable]
        [DataTableOrderable]
        public string? Secret { get; init; }
    }
}
