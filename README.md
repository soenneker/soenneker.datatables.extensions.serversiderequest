[![](https://img.shields.io/nuget/v/soenneker.datatables.extensions.serversiderequest.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.datatables.extensions.serversiderequest/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.datatables.extensions.serversiderequest/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.datatables.extensions.serversiderequest/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.datatables.extensions.serversiderequest.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.datatables.extensions.serversiderequest/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.datatables.extensions.serversiderequest/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.datatables.extensions.serversiderequest/actions/workflows/codeql.yml)

# Soenneker.DataTables.Extensions.ServerSideRequest

A collection of helpful DataTableServerSideRequest extension methods.

## Install

```bash
dotnet add package Soenneker.DataTables.Extensions.ServerSideRequest
```

## Quick start

```csharp
using Soenneker.DataTables.Extensions.ServerSideRequest;

DataTableServerSideRequest request = /* obtain from your application */;
var result = request.ToRequestDataOptions();
```

Converts to request Data Options.

## What you get

- `DataTableServerSideRequestsExtension` — A collection of helpful DataTableServerSideRequest extension methods.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `DataTableServerSideRequestsExtension.ToRequestDataOptions(request)` | Converts to request Data Options. | The resulting request Data Options. |
