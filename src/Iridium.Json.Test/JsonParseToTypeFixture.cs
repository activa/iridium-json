using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonParseToTypeFixture
    {
        private class _TestClassInts
        {
            public _TestClassInts()
            {
            }

            public _TestClassInts(int roValue)
            {
                IntProp1ReadOnly = roValue;
            }

            public int IntField1;
            public int IntProp1 { get; set; }
            public int IntProp1ReadOnly { get; }
            public int IntProp1Ignore { get; set; }
        }

        private class _TestClassIntsNested
        {
            public _TestClassInts sub1;
            public _TestClassInts sub2 = new _TestClassInts(999);
        }

        [Test]
        public void IntField()
        {
            var json = JsonSerializer.ToJson(new
            {
                intfield1 = 123,
                intprop1 = 456,
                intprop1readonly = 789
            });

            var obj = JsonParser.Parse(json).As<_TestClassInts>();

            Assert.That(obj.IntField1, Is.EqualTo(123));
            Assert.That(obj.IntProp1, Is.EqualTo(456));
            Assert.That(obj.IntProp1ReadOnly, Is.EqualTo(0));
        }

        [Test]
        public void IntFieldNested()
        {
            var json = JsonSerializer.ToJson(new
            {
                sub1 = new
                {
                    intfield1 = 123,
                    intprop1 = 456,
                    intprop1readonly = 789

                },
                sub2 = new
                {
                    intfield1 = 1234,
                    intprop1 = 4567,
                    intprop1readonly = 7890

                }
            });

            var obj = JsonParser.Parse(json).As<_TestClassIntsNested>();

            Assert.That(obj.sub1.IntField1, Is.EqualTo(123));
            Assert.That(obj.sub1.IntProp1, Is.EqualTo(456));
            Assert.That(obj.sub1.IntProp1ReadOnly, Is.EqualTo(0));
            Assert.That(obj.sub2.IntField1, Is.EqualTo(1234));
            Assert.That(obj.sub2.IntProp1, Is.EqualTo(4567));
            Assert.That(obj.sub2.IntProp1ReadOnly, Is.EqualTo(999));

            obj.sub1.IntField1 = 9999;
            obj.sub1.IntProp1Ignore = 555;
            obj.sub2.IntProp1Ignore = 666;

            JsonParser.Parse(json).FillObject(obj);

            Assert.That(obj.sub1.IntField1, Is.EqualTo(123));
            Assert.That(obj.sub1.IntProp1Ignore, Is.EqualTo(555));
            Assert.That(obj.sub2.IntProp1Ignore, Is.EqualTo(666));

        }
    }
}