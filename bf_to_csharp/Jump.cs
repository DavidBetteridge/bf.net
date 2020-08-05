using System.Text;

namespace bf_to_csharp
{
    class Jump : IInstruction
    {
        public Jump(string targetLabel)
        {
            TargetLabel = targetLabel;
        }

        public string TargetLabel { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"goto {TargetLabel};");
        }
    }

}
