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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Iridium.Json
{
    internal class JsonTokenizer
    {
        public JsonTokenizer(string s)
        {
            _charFeeder = new StringCharFeeder(s);
        }

        public JsonTokenizer(Stream stream)
        {
            _charFeeder = new StreamCharFeeder(stream);
        }

        private abstract class CharFeeder
        {
            public char Current { get; private set; }

            private bool _backTracked;

            protected abstract int ReadNext();

            public char Next()
            {
                if (_backTracked)
                {
                    _backTracked = false;
                }
                else
                {
                    int next = ReadNext();

                    if (next < 0)
                        Current = (char) 0;
                    else
                        Current = (char) next;
                }

                return Current;
            }

            public void Backtrack()
            {
                _backTracked = true;
            }
        }

        private class StringCharFeeder : CharFeeder
        {
            private readonly string _s;
            private readonly int _len;
            private int _index;

            public StringCharFeeder(string s)
            {
                _s = s;
                _index = 0;
                _len = s.Length;
            }

            protected override int ReadNext()
            {
                return _index >= _len ? -1 : _s[_index++];
            }
        }

        private class StreamCharFeeder : CharFeeder
        {
            private readonly StreamReader _reader;

            public StreamCharFeeder(Stream stream)
            {
                _reader = new StreamReader(stream, Encoding.UTF8);
            }

            protected override int ReadNext()
            {
                return _reader.Read();
            }
        }


        private readonly CharFeeder _charFeeder;

        private static readonly Dictionary<string, JsonToken> _keywords = new Dictionary<string, JsonToken>()
        {
            {"null", JsonToken.Null},
            {"true", JsonToken.True},
            {"false", JsonToken.False}
        };

        public JsonToken NextToken()
        {
            for (;;)
            {
                char c = _charFeeder.Next();

                if (char.IsWhiteSpace(c))
                    continue;

                switch (c)
                {
                    case ',':
                        return JsonToken.Comma;
                    case ':':
                        return JsonToken.Colon;
                    case '"':
                        return ReadStringToken();
                    case '{':
                        return JsonToken.ObjectStart;
                    case '}':
                        return JsonToken.ObjectEnd;
                    case '[':
                        return JsonToken.ArrayStart;
                    case ']':
                        return JsonToken.ArrayEnd;
                    case 'n':
                    case 't':
                    case 'f':
                        return ReadKeyword();
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return ReadNumber();
                    case (char)0:
                        return JsonToken.Eof;
                    default:
                        throw new Exception("Unexpected json character " + c);
                }
            }
        }

        private JsonToken ReadKeyword()
        {
            char c = _charFeeder.Current;

            string keyword = c.ToString();

            for (;;)
            {
                if (_keywords.TryGetValue(keyword, out var match))
                {
                    return match;
                }

                c = _charFeeder.Next();

                if (!char.IsLetter(c))
                    throw new Exception($"Invalid keyword {keyword}");

                keyword += c;
            }
        }

        private JsonToken ReadNumber()
        {
            bool hasDot = false;
            bool hasExp = false;

            string s = new string(_charFeeder.Current,1);

            for (;;)
            {
                char c = _charFeeder.Next();

                if ((c == 'e' || c == 'E') && !hasExp)
                {
                    hasExp = true;
                }
                else if ((c == '+' || c == '-') && hasExp)
                {
                }
                else if (c == '.' && !hasDot)
                {
                    hasDot = true;
                }
                else if (char.IsDigit(c))
                {
                }
                else
                {
                    _charFeeder.Backtrack();

                    return new JsonToken((hasDot || hasExp) ? JsonTokenType.Float : JsonTokenType.Integer, s);
                }

                s += c;
            }
        }

        private JsonToken ReadStringToken()
        {
            bool inEscape = false;
            bool foundEscape = false;
            StringBuilder s = new StringBuilder();

            for (;;)
            {
                char c = _charFeeder.Next();

                if (c == 0)
                    throw new Exception("JSON: Unexpected EOF ");

                if (inEscape)
                {
                    inEscape = false;
                }
                else if (c == '\\')
                {
                    inEscape = true;
                    foundEscape = true;
                }
                else if (c == '"')
                {
                    if (foundEscape)
                    {
                        s.Replace("\\n", "\n");
                        s.Replace("\\r", "\r");
                        s.Replace("\\t", "\t");
                        s.Replace("\\\"", "\"");

                        if (s.ToString().IndexOf("\\u", StringComparison.Ordinal) >= 0)
                        {
                            s = new StringBuilder(Regex.Replace(s.ToString(), @"\\[uU][a-fA-F0-9]{4}", m => ((char) uint.Parse(m.Value.Substring(2), NumberStyles.HexNumber)).ToString()));
                        }

                        s.Replace("\\\\", "\\");
                        s.Replace("\\/", "/");
                    }

                    return new JsonToken(JsonTokenType.String, s.ToString());
                }

                s.Append(c);
            }
        }
    }
}