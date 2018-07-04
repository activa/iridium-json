#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2017 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iridium.Reflection;

namespace Iridium.Json
{
    public class JsonObject : IEnumerable<JsonObject>
    {
        private object _value;
        private JsonObjectType _type;

        private JsonObject(object value)
        {
            _value = value;
            _type = JsonObjectType.Value;
        }

        private JsonObject(IEnumerable<JsonObject> array)
        {
            _value = array.ToArray();
            _type = JsonObjectType.Array;
        }

        private JsonObject(Dictionary<string, JsonObject> obj)
        {
            _value = obj;
            _type = JsonObjectType.Object;
        }

        private JsonObject(JsonObjectType type)
        {
            _type = type;

            switch (type)
            {
                case JsonObjectType.Array:
                    _value = new JsonObject[0];
                    break;
                case JsonObjectType.Object:
                    _value = new Dictionary<string,JsonObject>();
                    break;
            }
        }

        public JsonObject()
        {
            _type = JsonObjectType.Undefined;
        }

        [Obsolete("IsEmpty has been renamed to IsUndefined")]
        public bool IsEmpty => IsUndefined;

        public bool IsObject => _type == JsonObjectType.Object;
        public bool IsArray => _type == JsonObjectType.Array;
        public bool IsValue => _type == JsonObjectType.Value;
        public bool IsUndefined => _type == JsonObjectType.Undefined;
        public bool IsNull => _value == null && _type != JsonObjectType.Undefined;
        [Obsolete("IsNullOrEmpty has been renamed to IsNullOrUndefined")]
        public bool IsNullOrEmpty => _value == null;
        public bool IsNullOrUndefined => _value == null;
        public object Value => IsValue ? _value : null;

        public static JsonObject Undefined() => new JsonObject();
        public static JsonObject EmptyObject() => new JsonObject(JsonObjectType.Object);
        public static JsonObject EmptyArray() => new JsonObject(JsonObjectType.Array);
        
        internal static JsonObject FromValue(object value) => new JsonObject(value);
        internal static JsonObject FromArray(IEnumerable<JsonObject> array) => new JsonObject(array);
        internal static JsonObject FromObject(Dictionary<string,JsonObject> obj) => new JsonObject(obj);

        private static readonly HashSet<Type> _simpleTypes = new HashSet<Type>(new[] { typeof(string),typeof(int),typeof(int?),typeof(uint),typeof(uint?),typeof(char),typeof(char?),typeof(long),typeof(long?),typeof(ulong),typeof(ulong?),typeof(decimal),typeof(decimal?),typeof(double),typeof(double?),typeof(float),typeof(float?),typeof(bool),typeof(bool?) });

        public object As(Type type)
        {
            if (_simpleTypes.Contains(type))
                return _value.Convert(type);

            if (type.IsArray)
                return AsArray(type.GetElementType());

            var obj = Activator.CreateInstance(type);

            FillObject(obj);

            return obj;
        }

        public T As<T>()
        {
            if (_value is T typed)
                return typed;

            if (_simpleTypes.Contains(typeof(T)))
                return _value.Convert<T>();

            return (T)(As(typeof(T)) ?? default(T));
        }

        public static implicit operator string(JsonObject jsonObject)
        {
            return jsonObject.As<string>();
        }

        public static implicit operator int(JsonObject jsonObject)
        {
            return jsonObject.As<int>();
        }

        public static implicit operator int?(JsonObject jsonObject)
        {
            return jsonObject.As<int?>();
        }

        public static implicit operator long(JsonObject jsonObject)
        {
            return jsonObject.As<long>();
        }

        public static implicit operator long?(JsonObject jsonObject)
        {
            return jsonObject.As<long?>();
        }

        public static implicit operator double(JsonObject jsonObject)
        {
            return jsonObject.As<double>();
        }

        public static implicit operator double?(JsonObject jsonObject)
        {
            return jsonObject.As<double?>();
        }

        public static implicit operator decimal(JsonObject jsonObject)
        {
            return jsonObject.As<decimal>();
        }

        public static implicit operator decimal?(JsonObject jsonObject)
        {
            return jsonObject.As<decimal?>();
        }

        public static implicit operator bool(JsonObject jsonObject)
        {
            return jsonObject.As<bool>();
        }

        public static implicit operator bool?(JsonObject jsonObject)
        {
            return jsonObject.As<bool?>();
        }

        public static implicit operator JsonObject[](JsonObject jsonObject)
        {
            return jsonObject.AsArray();
        }

        public static implicit operator string[](JsonObject jsonObject)
        {
            return jsonObject.AsArray<string>();
        }

        public static implicit operator int[](JsonObject jsonObject)
        {
            return jsonObject.AsArray<int>();
        }

        public static implicit operator double[](JsonObject jsonObject)
        {
            return jsonObject.AsArray<double>();
        }

        public static implicit operator JsonObject(string value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(int value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(int? value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(long value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(long? value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(decimal value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(decimal? value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(double value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(double? value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(bool value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(bool? value)
        {
            return FromValue(value);
        }

        public static implicit operator JsonObject(Array arr)
        {
            return FromArray(arr.Cast<object>().Select(o => (o is JsonObject jsonObject) ? jsonObject : FromValue(o)));
        }

        public JsonObject[] AsArray()
        {
            return (JsonObject[]) _value;
        }

        public Array AsArray(Type elementType)
        {
            if (!IsArray)
                return Array.CreateInstance(elementType, 0);

            var sourceArray = AsArray();

            int len = sourceArray.Length;

            var array = Array.CreateInstance(elementType, len);

            for (int i=0;i<len;i++)
                array.SetValue(sourceArray[i].As(elementType),i);

            return array;
        }

        public T[] AsArray<T>()
        {
            if (!IsArray)
                return new T[0];

            return AsArray().Select(x => x.As<T>()).ToArray();
        }

        public IList AsList(Type elementType)
        {
            var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            if (!IsArray)
                return list;

            foreach (var o in AsArray())
                list.Add(o.As(elementType));

            return list;
        }

        public List<T> AsList<T>()
        {
            if (!IsArray)
                return new List<T>();

            return AsArray().Select(x => x.As<T>()).ToList();
        }

        public IEnumerable<T> AsEnumerable<T>()
        {
            if (!IsArray)
                return Enumerable.Empty<T>();

            return AsArray().Select(x => x.As<T>());
        }

        public IEnumerable AsEnumerable(Type elementType)
        {
            if (!IsArray)
                return Enumerable.Empty<object>();

            return AsArray().Select(x => x.As(elementType));
        }

        public Dictionary<string, JsonObject> AsDictionary()
        {
            return (Dictionary<string, JsonObject>) _value;
        }

        public bool HasField(string field) => IsObject && AsDictionary().ContainsKey(field);

        public string[] Keys => IsObject ? AsDictionary().Keys.ToArray() : new string[0];

        public void Set(JsonObject o)
        {
            _value = o?._value;
            _type = o?._type ?? JsonObjectType.Undefined;
        }

        public JsonObject this[string path]
        {
            get => FindNode(path, createIfNotExists:false);
            set => FindNode(path, createIfNotExists:true).Set(value);
        }

        public JsonObject this[int index]
        {
            get => FindNode(index, createIfNotExists:false);
            set => FindNode(index, createIfNotExists:true).Set(value);
        }

        public void Add(string key, JsonObject value)
        {
            this[key] = value;
        }

        private JsonObject FindNode(int index, bool createIfNotExists)
        {
            if (createIfNotExists)
            {
                if (!IsArray)
                    Set(EmptyArray());

                var arr = AsArray();

                int originalLength = arr.Length;

                if (index >= originalLength)
                {
                    Array.Resize(ref arr,index+1);

                    for (int i = originalLength; i <= index; i++)
                        arr[i] = Undefined();

                    _value = arr;
                }

                return arr[index];
            }
            else
            {
                if (!IsArray || index >= AsArray().Length)
                    return Undefined();

                return AsArray()[index];
            }
        }

        private JsonObject FindNode(string path, bool createIfNotExists)
        {
            int dotIndex = path.IndexOf('.');
            int bIndex = path.IndexOf('[');

            if (bIndex < 0 && dotIndex < 0 && IsObject)
            {
                if (AsDictionary().TryGetValue(path, out var value))
                    return value;
            }

            int nextIndex = -1;

            string firstKey = path;

            if (dotIndex > 0 && (bIndex < 0 || dotIndex < bIndex))
            {
                firstKey = path.Substring(0, dotIndex);
                nextIndex = dotIndex + 1;
            }
            else if (bIndex > 0 && (dotIndex < 0 || bIndex < dotIndex))
            {
                firstKey = path.Substring(0, bIndex);
                nextIndex = bIndex;
            }

            if (nextIndex >= 0)
            {
                if (createIfNotExists)
                {
                    if (!IsObject)
                        Set(EmptyObject());

                    var dict = AsDictionary();

                    if (!dict.ContainsKey(firstKey))
                        dict[firstKey] = Undefined();
                }

                return this[firstKey].FindNode(path.Substring(nextIndex), createIfNotExists);
            }

            if (bIndex == 0)
            {
                bIndex = path.IndexOf(']');

                if (bIndex < 2)
                    return Undefined();

                int index = path.Substring(1, bIndex - 1).To<int>();

                if (index < 0)
                    return Undefined();

                if (createIfNotExists)
                {
                    if (!IsArray)
                        Set(EmptyArray());

                    var arr = AsArray();

                    if (index >= arr.Length)
                    {
                        int oldLength = arr.Length;

                        Array.Resize(ref arr,index+1);

                        for (int i = oldLength; i <= index; i++)
                            arr[i] = Undefined();

                        _value = arr;
                    }
                }

                if (bIndex + 1 >= path.Length)
                    return this[index];

                if (path[bIndex + 1] == '.')
                    return this[index].FindNode(path.Substring(bIndex + 2), createIfNotExists);
                else
                    return this[index].FindNode(path.Substring(bIndex + 1), createIfNotExists);
            }

            var returnValue = Undefined();

            if (createIfNotExists)
            {
                if (!IsObject)
                    Set(EmptyObject());

                AsDictionary()[path] = returnValue;
            }

            return returnValue;
        }

        public void FillObject(object obj)
        {
            if (!IsObject)
                return;

            var fieldsInType = obj.GetType().Inspector().GetFieldsAndProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            foreach (var jsonField in AsDictionary())
            {
                var field = fieldsInType.FirstOrDefault(f => string.Equals(f.Name, jsonField.Key, StringComparison.OrdinalIgnoreCase));

                if (field == null)
                    continue;

                var existingObject = field.GetValue(obj);

                if (existingObject != null && jsonField.Value.IsObject)
                    jsonField.Value.FillObject(existingObject);
                else if (field.CanWrite)
                    field.SetValue(obj, jsonField.Value.As(field.Type));
            }
        }
        
        public IEnumerator<JsonObject> GetEnumerator()
        {
            if (IsObject)
            {
                return AsDictionary().Values.GetEnumerator();
            }
            else if (IsArray)
            {
                return (from obj in AsArray() select obj).GetEnumerator();
            }
            else if (IsUndefined)
            {
                return Enumerable.Empty<JsonObject>().GetEnumerator();
            }
            else
            {
                return Enumerable.Repeat(this, 1).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}