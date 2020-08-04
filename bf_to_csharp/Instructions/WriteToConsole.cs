using System.Text;

namespace bf_to_csharp
{
    internal class WriteToConsole : IInstruction
    {
        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "Console.Write((char)tape[dataPointer]);");
        }
    }

}
