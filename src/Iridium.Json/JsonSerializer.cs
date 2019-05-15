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
using System.Globalization;
using System.Linq;
using System.Text;
using Iridium.Reflection;

namespace Iridium.Json
{
    public enum JsonDateFormat
    {
        SlashDate,
        EscapedSlashDate,
        NewDate,
        Date,
        UtcISO,
        LocalISO
    }

    public class JsonSerializer
    {
        private readonly StringBuilder _output = new StringBuilder();
        private readonly JsonDateFormat _dateFormat;
        private readonly Stack<object> _circularStack = new Stack<object>();

        private JsonSerializer()
        {
            _dateFormat = JsonDateFormat.UtcISO;
        }

        private JsonSerializer(JsonDateFormat dateFormat)
        {
            _dateFormat = dateFormat;
        }

        public static string ToJson(object obj,JsonDateFormat dateFormat)
        {
            return new JsonSerializer(dateFormat).ConvertToJson(obj);
        }

        public static string ToJson(object obj)
        {
            return new JsonSerializer().ConvertToJson(obj);
        }

        private string ConvertToJson(object obj)
        {
            WriteValue(obj);

            return _output.ToString();
        }

        private void WriteValue(object obj)
        {
            if (obj == null)
                _output.Append("null");
            else if (obj is JsonObject jsonObject)
                WriteJsonObject(jsonObject);
            else if (obj is sbyte || obj is byte || obj is short || obj is ushort || obj is int || obj is uint || obj is long || obj is ulong || obj is decimal || obj is double || obj is float)
                _output.Append(Convert.ToString(obj, NumberFormatInfo.InvariantInfo));
            else if (obj is bool)
                _output.Append(obj.ToString().ToLower());
            else if (obj is char || obj is Enum || obj is Guid)
                WriteString("" + obj);
            else if (obj is DateTime dateTime)
                WriteDate(dateTime);
            else if (obj is string s)
                WriteString(s);
            else if (obj is IDictionary dictionary)
                WriteDictionary(dictionary);
            else if (obj is IEnumerable enumerable)
                WriteArray(enumerable);
            else
                WriteObject(obj);
        }

        private void WriteDate(DateTime date)
        {
            long ticks = ((long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);

            switch (_dateFormat)
            {
                case JsonDateFormat.NewDate: 
                    _output.AppendFormat("new Date({0})",ticks);
                    break;

                case JsonDateFormat.Date:
                    _output.AppendFormat("\"Date({0})\"", ticks);
                    break;

                case JsonDateFormat.SlashDate:
                    _output.AppendFormat("\"/Date({0})/\"", ticks);
                    break;

                case JsonDateFormat.EscapedSlashDate:
                    _output.AppendFormat("\"\\/Date({0})\\/\"", ticks);
                    break;

                case JsonDateFormat.UtcISO:
                    _output.AppendFormat("\"{0:yyyy-MM-ddTHH:mm:ssZ}\"", date.ToUniversalTime());
                    break;

                case JsonDateFormat.LocalISO:
                    _output.AppendFormat("\"{0:yyyy-MM-ddTHH:mm:ss}\"", date.ToLocalTime());
                    break;
            }
        }

        private void WriteJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
                return;

            if (jsonObject.IsArray)
            {
                WriteArray(jsonObject.AsArray());
            }
            else if (jsonObject.IsValue)
            {
                WriteValue(jsonObject.Value);
            }
            else if (jsonObject.IsObject)
            {
                WriteDictionary(jsonObject.AsDictionary());
            }
            else
            {
                WriteValue(null);
            }
        }

        private void WriteObject(object obj)
        {
            if (_circularStack.Any(o => o == obj))
            {
                WriteValue(null);
                return;
            }

            _circularStack.Push(obj);

            _output.Append('{');

            bool pendingSeparator = false;

            foreach (var fieldInfo in obj.GetType().Inspector().GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!fieldInfo.CanRead)
                    continue;

                if (pendingSeparator)
                    _output.Append(',');

                WritePair(fieldInfo.Name, fieldInfo.GetValue(obj));

                pendingSeparator = true;
            }

            _output.Append('}');

            _circularStack.Pop();
        }

        private void WritePair(string name, object value)
        {
            WriteString(name);

            _output.Append(':');

            WriteValue(value);
        }

        private void WriteArray(IEnumerable array)
        {
            _output.Append('[');

            bool pendingSeparator = false;

            foreach (object obj in array)
            {
                if (pendingSeparator)
                    _output.Append(',');

                WriteValue(obj);

                pendingSeparator = true;
            }

            _output.Append(']');
        }

        private void WriteDictionary(IDictionary dic)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (entry.Key is string key)
                {
                    if (entry.Value is JsonObject jsonObject && jsonObject.IsUndefined)
                        continue;

                    if (pendingSeparator)
                        _output.Append(',');

                    WritePair(key, entry.Value);

                    pendingSeparator = true;
                }
            }

            _output.Append('}');
        }

        private void WriteString(string s)
        {
            _output.Append('\"');

            int l = s.Length;

            for (var i = 0; i < l; i++)
            {
                char c = s[i];

                switch (c)
                {
                    case '\t':
                        _output.Append("\\t");
                        break;
                    case '\r':
                        _output.Append("\\r");
                        break;
                    case '\n':
                        _output.Append("\\n");
                        break;
                    case '"':
                    case '\\':
                        _output.Append("\\" + c);
                        break;
                    default:
                    {
                        if (c >= ' ' && c < 128)
                            _output.Append(c);
                        else
                            _output.Append("\\u" + ((int) c).ToString("X4"));
                    }
                        break;
                }
            }

            _output.Append('\"');
        }
    }
}
