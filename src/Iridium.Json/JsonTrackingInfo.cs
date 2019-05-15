#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2019 Philippe Leybaert
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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Iridium.Json
{
    public class JsonTrackingInfo
    {
        public JsonTrackingInfo(JsonObject parent) => ParentObject = parent;
        public JsonTrackingInfo(JsonObject parent, string key) : this(parent) => ParentKey = key;
        public JsonTrackingInfo(JsonObject parent, int index) : this(parent) => ParentIndex = index;

        public JsonObject ParentObject { get; private set; }
        public string ParentKey { get; private set; }
        public int? ParentIndex { get; private set; }

        public bool IsRoot => ParentObject == null;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnValueChanged(JsonObject obj)
        {
            PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs("Value"));

            if (obj.TrackingInfo.IsRoot)
                return;

            var root = obj.FindRoot();

            root?.TrackingInfo.OnValueChanged(root);
        }

        internal JsonObject FindRoot()
        {
            if (ParentObject == null)
                return null;

            var obj = ParentObject;

            while (obj.TrackingInfo.ParentObject != null)
                obj = obj.TrackingInfo.ParentObject;

            return obj;
        }


        internal string Key()
        {
            if (ParentObject == null)
                return null;

            var parentPath = ParentObject.Path;

            if (ParentKey != null)
            {
                if (parentPath != null)
                    return parentPath + "." + ParentKey;
                else
                    return ParentKey;
            }

            if (ParentIndex != null)
            {
                if (parentPath != null)
                    return parentPath + "[" + ParentIndex + "]";
                else
                    return "[" + ParentIndex + "]";
            }

            return null;
        }

        internal void Update(JsonObject parent, string key)
        {
            ParentObject = parent;
            ParentKey = key;
        }

        internal void Update(JsonObject parent, int index)
        {
            ParentObject = parent;
            ParentIndex = index;
        }

    }
}