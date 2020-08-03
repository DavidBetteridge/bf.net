using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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

            var instructions = File.ReadAllText(sourceCodeFile);
            var sb = new StringBuilder();
            var indent = 3;
            foreach (var instruction in instructions)
            {
                switch (instruction)
                {
                    case '>':
                        EmitCSharp("dataPointer++;");
                        break;
                    case '<':
                        EmitCSharp("dataPointer--;");
                        break;
                    case '+':
                        EmitCSharp("tape[dataPointer]=(byte)(tape[dataPointer]+1);");
                        break;
                    case '-':
                        EmitCSharp("tape[dataPointer]=(byte)(tape[dataPointer]-1);");
                        break;
                    case '.':
                        EmitCSharp("Console.Write((char)tape[dataPointer]);");
                        break;
                    case ',':
                        EmitCSharp("tape[dataPointer] = (byte)Console.ReadKey().KeyChar;");
                        break;
                    case '[':
                        EmitCSharp("while (tape[dataPointer] != 0) {");
                        indent++;
                        break;
                    case ']':
                        indent--;
                        EmitCSharp("}");
                        break;
                    default:
                        break;
                }
            }

            File.WriteAllText(Path.Combine(projectFolder, "program.cs"), Header(projectName) + sb.ToString() + Footer());

            var pathToProjectFile = Path.Combine(projectFolder, projectName + ".csproj");
            GenerateProjectFile(pathToProjectFile);

            using var myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "dotnet";
            myProcess.StartInfo.Arguments = $@"build ""{pathToProjectFile}""";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();

            void EmitCSharp(string code)
            {
                sb.AppendLine(new string('\t', indent) + code);
            }
        }

        private static void GenerateProjectFile(string filename)
        {
            var contents = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>

</Project>
";
            File.WriteAllText(filename, contents);

        }

        private static string Header(string projectName) =>
            @"using System;
using System.Collections.Generic;

namespace " + projectName + @"
{
    class Program
    {
        static void Main(string[] args)
        {
            var tape = new byte[10000];
            var dataPointer = 5000;
";

        private static string Footer() =>
            @"        }
    }
}";

    }

}
