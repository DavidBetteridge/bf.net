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

}
