using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using NUnit.Framework;

namespace Iridium.Json.Test
{
    [TestFixture]
    public class JsonTrackerTests
    {
        private JsonObject CreateTestJson()
        {            var obj = new
            {
                int1 = 123,
                string1 = "ABC",
                intArr1 = new [] {1,2,3},
                obj1 = new
                {
                    x = 1,
                    y = "y"
                },
                objArray = new []
                {
                    new { value1 = 1}
                }

            };

            var json = JsonParser.Parse(JsonSerializer.ToJson(obj)).MakeWritable();

            ValidateTracking(json);

            return json;
        }

        [Test]
        public void TestPath()
        {
            var json = CreateTestJson();

            Assert.That(json["int1"].Path, Is.EqualTo("int1"));
            Assert.That(json["string1"].Path, Is.EqualTo("string1"));
            Assert.That(json["intArr1"].Path, Is.EqualTo("intArr1"));
            Assert.That(json["intArr1"][0].Path, Is.EqualTo("intArr1[0]"));
            Assert.That(json["obj1"]["x"].Path, Is.EqualTo("obj1.x"));
        }

        [Test]
        public void TestNotify()
        {
            var json = CreateTestJson();

            json.MakeWritable();

            HashSet<string> valuesTriggered = new HashSet<string>();

            List<JsonObject> jsonChangedTriggers = new List<JsonObject>();

            json.PropertyChanged += (sender, args) =>
            {
                jsonChangedTriggers.Add((JsonObject) sender);
            };

            json["int1"].PropertyChanged += (sender, args) =>
            {
                valuesTriggered.Add("int1");
            };


            Assert.That(valuesTriggered.Contains("int1"), Is.False);
            Assert.That(jsonChangedTriggers.Count, Is.EqualTo(0));

            json["int1"] = 1;

            Assert.That(valuesTriggered.Contains("int1"), Is.True);
            Assert.That(jsonChangedTriggers.Count, Is.EqualTo(1));
            Assert.That(jsonChangedTriggers[0], Is.SameAs(json));
            
            Assert.That(json.FindRoot(), Is.SameAs(json));
            Assert.That(json["int1"].FindRoot(), Is.SameAs(json));
        }

        [Test]
        public void TestNotify2()
        {
            var json = CreateTestJson();

            List<JsonObject> notifications = new List<JsonObject>();

            json.PropertyChanged += (sender, args) =>
            {
                notifications.Add((JsonObject)sender);
            };

            json["obj1.x"] = 2;

            Assert.That(notifications.Count, Is.EqualTo(1));

            int xChangeCount = 0;

            json["obj1.x"].PropertyChanged += (sender, args) => { xChangeCount++; };
            json["obj1.x"] = 3;

            Assert.That(notifications.Count, Is.EqualTo(2));
            Assert.That(xChangeCount, Is.EqualTo(1));

            ValidateTracking(json);
        }

        [Test]
        public void TestModifyAndTrack()
        {
            var json = CreateTestJson();

            ValidateTracking(json);
            
            json["test[0].test1[0].test"] = "x";

            ValidateTracking(json);

            json["intArr1"] = new[] {11, 22, 33};

            ValidateTracking(json);

            json["x"] = new[] {11, 22, 33};

            ValidateTracking(json);

            json["z.y[6]"] = 5;

            ValidateTracking(json);

        }

        [Test]
        public void TestTrackOnNewValue()
        {
            var json = CreateTestJson();

            ValidateTracking(json);

            int changeCount = 0;

            Assert.That(json["objArray"].IsArray);
            Assert.That(json["objArray.xyz"].IsUndefined);

            // Setting an event handler on a field on an array should not affect the array
            json["objArray.xyz"].PropertyChanged += (sender, args) => { changeCount++; };

            Assert.That(json["objArray"].IsArray);
            Assert.That(json["objArray"].AsArray().Length, Is.EqualTo(1));
            Assert.That(json["objArray.xyz"].IsUndefined);

            json = CreateTestJson();

            ValidateTracking(json);

            Assert.That(json["obj1"].IsObject);
            Assert.That(json["obj1[0]"].IsUndefined);

            // Setting an event handler on a field on an array should not affect the array
            json["obj1[0]"].PropertyChanged += (sender, args) => { changeCount++; };

            Assert.That(json["obj1"].IsObject);
            Assert.That(json["obj1[0]"].IsUndefined);
        }


        [Test]
        public void TestTrackOnDeepNewValue1()
        {
            var json = CreateTestJson();

            ValidateTracking(json);

            int changeCountObj2 = 0;
            int changeCountObj3 = 0;

            json["obj1"]["obj2"].PropertyChanged += (s, a) => changeCountObj2++;

            json["obj1.obj2"] = 123;

            Assert.That(json["obj1"]["obj2"].As<int>(), Is.EqualTo(123));
            Assert.That(changeCountObj2, Is.EqualTo(1));

            ValidateTracking(json);
        }

        [Test]
        public void TestTrackOnDeepNewValue2()
        {
            var json = CreateTestJson();

            ValidateTracking(json);

            int changeCountObj2 = 0;
            int changeCountObj3 = 0;

            json["obj1"]["obj2"]["obj3"].PropertyChanged += (s, a) => changeCountObj3++;

            json["obj1.obj2.obj3"] = 123;

            Assert.That(json["obj1.obj2"].IsObject);
            Assert.That(json["obj1"]["obj2"]["obj3"].As<int>(), Is.EqualTo(123));
            Assert.That(changeCountObj3, Is.EqualTo(1));

            ValidateTracking(json);
        }

        [Test]
        public void TestTrackOnDeepNewValue3()
        {
            var json = CreateTestJson();

            ValidateTracking(json);

            int changeCountObj2 = 0;
            int changeCountObj3 = 0;

            var jObj2 = json["obj1"]["obj2"];
            var jObj3 = jObj2["obj3"];

            jObj3.PropertyChanged += (s, a) => changeCountObj3++;

            Assert.That(json["obj1"].TrackingInfo.Temporary, Is.False);
            Assert.That(json["obj1.obj2"].TrackingInfo.Temporary, Is.True);


            jObj2["obj3"] = 123;
            
            Assert.That(json["obj1.obj2"].IsObject);
            Assert.That(jObj2.IsObject);
            Assert.That(json["obj1"]["obj2"]["obj3"].As<int>(), Is.EqualTo(123));
            Assert.That(jObj3.As<int>(), Is.EqualTo(123));
            Assert.That(jObj2["obj3"].As<int>(), Is.EqualTo(123));
            Assert.That(changeCountObj3, Is.EqualTo(1));

            ValidateTracking(json);
        }


        [Test]
        public void TestTrackOnBadType()
        {
            var json = CreateTestJson();

            ValidateTracking(json);

            int changeCount = 0;

            json["xyz"].PropertyChanged += (sender, args) => { changeCount++; };

            Assert.That(json["xyz"].IsUndefined);
            Assert.That(changeCount, Is.Zero);

            json["xyz"] = 123;

            Assert.That(json["xyz"].Value, Is.EqualTo(123));
            Assert.That(changeCount, Is.EqualTo(1));
        }


        [Test]
        public void TestParentForObject()
        {
            string jsonText = @"{""x"":1}";

            var json = JsonParser.Parse(jsonText).MakeWritable();

            Assert.That(json.TrackingInfo, Is.Not.Null);
            Assert.That(json.TrackingInfo.ParentObject, Is.Null);

            Assert.That(json["x"].TrackingInfo.ParentObject, Is.SameAs(json));
            Assert.That(json["x"].TrackingInfo.ParentKey, Is.EqualTo("x"));
            Assert.That(json["x"].TrackingInfo.ParentIndex, Is.Null);

            json = JsonParser.Parse(jsonText);

            Assert.That(json.TrackingInfo, Is.Null);
            Assert.That(json["x"].TrackingInfo, Is.Null);
        }

        [Test]
        public void TestParentForArray()
        {
            string jsonText = @"{""x"":[1,2,3]}";

            var json = JsonParser.Parse(jsonText).MakeWritable();
            
            ValidateTracking(json);

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

        private void ValidateTracking(JsonObject obj)
        {
            Assert.That(obj.TrackingInfo, Is.Not.Null);

            Assert.That(obj.FindRoot(), Is.SameAs(obj));

            void validate(JsonObject o)
            {
                if (!o.TrackingInfo.IsRoot)
                {
                    Assert.That(obj[o.Path], Is.SameAs(o));
                }

                Assert.That(o.FindRoot(), Is.SameAs(obj));

                if (o.IsObject)
                {
                    foreach (var item in o.AsDictionary())
                    {
                        Assert.That(item.Value.TrackingInfo, Is.Not.Null, "Object dictionary item has no tracking info");
                        Assert.That(item.Value.TrackingInfo.ParentObject, Is.SameAs(o));
                        Assert.That(item.Value.TrackingInfo.ParentKey, Is.EqualTo(item.Key));

                        validate(item.Value);
                    }
                }
                else if (o.IsArray)
                {
                    var arr = o.AsArray();

                    for (int i = 0; i < arr.Length; i++)
                    {
                        Assert.That(arr[i].TrackingInfo, Is.Not.Null, "Array item has no tracking info");
                        Assert.That(arr[i].TrackingInfo.ParentObject, Is.SameAs(o));
                        Assert.That(arr[i].TrackingInfo.ParentIndex, Is.EqualTo(i));

                        validate(arr[i]);
                    }
                }
            }

            validate(obj);
        }
    }
}