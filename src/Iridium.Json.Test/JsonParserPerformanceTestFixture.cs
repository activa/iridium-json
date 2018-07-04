using System.Linq;
using NUnit.Framework;

namespace Iridium.Json.Test
{
    /*
    [TestFixture]
    public class JsonParserPerformanceTestFixture
    {
        private string _json;
        private JsonObject _jsonObject;

        [OneTimeSetUp]
        public void CreateTestJson()
        {
            var childObj = new
            {
                string1 = "String 1",
                string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                string3 = "A",
                int1 = 123,
                double1 = 567.0,
                bool1 = true,
                bool2 = false,
                array1 = Enumerable.Range(1,100).ToArray(),
                array2 = Enumerable.Range(1,100).Select(i => "string" + i).ToArray(),
            };

            var o = new
            {
                string1 = "String 1",
                string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                string3 = "A",
                int1 = 123,
                double1 = 567.0,
                bool1 = true,
                bool2 = false,
                array1 = Enumerable.Range(1, 100).ToArray(),
                array2 = Enumerable.Range(1, 100).Select(i => "string" + i).ToArray(),
                array3 = Enumerable.Range(1, 20).Select(i => childObj).ToArray(),
                object1 = new
                {
                    string1 = "String 1",
                    string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                    string3 = "A",
                    object1 = new
                    {
                        string1 = "String 1",
                        string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                        string3 = "A",
                        object1 = new
                        {
                            string1 = "String 1",
                            string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                            string3 = "A",
                            object1 = new
                            {
                                string1 = "String 1",
                                string2 = "A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2, A very long String 2",
                                string3 = "A",
                            }
                        }
                    }
                }
            };

            _json = JsonSerializer.ToJson(o);
            _jsonObject = JsonParser.Parse(_json);
        }

        [Test]
        [Repeat(100)]
        public void PerfTestParse()
        {
            var jsonObject = JsonParser.Parse(_json);
        }

        [Test]
        [Repeat(100)]
        public void PerfTestSerialize()
        {
            var jsonText = JsonSerializer.ToJson(_jsonObject);
        }

        [Test]
        [Repeat(10000)]
        public void PerfTestChainedAccess()
        {
            var value = _jsonObject["object1"]["object1"]["object1"]["string3"].As<string>();
        }

        [Test]
        [Repeat(10000)]
        public void PerfTestIndexed()
        {
            var value = _jsonObject["array3[10].string3"].As<string>();
        }

    }
    */
}