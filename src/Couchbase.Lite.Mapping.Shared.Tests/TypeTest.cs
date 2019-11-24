using System;
using Xunit;
using Couchbase.Lite.Mapping.Tests.TestObjects;

namespace Couchbase.Lite.Mapping.Tests
{
    public class TypeTest
    {
        class A : IA
        {
            public int X { get; set; } = 3;
        }

        class B : A
        {
            public int Y { get; set; } = 5;
        }

        interface IA
        {

        }


        class ComplexObject
        {
            public IA Foo { get; set; } = new B();
        }

        [Fact]
        public void TestComplexObject()
        {
            var obj = new ComplexObject();

            var doc = obj.ToMutableDocument();

            ComplexObject newObject = (ComplexObject)doc.ToObject();

            Assert.True(newObject.Foo is B);
        }
    }
}
