using System;
using System.Collections.Generic;
using Couchbase.Lite.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Lite
{
    public static class ResultSetExtensions
    {     
        /*
        public static T ToObject<T>(this Query.Result result)
        {
            return DictionaryExtensions.ToObject<T>(result);
        }
        */

        public static object ToObject(this Query.Result result, Type fallbackType)
        {
            object obj = default;

            var dbObject = result.GetDictionary("Database");
            Type type = null;

            if (dbObject != null)
            {
                string typeName = dbObject.GetString("$type");

                if (typeName != null)
                {
                    type = Type.GetType(typeName);
                }
            }

            if (type == null && fallbackType != null)
            {
                if (fallbackType != null)
                {
                    type = fallbackType;
                }
                else
                {
                    // todo: 
                    throw new Exception();
                }
            }

            if (result != null)
            {
                var settings = Constants.JsonSettings;

                JObject rootJObj = new JObject();

                foreach (var key in result.Keys)
                {
                    var value = result[key]?.Value;

                    if (value != null)
                    {
                        JObject jObj = null;

                        if (value.GetType() == typeof(DictionaryObject))
                        {
                            var json = JsonConvert.SerializeObject(value, settings);

                            if (!string.IsNullOrEmpty(json))
                            {
                                jObj = JObject.Parse(json);
                            }
                        }
                        else
                        {
                            jObj = new JObject
                            {
                                new JProperty(key, value)
                            };
                        }

                        if (jObj != null)
                        {
                            rootJObj.Merge(jObj, new JsonMergeSettings
                            {
                                // Union array values together to avoid duplicates (e.g. "id")
                                MergeArrayHandling = MergeArrayHandling.Union
                            });
                        }

                        if (rootJObj != null)
                        {
                            obj = rootJObj.ToObject(type);
                        }
                    }
                }
            }

            return obj;
        }

        public static T ToObject<T>(this Query.Result result, Type fallbackType = null)
        {
            return (T)ToObject(result, typeof(T));
        }

        public static IEnumerable<T> ToObjects<T>(this List<Query.Result> results)
        {
            List<T> objects = default;

            if (results?.Count > 0)
            {
                var settings = Constants.JsonSettings;

                objects = new List<T>();

                foreach (var result in results)
                {
                    var obj = ToObject<T>(result, typeof(T));

                    if (obj != default)
                    {
                        objects.Add(obj);
                    }
                }
            }

            return objects;
        }

        /*
        public static IEnumerable<object> ToObjects(this List<Query.Result> results)
        {
            List<object> objects = new List<object>();

            if (results?.Count > 0)
            {
                var settings = Constants.JsonSettings;

                objects = new List<object>();

                foreach (var result in results)
                {
                    object obj = DictionaryExtensions.ToObject(result);

                    if (obj != default)
                    {
                        objects.Add(obj);
                    }
                }
            }

            return objects;
        }
        */

        /*
        public static IEnumerable<T> ToObjects<T>(this List<Query.Result> results)
        {
            List<T> objects = new List<T>();

            if (results?.Count > 0)
            {
                var settings = Constants.JsonSettings;

                foreach (var result in results)
                {
                    T obj = (T)DictionaryExtensions.ToObject(result, typeof(T));

                    if (obj != default)
                    {
                        objects.Add(obj);
                    }
                }
            }

            return objects;
        }
        */
    }
}
