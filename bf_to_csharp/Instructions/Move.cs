using System.Text;

namespace bf
{
    class Move : IInstruction
    {
        public Move(int quantity)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }

    }

}
