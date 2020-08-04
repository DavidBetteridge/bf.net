using System.Text;

namespace bf_to_csharp
{
    class MoveRight : IInstruction
    {
        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "dataPointer++;");
        }
    }

}
