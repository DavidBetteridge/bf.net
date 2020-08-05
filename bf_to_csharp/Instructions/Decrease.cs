using System.Text;

namespace bf_to_csharp
{
    class Label : IInstruction
    {
        public Label(string labelName)
        {
            LabelName = labelName;
        }

        public string LabelName { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"{LabelName}:");
        }
    }

    class Jump : IInstruction
    {
        public Jump(string targetLabelName)
        {
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"goto {TargetLabelName};");
        }
    }

    class ConditionalJump : IInstruction
    {
        public ConditionalJump(string targetLabelName)
        {
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"if (tape[dataPointer] == 0) goto {TargetLabelName};");
        }
    }

    class Decrease : IInstruction
    {
        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "tape[dataPointer]=(byte)(tape[dataPointer]-1);");
        }
    }

}
