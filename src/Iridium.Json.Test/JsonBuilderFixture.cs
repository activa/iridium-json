using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iridium.Reflection;
using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonBuilderFixture
    {
        private JsonObject _json;

        [OneTimeSetUp]
        public void SetupObject()
        {
            _json = new JsonObject()
            {
                {"value1", "string1"},
                {"value2", "string2"},
                {"bool1", true},
                {"bool2", false},
                {"arrayInts" , new[] {1,2,3}},
                {"object1", new JsonObject
                                {
                                    {"value3", 123}
                                }
                }
            };

            _json.ValidateTracking();
        }

        [Test]
        public void BasicObjectProperties()
        {
            Assert.That(_json.IsObject);

            Assert.That(_json.Keys.Length, Is.EqualTo(6));
        }

        [Test]
        public void StringValues()
        {
            Assert.That((string)_json["value1"], Is.EqualTo("string1"));
            Assert.That((string)_json["value2"], Is.EqualTo("string2"));
        }

        [Test]
        public void ArrayValues()
        {
            Assert.That(_json["arrayInts"].IsArray);
            Assert.That(_json["arrayInts"].AsArray<int>(), Is.EquivalentTo(new[] {1,2,3}));
        }

        [Test]
        public void ObjectValues()
        {
            Assert.That(_json["object1"].IsObject);
            Assert.That((int)_json["object1"]["value3"], Is.EqualTo(123));
        }

        [Test]
        public void TestCreateFromInt()
        {
            JsonObject json = new JsonObject(123);

            Assert.That(json.IsValue);
            Assert.That(json.Value, Is.EqualTo(123));
        }

        [Test]
        public void TestCreateFromAnonymousObject()
        {
            var o = new
            {
                intValue = 123,
                stringValue = "ABC",
                dictionary = new Dictionary<string, object>()
                {
                    {"key1" , new { stringValue = "string234" }},
                    {"key2" , new { intValue = 234 }},
                },
                objectValue = new
                {
                    intValue = 345,
                    doubleValue = 456.0,
                    decimalValue = 567m
                },
                array = new [] { 1,2,3,4,5}
            };

            JsonObject json = new JsonObject(o);

            json.ValidateTracking();
            json["objectValue"].ValidateTracking(true);

            Assert.That(json.IsObject);

            Assert.That(json["intValue"].IsValue);
            Assert.That(json["intValue"].Value, Is.EqualTo(o.intValue));
            
            Assert.That(json["stringValue"].IsValue);
            Assert.That(json["stringValue"].Value, Is.EqualTo(o.stringValue));

            Assert.That(json["dictionary"].IsObject);
            Assert.That(json["dictionary"]["key1"].IsObject);
            Assert.That(json["dictionary"]["key2"].IsObject);
            Assert.That(json["dictionary"]["key1"]["stringValue"].IsValue);
            Assert.That(json["dictionary"]["key1"]["stringValue"].Value, Is.EqualTo("string234"));
            Assert.That(json["dictionary"]["key2"]["intValue"].IsValue);
            Assert.That(json["dictionary"]["key2"]["intValue"].Value, Is.EqualTo(234));

            Assert.That(json["objectValue"].IsObject);
            Assert.That(json["objectValue"]["intValue"].IsValue);
            Assert.That(json["objectValue"]["intValue"].Value, Is.EqualTo(345));
            Assert.That(json["objectValue"]["doubleValue"].IsValue);
            Assert.That(json["objectValue"]["doubleValue"].Value, Is.EqualTo(456.0).Within(0.001));
            Assert.That(json["objectValue"]["decimalValue"].IsValue);
            Assert.That(json["objectValue"]["decimalValue"].Value, Is.EqualTo(567m));

            Assert.That(json["array"].IsArray);
            Assert.That(json["array"].As<int[]>(), Is.EquivalentTo(new[] {1,2,3,4,5}));
        }

        [Test]
        public void TestCreateFromObjectWithCircularRefs()
        {
            json_ParentClass obj1 = new json_ParentClass();
            json_ParentClass obj2 = new json_ParentClass();
            json_ParentClass obj3 = new json_ParentClass();

            obj1.Id = "A";
            obj2.Id = "B";
            obj3.Id = "C";

            obj1.Children = new []
            {
                new json_ChildClass
                {
                    Id = "A1",
                    Parent = obj1,
                    Others = new [] { obj1,obj2,obj3 }
                }
            };

            var json = new JsonObject(obj1);

            Assert.That(json["Id"].Value, Is.EqualTo("A"));
            Assert.That(json["Children"][0]["Id"].Value, Is.EqualTo("A1"));
            Assert.That(json["Children"][0]["Parent"].IsNull); // circular reference

            Assert.That(json["Children"][0]["Others"][0].IsNull); // circular reference
            Assert.That(json["Children"][0]["Others"][1].IsObject);
            Assert.That(json["Children"][0]["Others"][2].IsObject);
        }

        private class json_ParentClass
        {
            public string Id;
            public json_ChildClass[] Children;

            public override string ToString()
            {
                return $"Parent({Id})";
            }
        }

        private class json_ChildClass
        {
            public string Id;
            public json_ParentClass Parent;
            public json_ParentClass[] Others;

            public override string ToString()
            {
                return $"Child({Id})";
            }

        }

    }

    [TestFixture]
    public class DeepCloneTestFixture
    {
        [Test]
        public void DeepCloneTest()
        {
            JsonObject json1 = new JsonObject()
            {
                {"value1" , 123},
                {"value2" , "ABC"},
                {"obj" , new JsonObject(new { value1 = 234})}
            };

            var json2 = json1.Clone();

            Assert.That(json1["value1"].Value, Is.EqualTo(json2["value1"].Value));
            Assert.That(json1["obj"]["value1"].Value, Is.EqualTo(json2["obj"]["value1"].Value));

            Assert.That(!ReferenceEquals(json1["value1"], json2["value1"]));
            Assert.That(!ReferenceEquals(json1["obj"]["value1"], json2["obj"]["value1"]));
        }
    }
}