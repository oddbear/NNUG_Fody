using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Fody;
using Mono.Cecil.Rocks;

public class ModuleWeaver: BaseModuleWeaver
{
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
        yield return "System";
    }

    public override bool ShouldCleanReference => true;

    private TypeDefinition _consoleTypeDefinition;
    private MethodReference _beepMethodReference;

    public override void Execute()
    {
        var mscorlibAssembly = base.ResolveAssembly("mscorlib");
        _consoleTypeDefinition = mscorlibAssembly.MainModule.GetType("System.Console");

        var beepMethdoDefinition = _consoleTypeDefinition?.GetMethods()
            .Where(m => m.Name  == "Beep")
            .Where(m => m.Parameters.Count == 2)
            .SingleOrDefault(m => m.Name == "Beep");

        if(beepMethdoDefinition == null)
            return;

        _beepMethodReference = base.ModuleDefinition.ImportReference(beepMethdoDefinition);

        foreach (var type in ModuleDefinition.GetTypes())
        {
            if (type.IsInterface || type.IsEnum)
                continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody || method.IsConstructor)
                    continue;
                
                ProcessMethod(method);
            }
        }
    }

    void ProcessMethod(MethodDefinition method)
    {
        //If you inject too much instructions, it might overflow (therefor SimplifyMacros):
        //https://stackoverflow.com/questions/7267480/does-mono-cecil-take-care-of-branches-etc-location
        method.Body.SimplifyMacros();

        var instructions = method.Body.Instructions;
        instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4, 5000));
        instructions.Insert(1, Instruction.Create(OpCodes.Ldc_I4, 1000));
        instructions.Insert(2, Instruction.Create(OpCodes.Call, _beepMethodReference));

        //OptimizeMacros after we're done:
        method.Body.OptimizeMacros();
    }
}