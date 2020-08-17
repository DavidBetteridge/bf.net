using System.Text;

namespace bf_to_csharp
{
    class MoveRight : IInstruction
    {
        public MoveRight(int location)
        {
            Location = location;
        }
        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "dataPointer++;");
        }
    }

}
