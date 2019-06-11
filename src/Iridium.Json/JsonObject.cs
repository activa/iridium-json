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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Iridium.Reflection;

namespace Iridium.Json
{
    public class JsonObject : IEnumerable<JsonObject> , IDynamicObject, INotifyPropertyChanged
    {
        private static readonly HashSet<Type> _simpleTypes = new HashSet<Type>(new[] { typeof(string),typeof(int),typeof(int?),typeof(uint),typeof(uint?),typeof(char),typeof(char?),typeof(byte),typeof(byte?),typeof(sbyte),typeof(sbyte?),typeof(short),typeof(short?),typeof(ushort),typeof(ushort?),typeof(long),typeof(long?),typeof(ulong),typeof(ulong?),typeof(decimal),typeof(decimal?),typeof(double),typeof(double?),typeof(float),typeof(float?),typeof(bool),typeof(bool?) });

        private object _value;
        private JsonObjectType _type;
        private JsonTrackingInfo _trackingInfo;

        public JsonObject()
        {
            _type = JsonObjectType.Undefined;

            _trackingInfo = new JsonTrackingInfo(null);
        }

        public JsonObject(object valueOrObject)
        {
            JsonObject o = FromObject(valueOrObject);

            _value = o?._value;
            _type = o?._type ?? JsonObjectType.Undefined;

            UpdateTracking();
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

        private JsonObject(JsonObjectType type, object value = null)
        {
            _type = type;

            switch (type)
            {
                case JsonObjectType.Value:
                    _value = value;
                    break;
                case JsonObjectType.Array:
                    _value = new JsonObject[0];
                    break;
                case JsonObjectType.Object:
                    _value = new Dictionary<string,JsonObject>();
                    break;
            }
        }

        public bool IsObject => _type == JsonObjectType.Object;
        public bool IsArray => _type == JsonObjectType.Array;
        public bool IsValue => _type == JsonObjectType.Value;
        public bool IsUndefined => _type == JsonObjectType.Undefined || (_trackingInfo != null && _trackingInfo.Temporary);
        public bool IsNull => _value == null && _type != JsonObjectType.Undefined;
        public bool IsNullOrUndefined => _value == null;
        public object Value => IsValue ? _value : null;

        public JsonTrackingInfo TrackingInfo => _trackingInfo;
        public string Path => _trackingInfo?.Key();
        public bool IsReadOnly => _trackingInfo == null;

        public static JsonObject Undefined() => new JsonObject(JsonObjectType.Undefined);
        public static JsonObject EmptyObject() => new JsonObject(JsonObjectType.Object);
        public static JsonObject EmptyArray() => new JsonObject(JsonObjectType.Array);
        public static JsonObject Null() => new JsonObject(JsonObjectType.Value, null);

        internal static JsonObject FromValue(object value) => new JsonObject(JsonObjectType.Value, value);
        internal static JsonObject FromArray(IEnumerable<JsonObject> array) => new JsonObject(array);
        internal static JsonObject FromDictionary(Dictionary<string,JsonObject> dictionary) => new JsonObject(dictionary);

        private static JsonObject FromObject(object obj, Stack<object> circularStack = null)
        {
            if (obj == null)
                return Null();

            if (obj is char || obj is Enum || obj is Guid)
                return FromValue("" + obj);

            if (obj is DateTime)
                return FromValue(obj);

            var type = obj.GetType();

            if (_simpleTypes.Contains(type))
                return FromValue(obj);

            if (obj is JsonObject jsonObject)
                return jsonObject.Clone();

            if (circularStack == null)
                circularStack = new Stack<object>();

            if (circularStack.Any(o => ReferenceEquals(o,obj)))
                return Null();

            circularStack.Push(obj);

            try
            {
                if (obj is IDictionary dictionary)
                {
                    var jObj = EmptyObject();

                    foreach (DictionaryEntry entry in dictionary)
                        if (entry.Key is string key)
                            jObj.Add(key, FromObject(entry.Value, circularStack));

                    return jObj;
                }
                
                if (obj is ICollection collection)
                    return FromArray(collection.OfType<object>().Select(o => FromObject(o, circularStack)));

                var dic = obj.GetType().Inspector().GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public).Where(f => f.CanRead).ToDictionary(f => f.Name, f => FromObject(f.GetValue(obj), circularStack));

                return FromDictionary(dic);
            }
            finally
            {
                circularStack.Pop();
            }
        }

        public JsonObject Clone()
        {
            switch (_type)
            {
                case JsonObjectType.Value:
                    return FromValue(_value);

                case JsonObjectType.Array:
                    return FromArray(AsArray().Select(j => j.Clone()));

                case JsonObjectType.Object:
                    return FromDictionary(AsDictionary().ToDictionary(kv => kv.Key, kv => kv.Value.Clone()));
 
                default:
                    return Undefined();
            }
        }

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
            return FromArray(arr.Cast<object>().Select(o => (o is JsonObject jsonObject) ? jsonObject : FromObject(o)));
        }

        public JsonObject[] AsArray()
        {
            if (!IsArray)
                return new JsonObject[0];

            return _value as JsonObject[];
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
            return _value as Dictionary<string, JsonObject>;
        }

        public bool HasField(string field) => IsObject && AsDictionary().ContainsKey(field);

        public string[] Keys => IsObject ? AsDictionary().Keys.ToArray() : new string[0];

        public void Set(JsonObject o)
        {
            if (IsReadOnly)
                throw new Exception("JsonObject is read-only");

            _value = o?._value;
            _type = o?._type ?? JsonObjectType.Undefined;

            ClearTemporary();

            UpdateTracking();

            _trackingInfo.OnValueChanged(this);
        }

        private void ClearTemporary()
        {
            if (TrackingInfo.IsRoot || !TrackingInfo.ParentObject.TrackingInfo.Temporary)
                return;

            JsonObject j = TrackingInfo.ParentObject;

            while (j != null && j.TrackingInfo.Temporary)
            {
                j.TrackingInfo.Temporary = false;
                j = j.TrackingInfo.ParentObject;
            }
        }

        public JsonObject this[string path]
        {
            get => FindNode(path);
            set => FindNode(path, true).Set(value);
        }

        public JsonObject this[int index]
        {
            get => FindNode(index);
            set => FindNode(index).Set(value);
        }

        public void Add(string key, JsonObject value)
        {
            if (_type == JsonObjectType.Undefined)
            {
                _type = JsonObjectType.Object;
                _value = new Dictionary<string,JsonObject>();
            }
            else if (_type != JsonObjectType.Object)
                throw new Exception("JsonObject.Add() called on non-object");

            AsDictionary()[key] = value;

            UpdateTracking();
        }

        public bool TryGetValue(out object value, out Type type)
        {
            if (IsUndefined)
            {
                value = null;
                type = null;
                return false;
            }

            value = Value;
            type = value == null ? typeof(object) : value.GetType();

            return true;
        }

        public bool TryGetValue(string key, out object value, out Type type)
        {
            type = typeof(JsonObject);

            if (IsObject)
            {
                if (AsDictionary().TryGetValue(key, out var obj))
                {
                    value = obj;

                    return true;
                }
            }

            value = null;

            return false;
        }

        public bool TryGetValue(int index, out object value, out Type type)
        {
            type = typeof(JsonObject);

            if (IsArray)
            {
                var array = AsArray();

                if (index >= 0 && index < array.Length)
                {
                    value = array[index];

                    return true;
                }
            }

            value = null;

            return false;
        }

        private JsonObject FindNode(int index)
        {
            if (_trackingInfo != null)
            {
                if (!IsUndefined && !IsArray)
                    return Undefined();

                if (!IsArray)
                {
                    _type = JsonObjectType.Array;
                    _value = new JsonObject[0];
                    _trackingInfo.Temporary = true;
                }

                var arr = AsArray();

                int originalLength = arr.Length;

                if (index >= originalLength)
                {
                    Array.Resize(ref arr,index+1);

                    for (int i = originalLength; i <= index; i++)
                    {
                        arr[i] = Undefined();
                    }

                    _value = arr;

                    UpdateTracking();
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

        private JsonObject FindNode(string path, bool forceCreate = false)
        {
            if (string.IsNullOrEmpty(path))
                return Undefined();

            int dotIndex = path.IndexOf('.');
            int bIndex = path.IndexOf('[');

            if (bIndex < 0 && dotIndex < 0) // just a simple key
            {
                if (IsObject && AsDictionary().TryGetValue(path, out var value))
                    return value;

                var returnValue = Undefined();

                if (_trackingInfo != null)
                {
                    if (!IsUndefined && !IsObject && !forceCreate)
                    {
                        return returnValue;
                    }

                    if (!IsObject)
                    {
                        _type = JsonObjectType.Object;
                        _value = new Dictionary<string,JsonObject>();
                        _trackingInfo.Temporary = true;
                    }

                    AsDictionary()[path] = returnValue;

                    returnValue._trackingInfo = new JsonTrackingInfo(this, path);
                }

                return returnValue;
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
                return FindNode(firstKey).FindNode(path.Substring(nextIndex));
            }

            if (bIndex == 0)
            {
                bIndex = path.IndexOf(']');

                if (bIndex < 2)
                    return Undefined();

                int index = path.Substring(1, bIndex - 1).To<int>();

                if (index < 0)
                    return Undefined();

                if (bIndex + 1 >= path.Length)
                    return FindNode(index);

                if (path[bIndex + 1] == '.')
                    return FindNode(index).FindNode(path.Substring(bIndex + 2));
                else
                    return FindNode(index).FindNode(path.Substring(bIndex + 1));
            }

            return Undefined();
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

        private void UpdateTracking(bool remove = false)
        {
            switch (_type)
            {
                case JsonObjectType.Array:
                {
                    var arr = AsArray();

                    for (var i = 0; i < arr.Length; i++)
                    {
                        var o = arr[i];

                        if (!remove)
                        {
                            if (o._trackingInfo == null)
                                o._trackingInfo = new JsonTrackingInfo(this, i);
                            else
                                o._trackingInfo.Update(this,i);
                        }

                        o.UpdateTracking(remove);
                    }

                    break;
                }
                    
                case JsonObjectType.Object:
                {
                    foreach (var kvp in AsDictionary())
                    {
                        var o = kvp.Value;

                        if (!remove)
                        {
                            if (o._trackingInfo == null)
                                o._trackingInfo = new JsonTrackingInfo(this, kvp.Key);
                            else
                                o._trackingInfo.Update(this, kvp.Key);
                        }

                        o.UpdateTracking(remove);
                    }

                    break;
                }
            }

            if (_trackingInfo == null && !remove)
            {
                _trackingInfo = new JsonTrackingInfo(null);
            }
            else if (_trackingInfo != null && remove)
            {
                _trackingInfo = null;
            }
        }

        public JsonObject FindRoot()
        {
            if (_trackingInfo == null)
                return null;

            var root = _trackingInfo.FindRoot();

            return root ?? this;
        }

        public JsonObject MakeWritable()
        {
            if (_trackingInfo == null)
                UpdateTracking();

            return this;
        }

        public JsonObject MakeWritableClone()
        {
            return Clone().MakeWritable();
        }

        public JsonObject MakeReadOnly()
        {
            UpdateTracking(true);

            return this;
        }

        public JsonObject MakeReadOnlyClone()
        {
            return Clone();
        }

        public IEnumerator<JsonObject> GetEnumerator()
        {
            if (IsArray)
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

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_trackingInfo != null) 
                    _trackingInfo.PropertyChanged += value;
            }
            remove
            {
                if (_trackingInfo != null)
                    _trackingInfo.PropertyChanged -= value;
            }
        }
    }
}