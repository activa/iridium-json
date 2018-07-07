using Iridium.Reflection;
using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonDynamicObjectTests
    {
        private JsonObject _json;

        [OneTimeSetUp]
        public void SetupJsonObject()
        {
            _json = JsonParser.Parse(JsonSerializer.ToJson(new
            {
                a = 123,
                arrayValue = new object[]
                {
                    new {a = 1},
                    new {a = 2},
                    new {arr = new[] {1, 2, 3}}
                },
                objectValue = new
                {
                    a = 1,
                    b = 2,
                    c = 3
                }
            }));
        }

        [Test]
        public void IntValue()
        {
            IDynamicObject dyn = (JsonObject)123;

            Assert.That(dyn.IsValue);

            Assert.That(dyn.TryGetValue(out var value, out var type), Is.True);
            Assert.That(value, Is.EqualTo(123));
            Assert.That(type, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void BoolValue()
        {
            IDynamicObject dyn = (JsonObject)true;

            Assert.That(dyn.IsValue);

            Assert.That(dyn.TryGetValue(out var value, out var type), Is.True);
            Assert.That(value, Is.EqualTo(true));
            Assert.That(type, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void IntFieldExisting()
        {
            IDynamicObject dyn = _json;

            Assert.That(dyn.IsObject);

            Assert.That(dyn.TryGetValue("a", out var value, out var type), Is.True);

            Assert.That(value, Is.InstanceOf<JsonObject>());
            Assert.That(value, Is.InstanceOf<IDynamicObject>());

            IDynamicObject dynField = (IDynamicObject) value;

            Assert.That(dynField.IsValue, Is.True);
            Assert.That(dynField.TryGetValue(out var fieldValue, out var fieldType), Is.True);
            Assert.That(fieldValue, Is.EqualTo(123));
        }

        [Test]
        public void ArrayFieldExisting()
        {
            IDynamicObject dyn = _json;

            Assert.That(dyn.IsObject);

            Assert.That(dyn.TryGetValue("arrayValue", out var value, out var type), Is.True);

            Assert.That(value, Is.InstanceOf<JsonObject>());
            Assert.That(value, Is.InstanceOf<IDynamicObject>());

            IDynamicObject dynField = (IDynamicObject) value;

            Assert.That(dynField.IsArray, Is.True);
            Assert.That(dynField.TryGetValue(0, out var fieldValue, out var fieldType), Is.True);
            Assert.That(fieldValue, Is.InstanceOf<JsonObject>());

            Assert.That(dynField.TryGetValue(1, out var fieldValue2, out var fieldType2), Is.True);
            Assert.That(dynField.TryGetValue(2, out var fieldValue3, out var fieldType3), Is.True);
            Assert.That(dynField.TryGetValue(3, out var fieldValue4, out var fieldType4), Is.False);
        }

        [Test]
        public void FieldNonExisting()
        {
            IDynamicObject dyn = _json;

            Assert.That(dyn.IsObject);

            Assert.That(dyn.TryGetValue("someValue", out var value, out var type), Is.False);
        }

    }
}