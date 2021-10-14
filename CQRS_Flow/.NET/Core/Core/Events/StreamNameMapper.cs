using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Core.Events
{
    public class StreamNameMapper
    {
        private static readonly StreamNameMapper Instance = new();

        private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

        public static void AddCustomMap<TStream>(string mappedStreamName) =>
            AddCustomMap(typeof(TStream), mappedStreamName);

        public static void AddCustomMap(Type streamType, string mappedStreamName)
        {
            Instance.typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
        }

        public static string ToStreamPrefix<TStream>() => ToStreamPrefix(typeof(TStream));

        public static string ToStreamPrefix(Type streamType) => Instance.typeNameMap.GetOrAdd(streamType, (_) =>
        {
            var modulePrefix = streamType.Namespace!.Split(".").First();
            return $"{modulePrefix}_{streamType.Name}";
        });

        public static string ToStreamId<TStream>(object aggregateId, object? tenantId = null) =>
            ToStreamId(typeof(TStream), aggregateId);

        // Generates a stream id in the canonical `{category}-{aggregateId}` format
        public static string ToStreamId(Type streamType, object aggregateId, object? tenantId = null)
        {
            var tenantPrefix = tenantId == null ? $"{tenantId}_"  : "";
            var category = ToStreamPrefix(streamType);

            // (Out-of-the box, the category projection treats the category as the left bit, based on the `-` separator)
            // For this reason, we place the "{tenantId}_" bit on the right hand side of the '-' if present
            return $"{category}-{tenantPrefix}{aggregateId}";
        }
    }
}
