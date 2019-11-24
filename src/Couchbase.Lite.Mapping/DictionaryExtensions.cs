using System;
using System.Collections.Generic;
using System.Text;
using Couchbase.Lite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Lite.Mapping
{
    public static class DictionaryExtensions
    {
        public static object ToObject(this IDictionaryObject dictionaryObj, Type fallbackType = null)
        {
            object obj = null;

            string typeString = dictionaryObj.GetString("$type");
            Type type = null;

            if (!string.IsNullOrEmpty(typeString))
            {
                type = Type.GetType(typeString);
            }

            if (type == null)
            {
                if (fallbackType != null)
                {
                    type = fallbackType;
                }
                else
                {
                    if (string.IsNullOrEmpty(typeString))
                    {
                        throw new Exception("Could not find $type");
                    }
                    else if (fallbackType == null)
                    {
                        throw new Exception($"Unable to turn {typeString} into a valid Type.");
                    }
                    else
                    {
                        throw new Exception($"Unable to turn {typeString} or fallbackType into a valid Type.");
                    }
                }
            }

            try
            {
                if (dictionaryObj != null)
                {
                    if (dictionaryObj.ToDictionary()?.Count > 0)
                    {
                        var settings = Constants.JsonSettings;

                        var dictionary = dictionaryObj?.ToDictionary();

                        if (dictionary != null)
                        {
                            var json = JsonConvert.SerializeObject(dictionary, settings);

                            if (!string.IsNullOrEmpty(json))
                            {
                                var jObj = JObject.Parse(json);

                                if (jObj != null)
                                {
                                    obj = jObj.ToObject(type);
                                }
                                else
                                {
                                    obj = Activator.CreateInstance(type);
                                }
                            }
                        }
                    }
                    else
                    {
                        obj = Activator.CreateInstance(type);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couchbase.Lite.Mapper - Error: {ex.Message}");
            }

            return obj;
        }

        public static T ToObject<T>(this IDictionaryObject dictionaryObj)
        {
            return (T)ToObject(dictionaryObj, typeof(T));
        }
    }
}
