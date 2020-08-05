using System.Text;

namespace bf_to_csharp
{
    class ConditionalJump : IInstruction
    {
        public ConditionalJump(string targetLabel)
        {
            TargetLabel = targetLabel;
        }

        public string TargetLabel { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"if (tape[dataPointer] == 0) goto {TargetLabel};");
        }
    }

}
