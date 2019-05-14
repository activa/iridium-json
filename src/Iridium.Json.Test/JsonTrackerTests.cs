using System.Collections.Generic;
using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonTrackerTests
    {
        [Test]
        public void Test1()
        {
            var obj = new
            {
                int1 = 123,
                string1 = "ABC",
                intArr1 = new [] {1,2,3},
                obj1 = new
                {
                    x = 1,
                    y = "y"
                }
            };

            var json = JsonParser.Parse(JsonSerializer.ToJson(obj), enableTracking:true);

            Assert.That(json["int1"].Path, Is.EqualTo("int1"));
            Assert.That(json["string1"].Path, Is.EqualTo("string1"));
            Assert.That(json["intArr1"].Path, Is.EqualTo("intArr1"));
            Assert.That(json["intArr1"][0].Path, Is.EqualTo("intArr1[0]"));
            Assert.That(json["obj1"]["x"].Path, Is.EqualTo("obj1.x"));
        }

        [Test]
        public void TestNotify()
        {
            var obj = new
            {
                int1 = 123,
                string1 = "ABC",
                intArr1 = new [] {1,2,3},
                obj1 = new
                {
                    x = 1,
                    y = "y"
                }
            };

            var json = JsonParser.Parse(JsonSerializer.ToJson(obj));

            json.AddTracking();

            HashSet<string> valuesTriggered = new HashSet<string>();

            json["int1"].PropertyChanged += (sender, args) =>
            {
                valuesTriggered.Add("int1");
            };

            Assert.That(valuesTriggered.Contains("int1"), Is.False);

            json["int1"] = 1;

            Assert.That(valuesTriggered.Contains("int1"), Is.True);
        }


        [Test]
        public void TestParentForObject()
        {
            string jsonText = @"{""x"":1}";

            var json = JsonParser.Parse(jsonText, true);

            Assert.That(json.TrackingInfo, Is.Not.Null);
            Assert.That(json.TrackingInfo.ParentObject, Is.Null);

            Assert.That(json["x"].TrackingInfo.ParentObject, Is.SameAs(json));
            Assert.That(json["x"].TrackingInfo.ParentKey, Is.EqualTo("x"));
            Assert.That(json["x"].TrackingInfo.ParentIndex, Is.Null);

            json = JsonParser.Parse(jsonText, false);

            Assert.That(json.TrackingInfo, Is.Null);
            Assert.That(json["x"].TrackingInfo, Is.Null);
        }

        [Test]
        public void TestParentForArray()
        {
            string jsonText = @"{""x"":[1,2,3]}";

            var json = JsonParser.Parse(jsonText, true);
            
            Assert.That(json.TrackingInfo, Is.Not.Null);
            Assert.That(json.TrackingInfo.ParentObject, Is.Null);

            var jX = json["x"];

            Assert.That(jX.TrackingInfo.ParentObject, Is.SameAs(json));
            Assert.That(jX.TrackingInfo.ParentKey, Is.EqualTo("x"));
            Assert.That(jX.TrackingInfo.ParentIndex, Is.Null);
            
            Assert.That(jX[0].TrackingInfo.ParentObject, Is.SameAs(jX));
            Assert.That(jX[0].TrackingInfo.ParentIndex, Is.EqualTo(0));
            Assert.That(jX[1].TrackingInfo.ParentObject, Is.SameAs(jX));
            Assert.That(jX[1].TrackingInfo.ParentIndex, Is.EqualTo(1));
            Assert.That(jX[2].TrackingInfo.ParentObject, Is.SameAs(jX));
            Assert.That(jX[2].TrackingInfo.ParentIndex, Is.EqualTo(2));
        }

    }
}