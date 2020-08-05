using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace bf_to_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Please supply the path to the source code file.");
                return;
            }
            var sourceCodeFile = args[0];

            if (!File.Exists(sourceCodeFile))
            {
                Console.WriteLine($"The source code file {sourceCodeFile} does not exist.");
                return;
            }

            var parentFolder = Path.GetDirectoryName(sourceCodeFile);
            var projectName = Path.GetFileNameWithoutExtension(sourceCodeFile);
            var projectFolder = Path.Combine(parentFolder, projectName);

            Directory.CreateDirectory(projectFolder);

            var sourceCode = File.ReadAllText(sourceCodeFile);

            var rootBlock = ParseSourceCode(sourceCode);

            rootBlock = Optimiser.Optimise(rootBlock, null);

            rootBlock = Lower(rootBlock);

            GenerateCSharp(projectName, projectFolder, rootBlock);

            var assemblies = new List<AssemblyDefinition>()
            {
                AssemblyDefinition.ReadAssembly(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Console.dll"),
                AssemblyDefinition.ReadAssembly(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.dll")
            };

            var fileName = Path.Combine(projectFolder, projectName + ".dll");
            var assemblyNameDefinition = new AssemblyNameDefinition(projectName, new Version(1, 0, 0, 0));
            using var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, projectName, ModuleKind.Console);

            var writeLine_String = ResolveMethod(assemblies, assemblyDefinition, "System.Console", "WriteLine", new[] { "System.String" });
            var write_Char = ResolveMethod(assemblies, assemblyDefinition, "System.Console", "Write", new[] { "System.Char" });
            
            var readKey = ResolveMethod(assemblies, assemblyDefinition, "System.Console", "ReadKey", Array.Empty<string>());
            var getKeyChar = ResolveMethod(assemblies, assemblyDefinition, "System.ConsoleKeyInfo", "get_KeyChar", Array.Empty<string>());
            
            var systemObjectRef = ResolveType(assemblies, assemblyDefinition, "System.Object");
            var voidTypeRef = ResolveType(assemblies, assemblyDefinition, "System.Void");
            var byteTypeRef = ResolveType(assemblies, assemblyDefinition, "System.Byte");
            var consoleKeyInfoRef = ResolveType(assemblies, assemblyDefinition, "System.ConsoleKeyInfo");

            var mainModule = assemblyDefinition.MainModule;

            var type = new TypeDefinition("", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, systemObjectRef);
            mainModule.Types.Add(type);

            var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, voidTypeRef);
            type.Methods.Add(main);

            main.Body.InitLocals = true;
            main.Body.Variables.Add(new VariableDefinition(new ArrayType(mainModule.TypeSystem.Byte)  ));  //tape
            main.Body.Variables.Add(new VariableDefinition(mainModule.TypeSystem.Int32));  //dataPointer
            main.Body.Variables.Add(new VariableDefinition(mainModule.TypeSystem.Boolean));  //guard conditional for loop
            main.Body.Variables.Add(new VariableDefinition(consoleKeyInfoRef));  // ConsoleRead

            var il = main.Body.GetILProcessor();

            // byte[] array = new byte[10000];
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldc_I4, 10000);
            il.Emit(OpCodes.Newarr, byteTypeRef);
            il.Emit(OpCodes.Stloc_0);

            // int dataPointer = 5000;
            il.Emit(OpCodes.Ldc_I4, 5000);
            il.Emit(OpCodes.Stloc_1);

            var labels = new Dictionary<string, int>();
            var fixups = new Dictionary<int, string>();
            foreach (var instruction in rootBlock.Instructions)
            {
                //il.Emit(OpCodes.Ldstr, instruction.GetType().Name);
                //il.Emit(OpCodes.Call, writeLine_String);

                switch (instruction)
                {
                    case Label label:
                        labels.Add(label.LabelName, il.Body.Instructions.Count);
                        break;

                    case Jump jump:
                        fixups.Add(il.Body.Instructions.Count, jump.TargetLabel);
                        il.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
                        break;

                    case ConditionalJump jump:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldelem_U1);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Stloc_2);  //for debug only?
                        il.Emit(OpCodes.Ldloc_2);  //for debug only? 

                        fixups.Add(il.Body.Instructions.Count, jump.TargetLabel);
                        il.Emit(OpCodes.Brtrue, Instruction.Create(OpCodes.Nop));

                        break;

                    case Block block:
                        break;

                    case WriteToConsole w:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldelema, byteTypeRef);
                        il.Emit(OpCodes.Ldind_U1);
                        il.Emit(OpCodes.Call, write_Char);
                        break;

                    case ReadFromConsole r:
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Call, readKey);
                        il.Emit(OpCodes.Stloc_3);
                        il.Emit(OpCodes.Ldloca,3);
                        il.Emit(OpCodes.Call, getKeyChar);
                        il.Emit(OpCodes.Conv_U1);
                        il.Emit(OpCodes.Stelem_I1);
                        break;

                    case Move move:
                        il.Emit(OpCodes.Ldloc_1);

                        il.Emit(OpCodes.Ldc_I4, Math.Abs(move.Quantity));
                        if (move.Quantity > 0)
                            il.Emit(OpCodes.Add);
                        else
                            il.Emit(OpCodes.Sub);

                        il.Emit(OpCodes.Stloc_1);
                        break;

                    case MoveLeft moveLeft:
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc_1);
                        break;

                    case MoveRight moveRight:
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc_1);
                        break;

                    case Decrease decrease:
                        il.Emit(OpCodes.Ldloc_0);  // push tape onto the stack
                        il.Emit(OpCodes.Ldloc_1);  // push dataPointer onto the stack
                        il.Emit(OpCodes.Ldelema, byteTypeRef);  // pop,pop and then put the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Dup);           // duplicate the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Ldind_U1);      // pop,  load the value of  tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Ldc_I4_1);      // push 1 onto the stack
                        il.Emit(OpCodes.Sub);           // pop,pop add values and put result onto the stack
                        il.Emit(OpCodes.Conv_U1);       // we added the values as ints,  so cast back into a byte
                        il.Emit(OpCodes.Stind_I1);      // store the value on the head of the stack, into the address also on the stack
                        break;

                    case Increase increase:
                        il.Emit(OpCodes.Ldloc_0);  // push tape onto the stack
                        il.Emit(OpCodes.Ldloc_1);  // push dataPointer onto the stack
                        il.Emit(OpCodes.Ldelema, byteTypeRef);  // pop,pop and then put the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Dup);           // duplicate the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Ldind_U1);      // pop,  load the value of  tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Ldc_I4_1);      // push 1 onto the stack
                        il.Emit(OpCodes.Add);           // pop,pop add values and put result onto the stack
                        il.Emit(OpCodes.Conv_U1);       // we added the values as ints,  so cast back into a byte
                        il.Emit(OpCodes.Stind_I1);      // store the value on the head of the stack, into the address also on the stack
                        break;

                    case IncreaseCell increaseCell:
                        il.Emit(OpCodes.Ldloc_0);  // push tape onto the stack
                        il.Emit(OpCodes.Ldloc_1);  // push dataPointer onto the stack
                        il.Emit(OpCodes.Ldelema, byteTypeRef);  // pop,pop and then put the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Dup);           // duplicate the address of tape[dataPointer] onto the stack
                        il.Emit(OpCodes.Ldind_U1);      // pop,  load the value of  tape[dataPointer] onto the stack

                        il.Emit(OpCodes.Ldc_I4, Math.Abs(increaseCell.Quantity));
                        if (increaseCell.Quantity > 0)
                            il.Emit(OpCodes.Add);
                        else
                            il.Emit(OpCodes.Sub);

                        il.Emit(OpCodes.Conv_U1);       // we added the values as ints,  so cast back into a byte
                        il.Emit(OpCodes.Stind_I1);      // store the value on the head of the stack, into the address also on the stack
                        break;

                    default:
                        break;
                }
            }

            il.Emit(OpCodes.Ret);

            foreach (var toFixup in fixups)
            {
                var labelName = toFixup.Value;
                var jumpTarget = labels[labelName];

                var targetInstruction = il.Body.Instructions[jumpTarget];
                var instructionToFixup = il.Body.Instructions[toFixup.Key];
                instructionToFixup.Operand = targetInstruction;
            }

            assemblyDefinition.EntryPoint = main;
            assemblyDefinition.Write(fileName);
        }

        private static Block Lower(Block originalBlock)
        {
            var newBlock = new Block();
            var labelCount = 0;

            DoWork(originalBlock, newBlock);

            return newBlock;



            void DoWork(Block originalBlock, Block newBlock)
            {
                foreach (var instruction in originalBlock.Instructions)
                {
                    switch (instruction)
                    {
                        case Block block:
                            var labelNumber = labelCount;
                            labelCount++;
                            newBlock.Add(new Label($"loopStart_{labelNumber}"));
                            newBlock.Add(new ConditionalJump($"loopEnd_{labelNumber}"));
                            DoWork(block, newBlock);
                            newBlock.Add(new Jump($"loopStart_{labelNumber}"));
                            newBlock.Add(new Label($"loopEnd_{labelNumber}"));

                            break;

                        default:
                            newBlock.Add(instruction);
                            break;
                    }
                }
            }
        }

        static TypeReference ResolveType(List<AssemblyDefinition> assemblies, AssemblyDefinition assemblyDefinition, string metadataName)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == metadataName)
                                       .ToArray();

            if (foundTypes.Length == 1)
            {
                var typeReference = assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                return typeReference;
            }
            else if (foundTypes.Length == 0)
            {
                throw new Exception($"{metadataName} not found");

            }
            else
            {
                throw new Exception($"{metadataName} ambiguous");
            }
        }

        static MethodReference ResolveMethod(List<AssemblyDefinition> assemblies, AssemblyDefinition assemblyDefinition, string typeName, string methodName, string[] parameterTypeNames)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == typeName)
                                       .ToArray();
            if (foundTypes.Length == 1)
            {
                var foundType = foundTypes[0];
                var methods = foundType.Methods.Where(m => m.Name == methodName);

                foreach (var method in methods)
                {
                    if (method.Parameters.Count != parameterTypeNames.Length)
                        continue;

                    var allParametersMatch = true;

                    for (var i = 0; i < parameterTypeNames.Length; i++)
                    {

                        if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                        {
                            allParametersMatch = false;
                            break;
                        }
                    }

                    if (!allParametersMatch)
                        continue;


                    return assemblyDefinition.MainModule.ImportReference(method);
                }

                throw new Exception($"{methodName} not found");
            }

            else if (foundTypes.Length == 0)
            {
                throw new Exception($"{typeName} not found");
            }
            else
            {
                throw new Exception($"{typeName} is ambiguous");
            }
        }


        private static void GenerateCSharp(string projectName, string projectFolder, Block rootBlock)
        {
            var sb = new StringBuilder();
            rootBlock.EmitCSharp(sb, 3);

            File.WriteAllText(Path.Combine(projectFolder, "program.cs"), Header(projectName) + sb.ToString() + Footer());

            var pathToProjectFile = Path.Combine(projectFolder, projectName + ".csproj");
            GenerateProjectFile(pathToProjectFile);
            var myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "dotnet";
            myProcess.StartInfo.Arguments = $@"build ""{pathToProjectFile}""";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();
        }

        private static Block ParseSourceCode(string sourceCode)
        {
            var rootBlock = new Block();
            var currentBlock = rootBlock;
            foreach (var instruction in sourceCode)
            {
                switch (instruction)
                {
                    case '>':
                        currentBlock.Add(new MoveRight());
                        break;
                    case '<':
                        currentBlock.Add(new MoveLeft());
                        break;
                    case '+':
                        currentBlock.Add(new Increase());
                        break;
                    case '-':
                        currentBlock.Add(new Decrease());
                        break;
                    case '.':
                        currentBlock.Add(new WriteToConsole());
                        break;
                    case ',':
                        currentBlock.Add(new ReadFromConsole());
                        break;
                    case '[':
                        var newBlock = new Block(currentBlock);
                        currentBlock.Add(newBlock);
                        currentBlock = newBlock;
                        break;
                    case ']':
                        currentBlock = currentBlock.Parent;
                        break;
                    default:
                        break;
                }
            }

            return rootBlock;
        }

        private static void GenerateProjectFile(string filename)
        {
            var contents = ReadTemplate("projectFile.txt");

            File.WriteAllText(filename, contents);
        }

        private static string Header(string projectName)
        {
            return ReadTemplate("header.txt")
                        .Replace("{{namespace}}", projectName);
        }

        private static string Footer() => ReadTemplate("footer.txt");

        private static string ReadTemplate(string templateName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "bf_to_csharp.Templates." + templateName;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

    }

}
