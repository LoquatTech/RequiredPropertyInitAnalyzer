using LoquatTech;

namespace ClassDefinitionTest
{
    [RequiredInit]
    public class DifferentProjectClass
    {
        [RequiredInit]
        public string StringProp { get; init; }

        [RequiredInit]
        public TestRecordProperties BaseRecordProp { get; init; }
    }

    public record TestRecordProperties
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRecordProperties RecursiveUninitTestProp { get; init; }

        [RequiredInit]
        public TestClassProperties TestClass { get; init; }
    }

    public class TestClassProperties
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRecordProperties UninitTestProp { get; init; }

        [RequiredInit]
        public TestClassProperties RecursiveProp { get; init; }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
