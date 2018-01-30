using System;
using Fody;
using Xunit;

#pragma warning disable 618
// ReSharper disable once CheckNamespace
public class WeaverTests
{
    private static readonly TestResult TestResult;

    static WeaverTests()
    {
        var weavingTask = new ModuleWeaver();

        //Modified/Weaved assembly:
        TestResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll");
    }

    [Fact]
    public void ReplaceMethod()
    {
        var type = TestResult.Assembly.GetType("AssemblyToProcess.TestClass");
        var instance = (dynamic)Activator.CreateInstance(type);
        
        Assert.Equal("Hello NNUG", instance.DoWork());
    }

    [Fact]
    public void AddMethod()
    {
        var type = TestResult.Assembly.GetType("AssemblyToProcess.TestClass");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.Equal("Hello NNUG", instance.DoWork2());
    }
}