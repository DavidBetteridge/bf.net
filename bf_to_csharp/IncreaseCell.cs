using System.Text;

namespace bf_to_csharp
{
    class IncreaseCell : IInstruction
    {
        public IncreaseCell(int quantity)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            if (Quantity > 0)
                sb.AppendLine(new string('\t', indents) + $"tape[dataPointer]+={Quantity};");
            else if (Quantity < 0)
                sb.AppendLine(new string('\t', indents) + $"tape[dataPointer]-={-Quantity};");
        }
    }

}
