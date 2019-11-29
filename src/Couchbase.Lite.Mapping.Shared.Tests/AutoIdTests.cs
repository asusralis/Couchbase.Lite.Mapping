using System;
using Xunit;
using Couchbase.Lite.Mapping.Tests.TestObjects;

namespace Couchbase.Lite.Mapping.Tests
{
    public class AutoIdTests
    {
        [Fact]
        public void TestExisitngId()
        {
            var obj = new SimpleObjectWithId()
            {
                StringValue = "Name!",
                Id = Guid.NewGuid().ToString()
            };

            var doc = obj.ToMutableDocument();

            var newObject = (SimpleObjectWithId)doc.ToObject();
        }

        [Fact]
        public void TestNewId()
        {
            var obj = new SimpleObjectWithId()
            {
                StringValue = "Name!",
                Id = null
            };

            var doc = obj.ToMutableDocument();

            var newObject = (SimpleObjectWithId)doc.ToObject();
        }
    }
}
