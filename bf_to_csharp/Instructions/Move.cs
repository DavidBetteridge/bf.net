using System.Text;

namespace bf_to_csharp
{
    class Move : IInstruction
    {
        public Move(int quantity, int location)
        {
            Quantity = quantity;
            Location = location;
        }

        public int Quantity { get; }
        public int Location { get; }

        public void EmitCSharp(StringBuilder sb, int indents)
        {
            if (Quantity > 0)
                sb.AppendLine(new string('\t', indents) + $"dataPointer+={Quantity};");
            else if (Quantity < 0)
                sb.AppendLine(new string('\t', indents) + $"dataPointer-={-Quantity};");
        }
    }

}
