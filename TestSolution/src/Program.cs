﻿using System.Collections.Generic;
using LoquatTech.RequiredPropertyInit;

namespace RequiredPropertyInitTestSolution
{
    class Program
    {
        unsafe static void Main()
        {
            // Test case
            var testClassProperties = new TestClassProperties
            {
                IntProp = 1,
                RecursiveProp = new TestClassProperties
                {
                    BoolProp = false,
                    IntProp = 2
                }
            };

            var testRecordProperties = new TestRecordProperties
            {
                IntProp = 2,
                RecursiveProp =
                {
                    BoolProp = false
                }
            };
            
            // Test case
            var testRequiredClass = new TestRequiredClass
            {
                IntProp = 1,
                RecursiveProp = new TestRecordProperties
                {
                    BoolProp = false,
                    IntProp = 2
                }
            };

            var testRequiredRecord = new TestRequiredRecord
            {
                IntProp = 2,
                RecursiveProp =
                {
                    BoolProp = false
                }
            };

            // Potential issues
            var foo = new Dictionary<int, int> { { 1, 1 } };

            var foo2 = new[] { 1, 2 };

            var foo3 = stackalloc[] { 1, 2, 3 };
        }
    }

    internal class TestClassProperties : TestBaseClassProperties
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRequiredRecord SetTestProp { get; set; }

        public TestClassProperties GetOnlyTestProp { get; }

        public TestRecordProperties UninitTestProp { get; init; }

        [RequiredInit]
        public TestClassProperties RecursiveProp { get; init; }
    }
    internal abstract class TestBaseClassProperties
    {
        [RequiredInit]
        public string StringProp { get; init; }

        [RequiredInit]
        public TestRecordProperties BaseRecordProp { get; init; }
    }

    internal record TestRecordProperties
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRequiredRecord SetTestProp { get; set; }

        public TestClassProperties GetOnlyTestProp { get; }

        public TestRecordProperties UninitTestProp { get; init; }

        [RequiredInit]
        public TestClassProperties RecursiveProp { get; init; }
    }

    [RequiredInit]
    internal class TestRequiredClass
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRequiredRecord SetTestProp { get; set; }

        public TestClassProperties GetOnlyTestProp { get; }

        public TestRecordProperties UninitTestProp { get; init; }

        [RequiredInit]
        public TestRecordProperties RecursiveProp { get; init; }
    }

    [RequiredInit]
    internal record TestRequiredRecord
    {
        [RequiredInit]
        public int IntProp { get; init; }

        public bool BoolProp { get; init; }

        public TestRequiredRecord SetTestProp { get; set; }

        public TestClassProperties GetOnlyTestProp { get; }

        public TestRecordProperties UninitTestProp { get; init; }

        [RequiredInit]
        public TestRecordProperties RecursiveProp { get; init; }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

