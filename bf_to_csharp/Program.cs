using System;
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

            var fileName = Path.Combine(projectFolder, projectName + ".exe");
            var assemblyNameDefinition = new AssemblyNameDefinition(projectName, new Version(1, 0, 0, 0));
            using (var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, projectName, ModuleKind.Console))
            {
                var mainModule = assemblyDefinition.MainModule;

                var pathToSystemConsole = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Console.dll";
                var systemConsoleAssembly = AssemblyDefinition.ReadAssembly(pathToSystemConsole);

                var pathToSystemRuntime = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.dll";
                var systemRuntimeAssembly = AssemblyDefinition.ReadAssembly(pathToSystemRuntime);

                var systemObjectType = systemRuntimeAssembly.Modules
                                      .SelectMany(m => m.Types)
                                      .Where(t => t.FullName == "System.Object")
                                      .SingleOrDefault();
                var systemObjectRef = assemblyDefinition.MainModule.ImportReference(systemObjectType);

                var voidType = systemRuntimeAssembly.Modules
                      .SelectMany(m => m.Types)
                      .Where(t => t.FullName == "System.Void")
                      .SingleOrDefault();
                var voidTypeRef = assemblyDefinition.MainModule.ImportReference(voidType);

                var systemConsoleType = systemConsoleAssembly.Modules
                                                      .SelectMany(m => m.Types)
                                                      .Where(t => t.FullName == "System.Console")
                                                      .SingleOrDefault();
                assemblyDefinition.MainModule.ImportReference(systemConsoleType);
                var writeLineMethod = systemConsoleType.Methods.SingleOrDefault(m => m.Name == "WriteLine" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.String");
                var writeLine = assemblyDefinition.MainModule.ImportReference(writeLineMethod);

                var type = new TypeDefinition("app", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, systemObjectRef)
                {
                    IsBeforeFieldInit = true
                };
                var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, voidTypeRef);

               // var writeLine = mainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

                var il = main.Body.GetILProcessor();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldstr, "Hello, Mate!");
                il.Emit(OpCodes.Call, writeLine);
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(main);
                mainModule.Types.Add(type);


                mainModule.EntryPoint = main;

                assemblyDefinition.EntryPoint = main;
                assemblyDefinition.Write(fileName);
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
