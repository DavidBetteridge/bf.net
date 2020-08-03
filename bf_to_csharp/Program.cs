using System;
using System.IO;
using System.Text;

namespace bf_to_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var instructions = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
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

            File.WriteAllText(@"C:\personal\bf.net\bf_to_csharp\generated\program.cs", Header() + sb.ToString() + Footer());

            GenerateProjectFile(@"C:\personal\bf.net\bf_to_csharp\generated\generated.csproj");

            void EmitCSharp(string code)
            {
                sb.AppendLine(new string('\t',indent) + code);
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

        private static string Header() =>
            @"using System;
using System.Collections.Generic;

namespace bfi
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
