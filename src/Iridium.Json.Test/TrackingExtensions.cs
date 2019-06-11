using NUnit.Framework;

namespace Iridium.Json.Test
{
    public static class TrackingExtensions
    {
        public static void ValidateReadOnly(this JsonObject obj)
        {
            obj.ValidateTracking(checkReadOnly:true, checkSubObject:false);
        }

        public static void ValidateTracking(this JsonObject obj, bool checkSubObject = false)
        {
            obj.ValidateTracking(false, checkSubObject);
        }

        private static void ValidateTracking(this JsonObject obj, bool checkReadOnly = false, bool checkSubObject = false)
        {
            if (checkReadOnly)
            {
                Assert.That(obj.TrackingInfo, Is.Null);
                Assert.That(obj.FindRoot(), Is.Null);
            }
            else
            {
                Assert.That(obj.TrackingInfo, Is.Not.Null);
                
                if (!checkSubObject)
                    Assert.That(object.ReferenceEquals(obj.FindRoot(), obj));
            }

            void validate(JsonObject o)
            {
                if (checkReadOnly)
                {
                    Assert.That(o.TrackingInfo, Is.Null, "Found tracking info on dictionary item");
                }
                else
                {
                    if (!checkSubObject)
                    {
                        if (!o.TrackingInfo.IsRoot)
                        {
                            Assert.That(object.ReferenceEquals(obj[o.Path], o));
                        }

                        Assert.That(object.ReferenceEquals(o.FindRoot(), obj));
                    }
                }

                if (o.IsObject)
                {
                    foreach (var item in o.AsDictionary())
                    {
                        if (!checkReadOnly)
                        {
                            Assert.That(item.Value.TrackingInfo, Is.Not.Null, "Object dictionary item has no tracking info");
                            Assert.That(item.Value.TrackingInfo.ParentObject, Is.SameAs(o));
                            Assert.That(item.Value.TrackingInfo.ParentKey, Is.EqualTo(item.Key));
                        }

                        validate(item.Value);
                    }
                }
                else if (o.IsArray)
                {
                    var arr = o.AsArray();

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!checkReadOnly)
                        {
                            Assert.That(arr[i].TrackingInfo, Is.Not.Null, "Array item has no tracking info");
                            Assert.That(arr[i].TrackingInfo.ParentObject, Is.SameAs(o));
                            Assert.That(arr[i].TrackingInfo.ParentIndex, Is.EqualTo(i));
                        }

                        validate(arr[i]);
                    }
                }
            }

            validate(obj);
        }
    }
}