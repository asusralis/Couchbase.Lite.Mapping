using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Couchbase.Lite.Mapping;
using Newtonsoft.Json;

namespace Couchbase.Lite
{
    public static class ObjectExtensions
    {
        static Dictionary<Type, IEnumerable<PropertyInfo>> _objectProperties = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        public static MutableDocument ToMutableDocument<T>(this T obj,
                                                           IPropertyNameConverter propertyNameConverter = null)
        {
            string id = (string)GetProperties(obj)
                .FirstOrDefault(x => (x.Name == "Id" || x.Name == "id") && (x.PropertyType == typeof(string)))
                ?.GetValue(obj);

            return ToMutableDocument(obj, id, propertyNameConverter);
        }

        public static MutableDocument ToMutableDocument<T>(this T obj, 
                                                           string id, 
                                                           IPropertyNameConverter propertyNameConverter = null)
        {
            MutableDocument document;

            if (string.IsNullOrEmpty(id))
            {
                document = new MutableDocument();
            }
            else
            {
                document = new MutableDocument(id);
            }

            var dictionary = GetDictionary(obj, propertyNameConverter);
            Type objType = obj.GetType();

            if (!IsSimple(objType) && objType.IsClass)
            {
                dictionary.Add("$type", obj.GetType().AssemblyQualifiedName);
            }

            if (dictionary != null)
            {
                document.SetData(dictionary);
            }

            return document;
        }

        static IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type type = obj.GetType();

            if (!_objectProperties.ContainsKey(type))
            {
                _objectProperties.Add(type, type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance).Where(pi => !Attribute.IsDefined(pi, typeof(JsonIgnoreAttribute)))?.ToList());
            }

            return _objectProperties[type];
        }

        static Dictionary<string, object> GetDictionary(object obj, IPropertyNameConverter propertyNameConverter = null)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (PropertyInfo propertyInfo in GetProperties(obj))
            {
                string propertyName;

                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;

                if (propertyType.IsEnum)
                {
                    var attribute = propertyInfo.PropertyType.GetMember(propertyValue.ToString()).FirstOrDefault()?.GetCustomAttribute<EnumMemberAttribute>();
                    if (attribute != null)
                    {
                        propertyValue = attribute.Value;
                    }
                }

                if (propertyValue != null)
                {
                    if (propertyInfo.CustomAttributes?.Count() > 0 && 
                        propertyInfo.GetCustomAttribute(typeof(MappingPropertyName)) is MappingPropertyName mappingProperty)
                    {
                        propertyName = mappingProperty.Name;
                    }
                    else if (propertyInfo.CustomAttributes?.Count() > 0 &&
                        propertyInfo.GetCustomAttribute(typeof(JsonPropertyAttribute)) is JsonPropertyAttribute jsonProperty)
                    {
                        propertyName = jsonProperty.PropertyName;
                    }
                    else if (propertyNameConverter != null)
                    {
                        propertyName = propertyNameConverter.Convert(propertyInfo.Name);
                    }
                    else
                    {
                        propertyName = Settings.PropertyNameConverter.Convert(propertyInfo.Name);
                    }

                    AddDictionaryValue(ref dictionary, propertyName, propertyValue, propertyInfo.PropertyType, propertyNameConverter);
                }
            }

            return dictionary;
        }

        static void AddDictionaryValue(ref Dictionary<string, object> dictionary, 
                                       string propertyName,
                                       object propertyValue, 
                                       Type propertyType,
                                       IPropertyNameConverter propertyNameConverter = null)
        {
            if (propertyType == typeof(byte[]) || propertyType == typeof(Stream))
            {
                dictionary[propertyName] = new Blob(string.Empty, (byte[])propertyValue);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                dictionary[propertyName] = new DateTimeOffset((DateTime)propertyValue);
            }
            else if (!propertyType.IsSimple() && !propertyType.IsEnum && propertyType.IsClass && propertyValue != null)
            {
                if (typeof(IEnumerable).IsAssignableFrom(propertyType))
                {
                    if (propertyType.IsArray && propertyType.GetElementType().IsSimple()
                         || (!propertyType.IsArray && propertyValue is IList 
                             && propertyValue.GetType().GetTypeInfo().GenericTypeArguments[0].IsSimple()))
                    {
                        dictionary[propertyName] = propertyValue;
                    }
                    else
                    {
                        var items = propertyValue as IEnumerable;

                        var dictionaries = new List<Dictionary<string, object>>();

                        foreach (var item in items)
                        {
                            dictionaries.Add(GetDictionary(item, propertyNameConverter));
                        }

                        dictionary[propertyName] = dictionaries.ToArray();
                    }
                }
                else
                {
                    dictionary[propertyName] = GetDictionary(propertyValue, propertyNameConverter);
                }
            }
            else if (propertyType.IsEnum)
            {
                dictionary[propertyName] = propertyValue.ToString();
            }
            else
            {
                dictionary[propertyName] = propertyValue;
            }
        }

        static bool IsSimple(this Type type) => (type.IsValueType || type == typeof(string));
    }
}
