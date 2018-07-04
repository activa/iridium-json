using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonValueGettersTestFixture
    {
        private JsonObject _json;

        [OneTimeSetUp]
        public void CreateTestJson()
        {
            var obj = new
            {
                int1 = 123,
                string1 = "ABC",
                intArr1 = new [] {1,2,3}
            };

            _json = JsonParser.Parse(JsonSerializer.ToJson(obj));
        }

        [Test]
        public void StringExplicitGeneric()
        {
            string s = _json["string1"].As<string>();

            Assert.That(s, Is.EqualTo("ABC"));
        }

        [Test]
        public void StringExplicitDynamic()
        {
            object s = _json["string1"].As(typeof(string));

            Assert.That(s, Is.EqualTo("ABC"));
        }

        [Test]
        public void StringImplicit()
        {
            string s = _json["string1"];

            Assert.That(s, Is.EqualTo("ABC"));
        }

        [Test]
        public void IntExplicitGeneric()
        {
            int s = _json["int1"].As<int>();

            Assert.That(s, Is.EqualTo(123));
        }

        [Test]
        public void IntExplicitDynamic()
        {
            object s = _json["int1"].As(typeof(int));

            Assert.That(s, Is.EqualTo(123));
        }

        [Test]
        public void IntImplicit()
        {
            int s = _json["int1"];

            Assert.That(s, Is.EqualTo(123));
        }

        [Test]
        public void IntArrayExplicitGeneric()
        {
            int[] arr = _json["intArr1"].As<int[]>();

            Assert.That(arr, Is.EquivalentTo(new[]{1,2,3}));
        }

        [Test]
        public void IntArrayExplicitDynamic()
        {
            object arr = _json["intArr1"].As(typeof(int[]));

            Assert.That(arr, Is.EquivalentTo(new[]{1,2,3}));
        }

        [Test]
        public void IntArrayImplicit()
        {
            int[] arr = _json["intArr1"];

            Assert.That(arr, Is.EquivalentTo(new[]{1,2,3}));
        }


    }
}