using System;
using Couchbase.Lite.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Lite
{
    public static class DocumentExtensions
    {      
        public static object ToObject(this Document document, Type fallbackType = null)
        {
            return DictionaryExtensions.ToObject(document, fallbackType);
        }

        public static T ToObject<T>(this Document document)
        {
            return DictionaryExtensions.ToObject<T>(document);
        }
    }
}