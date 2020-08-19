using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bf
{
    class ILGenerator
    {
        internal static void Emit(string projectName, string filename, Block code, bool releaseMode, List<string> referencePaths, string fullPathToSourceCode)
        {
            var pathToSystemConsole = referencePaths.SingleOrDefault(path => path.EndsWith("System.Console.dll", StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrWhiteSpace(pathToSystemConsole))
                throw new Exception("Could not find the reference to System.Console.dll");

            var pathToSystemRuntime = referencePaths.SingleOrDefault(path => path.EndsWith("System.Runtime.dll", StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrWhiteSpace(pathToSystemRuntime))
                throw new Exception("Could not find the reference to System.Runtime.dll");

            var systemConsole = AssemblyDefinition.ReadAssembly(pathToSystemConsole);
            var systemConsoleType = FindType(systemConsole, "System.Console");
            var systemConsoleKeyInfoType = FindType(systemConsole, "System.ConsoleKeyInfo");

            var systemRuntime = AssemblyDefinition.ReadAssembly(pathToSystemRuntime);
            var debuggableAttributeType = FindType(systemRuntime, "System.Diagnostics.DebuggableAttribute");


            var assemblyNameDefinition = new AssemblyNameDefinition(projectName, new Version(1, 0, 0));
            using var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, projectName, ModuleKind.Console);
            var module = assemblyDefinition.MainModule;

            var readKeyMethod = FindMethod(module, systemConsoleType, "ReadKey");                      //System.Console.ReadKey()
            var writeLineMethod = FindMethod(module, systemConsoleType, "WriteLine", "System.String"); //System.Console.WriteLine("")
            var writeMethod = FindMethod(module, systemConsoleType, "Write", "System.Char");           //System.Console.Write(' ') 
            var systemConsoleKeyInfoReference = module.ImportReference(systemConsoleKeyInfoType);
            var keyMethod = FindMethod(module, systemConsoleKeyInfoType, "get_KeyChar");               //System.Console.ReadKey().KeyChar

            var debuggableAttributeReference = FindMethod(module, debuggableAttributeType, ".ctor", "System.Boolean", "System.Boolean");


            var programClass = new TypeDefinition("", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, module.TypeSystem.Object);
            module.Types.Add(programClass);

            var mainMethod = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.Void);
            programClass.Methods.Add(mainMethod);
            assemblyDefinition.EntryPoint = mainMethod;

            mainMethod.Body.InitLocals = true;

            var tape = new VariableDefinition(new ArrayType(module.TypeSystem.Byte));
            mainMethod.Body.Variables.Add(tape);       //0 tape

            var dataPointer = new VariableDefinition(module.TypeSystem.Int32);
            mainMethod.Body.Variables.Add(dataPointer);                     //1 pointer
            mainMethod.Body.Variables.Add(new VariableDefinition(systemConsoleKeyInfoReference));               //2
            mainMethod.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Boolean));                   //3 compariasion


            var il = mainMethod.Body.GetILProcessor();

            //new byte[10000]
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldc_I4, 10000);
            il.Emit(OpCodes.Newarr, module.TypeSystem.Byte);
            il.Emit(OpCodes.Stloc_0);

            //datapointer = 5000
            il.Emit(OpCodes.Ldc_I4, 5000);
            il.Emit(OpCodes.Stloc_1);

            var labels = new Dictionary<string, int>();
            var instructionsToFix = new Dictionary<int, string>();

            var document = new Document(fullPathToSourceCode);

            foreach (var instruction in code.Instructions)
            {
                switch (instruction)
                {
                    case ReadFromConsole c:
                        EmitReadFromConsole(readKeyMethod, keyMethod, il, document, instruction);
                        break;

                    case WriteToConsole w:
                        EmitWriteToConsole(module, writeMethod, il, document, instruction);
                        break;

                    case Increase i:
                        EmitIncrease(module, il, +1, document, instruction);
                        break;

                    case Decrease i:
                        EmitIncrease(module, il, -1, document, instruction);
                        break;

                    case IncreaseCell i:
                        EmitIncrease(module, il, i.Quantity, document, instruction);
                        break;

                    case MoveRight m:
                        EmitMove(il, +1, document, instruction);
                        break;

                    case MoveLeft m:
                        EmitMove(il, -1, document, instruction);
                        break;

                    case Move m:
                        EmitMove(il, m.Quantity, document, instruction);
                        break;

                    case Label l:
                        labels.Add(l.LabelName, il.Body.Instructions.Count);
                        break;

                    case Jump j:
                        instructionsToFix.Add(il.Body.Instructions.Count, j.TargetLabelName);
                        il.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
                        break;

                    case ConditionalJump c:
                        EmitConditionalJump(il, instructionsToFix, c.TargetLabelName, document, instruction);
                        break;

                    default:
                        break;
                }

            }

            //il.Emit(OpCodes.Ldstr, "Hello from " + projectName);
            //il.Emit(OpCodes.Call, writeLineMethod);
            il.Emit(OpCodes.Ret);

            foreach (var instructionToFix in instructionsToFix)
            {
                var targetLabelName = instructionToFix.Value;
                var targetLabelLocation = labels[targetLabelName];
                var targetInstruction = il.Body.Instructions[targetLabelLocation];

                var needsFixingLocation = instructionToFix.Key;
                var needFixingInstruction = il.Body.Instructions[needsFixingLocation];

                needFixingInstruction.Operand = targetInstruction;
            }

            if (releaseMode)
            {
                assemblyDefinition.Write(filename);
            }
            else
            {
                var debuggableAttribute = new CustomAttribute(debuggableAttributeReference);
                debuggableAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.Boolean, true));
                debuggableAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.Boolean, true));
                assemblyDefinition.CustomAttributes.Add(debuggableAttribute);

                mainMethod.DebugInformation.Scope = new ScopeDebugInformation(mainMethod.Body.Instructions.First(), mainMethod.Body.Instructions.Last());
                mainMethod.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(tape, "Tape"));
                mainMethod.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(dataPointer, "DataPointer"));

                var symbolsPath = Path.ChangeExtension(filename, ".pdb");
                using var symbolsStream = File.Create(symbolsPath);
                var writerParameters = new WriterParameters
                {
                    WriteSymbols = true,
                    SymbolStream = symbolsStream,
                    SymbolWriterProvider = new PortablePdbWriterProvider()
                };

                assemblyDefinition.Write(filename, writerParameters);
            }
        }

        private static void EmitConditionalJump(ILProcessor il, Dictionary<int, string> instructionsToFix, string targetLabelName, Document document, IInstruction instruction)
        {
            AddSequencePoint(il, document, instruction, il =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_U1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Stloc_3);
                il.Emit(OpCodes.Ldloc_3);

                instructionsToFix.Add(il.Body.Instructions.Count, targetLabelName);
                il.Emit(OpCodes.Brtrue, Instruction.Create(OpCodes.Nop));
            });
        }

        private static void AddSequencePoint(ILProcessor il, Document document, IInstruction instruction, Action<ILProcessor> emit)
        {
            var index = il.Body.Instructions.Count;

            emit(il);

            var sequencePoint = new SequencePoint(il.Body.Instructions[index], document)
            {
                StartLine = 1,
                EndLine = 1,
                StartColumn = instruction.Location.StartColumn,
                EndColumn = instruction.Location.StartColumn + 1,
            };
            il.Body.Method.DebugInformation.SequencePoints.Add(sequencePoint);
        }

        private static void EmitMove(ILProcessor il, int quantity, Document document, IInstruction instruction)
        {
            AddSequencePoint(il, document, instruction, il =>
            {
                il.Emit(OpCodes.Ldloc_1);

                il.Emit(OpCodes.Ldc_I4, Math.Abs(quantity));
                if (quantity > 0)
                    il.Emit(OpCodes.Add);
                else
                    il.Emit(OpCodes.Sub);

                il.Emit(OpCodes.Stloc_1);
            });
        }

        private static void EmitIncrease(ModuleDefinition module, ILProcessor il, int quantity, Document document, IInstruction instruction)
        {
            AddSequencePoint(il, document, instruction, il =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelema, module.TypeSystem.Byte);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldind_U1);

                il.Emit(OpCodes.Ldc_I4, Math.Abs(quantity));
                if (quantity > 0)
                    il.Emit(OpCodes.Add);
                else
                    il.Emit(OpCodes.Sub);

                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stind_I1);
            });
        }

        private static void EmitWriteToConsole(ModuleDefinition module, MethodReference writeMethod, ILProcessor il, Document document, IInstruction instruction)
        {
            AddSequencePoint(il, document, instruction, il =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelema, module.TypeSystem.Byte);
                il.Emit(OpCodes.Ldind_U1);
                il.Emit(OpCodes.Call, writeMethod);
            });
        }

        private static void EmitReadFromConsole(MethodReference readKeyMethod, MethodReference keyMethod, ILProcessor il, Document document, IInstruction instruction)
        {
            AddSequencePoint(il, document, instruction, il =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Call, readKeyMethod);
                il.Emit(OpCodes.Stloc_2);
                il.Emit(OpCodes.Ldloca, 2);
                il.Emit(OpCodes.Call, keyMethod);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stelem_I1);
            });
        }

        private static MethodReference FindMethod(ModuleDefinition module, TypeDefinition typeDefinition, string name, string parameterType1, string parameterType2)
        {
            var method = typeDefinition.Methods.SingleOrDefault(m => m.Name == name && m.Parameters.Count == 2
                                                                && m.Parameters[0].ParameterType.FullName == parameterType1
                                                                && m.Parameters[1].ParameterType.FullName == parameterType2);
            if (method is null) throw new Exception("Could not find the method " + name);

            return module.ImportReference(method);
        }

        private static MethodReference FindMethod(ModuleDefinition module, TypeDefinition typeDefinition, string name, string parameterType)
        {
            var method = typeDefinition.Methods.SingleOrDefault(m => m.Name == name && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == parameterType);
            if (method is null) throw new Exception("Could not find the method " + name);

            return module.ImportReference(method);
        }

        private static MethodReference FindMethod(ModuleDefinition module, TypeDefinition typeDefinition, string name)
        {
            var method = typeDefinition.Methods.SingleOrDefault(m => m.Name == name && m.Parameters.Count == 0);
            if (method is null) throw new Exception("Could not find the method " + name);

            return module.ImportReference(method);
        }

        private static TypeDefinition FindType(AssemblyDefinition assemblyDefinition, string fullName)
        {
            var type = assemblyDefinition.Modules.SelectMany(m => m.Types).SingleOrDefault(t => t.FullName == fullName);
            if (type is null) throw new Exception("Could not find the type " + fullName);

            return type;
        }
    }
}
