using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Fody;

// ReSharper disable once CheckNamespace
public class ModuleWeaver: BaseModuleWeaver
{
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }

    public override bool ShouldCleanReference => true;

    public override void Execute()
    {
        ReplaceMethod("TestClass", "DoWork", "Hello NNUG");
        AddMethod("TestClass", "DoWork2", "Hello NNUG");
    }

    void ReplaceMethod(string className, string methodName, string returnValue)
    {
        //Including TestClass
        var allTypes = ModuleDefinition.GetTypes().ToList();
        var testClass = allTypes.Single(type => type.Name == className);
        var doWorkMethod = testClass.Methods.Single(m => m.Name == methodName);

        //Clear whole body:
        doWorkMethod.Body.Instructions.Clear();

        //Create new body:
        var processor = doWorkMethod.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldstr, returnValue);
        processor.Emit(OpCodes.Ret);
    }

    void AddMethod(string className, string methodName, string returnValue)
    {
        //Including TestClass
        var allTypes = ModuleDefinition.GetTypes().ToList();
        var testClass = allTypes.Single(type => type.Name == className);

        var stringRef = ModuleDefinition.ImportReference(FindType("System.String"));
        var method = new MethodDefinition(methodName, MethodAttributes.Public, stringRef);
        var processor = method.Body.GetILProcessor();

        /* Live code session:
        var exceptionType = typeof(Exception);
        var exceptionCtor = exceptionType.GetConstructor(new Type[] { });
        var exceptionReference = ModuleDefinition.ImportReference(exceptionCtor);
        processor.Emit(OpCodes.Newobj, exceptionReference);
        processor.Emit(OpCodes.Throw);
        */

        processor.Emit(OpCodes.Ldstr, returnValue);
        processor.Emit(OpCodes.Ret);

        testClass.Methods.Add(method);
    }
}