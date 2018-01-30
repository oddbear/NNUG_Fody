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
    private MethodReference _backgroundColorPropertySet;
    private Random _random;

    public override void Execute()
    {
        _random = new Random();
        var mscorlibAssembly = base.ResolveAssembly("mscorlib");
        _consoleTypeDefinition = mscorlibAssembly.MainModule.GetType("System.Console");
        if(_consoleTypeDefinition == null)
            return;
        
        var backgroundColorProperty = _consoleTypeDefinition.Properties.SingleOrDefault(m => m.Name == "BackgroundColor");
        if(backgroundColorProperty == null)
            return;

        //Important: Must be reference from ModuleDefinition, or:
        // "System.ArgumentException : Member '*' is declared in another module and needs to be imported"
        _backgroundColorPropertySet = base.ModuleDefinition.ImportReference(backgroundColorProperty.SetMethod);
        
        foreach (var type in ModuleDefinition.GetTypes())
        {
            if (type.IsInterface || type.IsEnum)
                continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;

                ProcessMethod(method);
            }
        }
    }

    void ProcessMethod(MethodDefinition method)
    {
        method.Body.SimplifyMacros();
        var instructions = method.Body.Instructions;
        for (var index = 0; index < instructions.Count; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode != OpCodes.Call)
            {
                continue;
            }

            if (!(instruction.Operand is MethodReference methodReference))
            {
                continue;
            }

            if (methodReference.DeclaringType.FullName != "System.Console")
            {
                continue;
            }

            var color = _random.Next(0, 16);
            instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)color));
            instructions.Insert(1, Instruction.Create(OpCodes.Call, _backgroundColorPropertySet));

            break;
        }

        method.Body.OptimizeMacros();
    }
}