using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Soenneker.Attributes.MapTo;
using Soenneker.DataTables.Attributes.Orderable;
using Soenneker.DataTables.Attributes.Searchable;

namespace Soenneker.DataTables.Extensions.ServerSideRequest;

public static class MapCache<T>
{
    /// <summary>
    /// The external to internal.
    /// </summary>
    public static readonly Dictionary<string, string> ExternalToInternal;
    internal static readonly HashSet<string> Searchable;
    internal static readonly HashSet<string> Orderable;

    static MapCache()
    {
        ExternalToInternal = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Searchable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Orderable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < props.Length; i++)
        {
            PropertyInfo p = props[i];

            if (p.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                continue;

            string external = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name;
            string internalPath = p.GetCustomAttribute<MapToAttribute>()?.Path ?? external;

            if (ambiguous.Contains(external))
                continue;

            if (!ExternalToInternal.TryAdd(external, internalPath))
            {
                ExternalToInternal.Remove(external);
                Searchable.Remove(external);
                Orderable.Remove(external);
                ambiguous.Add(external);
                continue;
            }

            if (p.GetCustomAttribute<DataTableSearchableAttribute>() is not null)
                Searchable.Add(external);

            if (p.GetCustomAttribute<DataTableOrderableAttribute>() is not null)
                Orderable.Add(external);
        }
    }
}
