[![](https://img.shields.io/nuget/v/soenneker.datatables.extensions.serversiderequest.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.datatables.extensions.serversiderequest/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.datatables.extensions.serversiderequest/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.datatables.extensions.serversiderequest/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.datatables.extensions.serversiderequest.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.datatables.extensions.serversiderequest/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.datatables.extensions.serversiderequest/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.datatables.extensions.serversiderequest/actions/workflows/codeql.yml)

# Soenneker.DataTables.Extensions.ServerSideRequest

Converts a DataTables server-side request into `RequestDataOptions` while mapping client column names to an explicit server-owned search and ordering allow-list.

## Installation

```bash
dotnet add package Soenneker.DataTables.Extensions.ServerSideRequest
```

## Define the queryable model

```csharp
using System.Text.Json.Serialization;
using Soenneker.Attributes.MapTo;
using Soenneker.DataTables.Attributes.Orderable;
using Soenneker.DataTables.Attributes.Searchable;

public sealed class CustomerRow
{
    [JsonPropertyName("name")]
    [MapTo("contact.name")]
    [DataTableSearchable]
    [DataTableOrderable]
    public required string Name { get; init; }

    [JsonPropertyName("createdAt")]
    [MapTo("createdAt")]
    [DataTableOrderable]
    public DateTimeOffset CreatedAt { get; init; }

    public string? InternalNote { get; init; }
}
```

`JsonPropertyName` is the external name sent by DataTables. `MapTo` optionally identifies the field understood by the downstream data API. Without `MapTo`, the external name is used.

Only properties marked with `DataTableSearchable` can become search fields or exact-match column filters. Only properties marked with `DataTableOrderable` can become order clauses. Client flags alone do not grant access.

## Convert the request

```csharp
using Soenneker.DataTables.Extensions.ServerSideRequest;
using Soenneker.Dtos.RequestDataOptions;

RequestDataOptions options = request.ToRequestDataOptions<CustomerRow>();
```

The conversion:

- uses a default page size of 50 when `Length` is zero or negative;
- caps the page size at 250;
- trims empty global and column search values to `null`;
- ignores unknown columns, invalid column indices, and sort directions other than `asc` or `desc`;
- ignores properties marked with `JsonIgnore` and rejects duplicate external names rather than choosing one arbitrarily;
- carries the continuation token through unchanged.

The result is still query input. The downstream repository should validate mapped paths it supports, enforce its own cost limits, and treat the continuation token and search text as untrusted. The DataTables regex flags are not translated into regex queries.
