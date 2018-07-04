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

    }
}