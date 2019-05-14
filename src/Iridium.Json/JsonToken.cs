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

namespace Iridium.Json
{
    internal class JsonToken
    {
        public JsonTokenType Type { get; }
        public string Token { get; }

        public JsonToken(JsonTokenType type, string token)
        {
            Type = type;
            Token = token;
        }

        public JsonToken(JsonTokenType type)
        {
            Type = type;
            Token = null;
        }

        public static readonly JsonToken Colon = new JsonToken(JsonTokenType.Colon);
        public static readonly JsonToken Comma = new JsonToken(JsonTokenType.Comma);
        public static readonly JsonToken ObjectStart = new JsonToken(JsonTokenType.ObjectStart);
        public static readonly JsonToken ObjectEnd = new JsonToken(JsonTokenType.ObjectEnd);
        public static readonly JsonToken ArrayStart = new JsonToken(JsonTokenType.ArrayStart);
        public static readonly JsonToken ArrayEnd = new JsonToken(JsonTokenType.ArrayEnd);
        public static readonly JsonToken Eof = new JsonToken(JsonTokenType.EOF);
        public static readonly JsonToken True = new JsonToken(JsonTokenType.True);
        public static readonly JsonToken False = new JsonToken(JsonTokenType.False);
        public static readonly JsonToken Null = new JsonToken(JsonTokenType.Null);
    }
}