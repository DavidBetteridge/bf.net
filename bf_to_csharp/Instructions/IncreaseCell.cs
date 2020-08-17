using System.Text;

namespace bf
{
    class IncreaseCell : IInstruction
    {
        public IncreaseCell(int quantity)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }

    }

}
