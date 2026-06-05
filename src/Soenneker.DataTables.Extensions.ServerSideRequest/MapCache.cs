using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Soenneker.Attributes.MapTo;

namespace Soenneker.DataTables.Extensions.ServerSideRequest;

/// <summary>
/// Represents the map cache.
/// </summary>
/// <typeparam name="T">The T type.</typeparam>
public static class MapCache<T>
{
    /// <summary>
    /// The external to internal.
    /// </summary>
    public static readonly Dictionary<string, string> ExternalToInternal;

    static MapCache()
    {
        ExternalToInternal = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < props.Length; i++)
        {
            PropertyInfo p = props[i];

            string external = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name;
            string internalPath = p.GetCustomAttribute<MapToAttribute>()?.Path ?? external;

            // If duplicates exist the last one wins
            ExternalToInternal[external] = internalPath;
        }
    }
}