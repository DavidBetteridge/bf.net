using System.Text;

namespace bf_to_csharp
{
    class Decrease : IInstruction
    {
        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "tape[dataPointer]=(byte)(tape[dataPointer]-1);");
        }
    }

}
