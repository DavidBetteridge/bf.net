using System.Text;

namespace bf_to_csharp
{
    class Label : IInstruction
    {
        public Label(string labelName, int location)
        {
            LabelName = labelName;
            Location = location;
        }

        public string LabelName { get; }
        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"{LabelName}:");
        }
    }

    class Jump : IInstruction
    {
        public Jump(string targetLabelName, int location)
        {
            TargetLabelName = targetLabelName;
            Location = location;
        }

        public string TargetLabelName { get; }
        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"goto {TargetLabelName};");
        }
    }

    class ConditionalJump : IInstruction
    {
        public ConditionalJump(string targetLabelName, int location)
        {
            TargetLabelName = targetLabelName;
            Location = location;
        }

        public string TargetLabelName { get; }
        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + $"if (tape[dataPointer] == 0) goto {TargetLabelName};");
        }
    }

    class Decrease : IInstruction
    {
        public Decrease(int location)
        {
            Location = location;
        }

        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            sb.AppendLine(new string('\t', indents) + "tape[dataPointer]=(byte)(tape[dataPointer]-1);");
        }
    }

}
