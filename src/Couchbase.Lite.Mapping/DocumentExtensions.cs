﻿using System;
using Couchbase.Lite.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Lite
{
    public static class DocumentExtensions
    {      
        public static object ToObject(this Document document, Type fallbackType = null)
        {
            object obj = null;

            string typeString = document.GetString("$type");
            Type type = null;

            if(!string.IsNullOrEmpty(typeString))
            {
                type = Type.GetType(typeString);              
            }

            if (type == null)
            {
                if(fallbackType != null)
                {
                    type = fallbackType;
                }
                else
                {
                    if(string.IsNullOrEmpty(typeString))
                    {
                        throw new Exception("Could not find $type");
                    }
                    else if(fallbackType == null)
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
                if (document != null)
                {
                    if (document.ToDictionary()?.Count > 0)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new ExcludeStreamPropertiesResolver()
                        };

                        settings.Converters.Add(new BlobToBytesJsonConverter());
                        settings.Converters.Add(new DateTimeOffsetToDateTimeJsonConverter());

                        var dictionary = document.ToMutable()?.ToDictionary();

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

        public static T ToObject<T>(this Document document)
        {
            return (T)DictionaryExtensions.ToObject(document, typeof(T)); 
        }
    }
}
