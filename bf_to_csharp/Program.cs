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
            var systemObjectRef = ResolveType(assemblies, assemblyDefinition, "System.Object");
            var voidTypeRef = ResolveType(assemblies, assemblyDefinition, "System.Void");
            var byteTypeRef = ResolveType(assemblies, assemblyDefinition, "System.Byte");
            //var intTypeRef = ResolveType(assemblies, assemblyDefinition, "System.Int32");

            var mainModule = assemblyDefinition.MainModule;

            var type = new TypeDefinition("", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, systemObjectRef);
            mainModule.Types.Add(type);

            var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, voidTypeRef);
            type.Methods.Add(main);

            main.Body.InitLocals = true;
            main.Body.Variables.Add(new VariableDefinition(mainModule.TypeSystem.Int32));  //tape
            main.Body.Variables.Add(new VariableDefinition(mainModule.TypeSystem.Int32));  //dataPointer

            var il = main.Body.GetILProcessor();

            // byte[] array = new byte[10000];
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldc_I4, 10000);
            il.Emit(OpCodes.Newarr, byteTypeRef);
            il.Emit(OpCodes.Stloc_0);

            // int dataPointer = 5000;
            il.Emit(OpCodes.Ldc_I4, 5000);
            il.Emit(OpCodes.Stloc_1);


            foreach (var instruction in rootBlock.Instructions)
            {
                switch (instruction)
                {
                    case Block block:
                        break;

                    case WriteToConsole w:
                        break;

                    case ReadFromConsole r:
                        break;

                    case Move move:
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldc_I4, move.Quantity);
                        il.Emit(OpCodes.Add);
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
                        break;

                    case Increase increase:
                        break;

                    default:
                        break;
                }
            }

            //    // >
            //    il.Emit(OpCodes.Ldloc_1);
            //il.Emit(OpCodes.Ldc_I4_1);
            //il.Emit(OpCodes.Add);
            //il.Emit(OpCodes.Stloc_1);

            //// <
            //il.Emit(OpCodes.Ldloc_1);
            //il.Emit(OpCodes.Ldc_I4_1);
            //il.Emit(OpCodes.Sub);
            //il.Emit(OpCodes.Stloc_1);


            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldstr, "Hello, Mate!");
            il.Emit(OpCodes.Call, writeLine_String);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);

            assemblyDefinition.EntryPoint = main;
            assemblyDefinition.Write(fileName);
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
