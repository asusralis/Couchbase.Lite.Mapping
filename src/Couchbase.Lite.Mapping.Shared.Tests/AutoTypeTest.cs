using System;
using Xunit;
using Couchbase.Lite.Mapping.Tests.TestObjects;

namespace Couchbase.Lite.Mapping.Tests
{
    public class AutoTypeTest
    {
        [Fact]
        public void TestAutoTyping()
        {
            SimpleObject obj = new SimpleObject()
            {
                StringValue = "Hey"
            };

            Document doc = obj.ToMutableDocument();

            object newObject = doc.ToObject();
        }

        [Fact]
        public void TestFallbackType()
        {
            SimpleObject obj = new SimpleObject()
            {
                StringValue = "Hey"
            };

            MutableDocument doc = obj.ToMutableDocument();
            doc.SetString("$type", "not a type");

            object newObject = doc.ToObject(typeof(SimpleObject));
        }
    }
}
