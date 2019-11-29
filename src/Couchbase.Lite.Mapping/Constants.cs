using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Couchbase.Lite.Mapping
{
    internal static class Constants
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new ExcludeStreamPropertiesResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            Converters = new JsonConverter[]
            {
                new BlobToBytesJsonConverter(),
                new DateTimeOffsetToDateTimeJsonConverter()
            }
        };
    }
}
