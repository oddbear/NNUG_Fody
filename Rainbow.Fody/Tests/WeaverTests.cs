using System;
using Fody;
using Xunit;
using Xunit.Abstractions;

public class WeaverTests
{
    static TestResult testResult;

    static WeaverTests()
    {
        var weavingTask = new ModuleWeaver();

        //Modified/Weaved assembly:
        testResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll");
    }

    [Fact]
    public void ConsoleMethod()
    {
        var type = testResult.Assembly.GetType("AssemblyToProcess.TestClass");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.DoWork();
    }
}