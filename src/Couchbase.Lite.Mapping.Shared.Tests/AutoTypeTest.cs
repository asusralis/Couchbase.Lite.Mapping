using System;
using Xunit;
using System.Collections.Generic;
using Couchbase.Lite.Mapping.Tests.TestObjects;

namespace Couchbase.Lite.Mapping.Tests
{
    public class AutoTypeTest
    {
        class A
        {
            public int X { get; set; }
        }

        class B : A
        {
            public int Y { get; set; }
        }

        class Foo
        {
            public List<A> MyList { get; set; }
        }

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

        [Fact]
        public void TestCollection()
        {
            List<A> items = new List<A>();

            items.Add(new A());
            items.Add(new B());
            items.Add(new B());

            var doc = new Foo() { MyList = items }.ToMutableDocument();

            Foo obj = (Foo)doc.ToObject();


        }
    }
}
