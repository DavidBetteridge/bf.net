using System.Text;

namespace bf
{
    class Move : IInstruction
    {
        public Move(Location location, int quantity)
        {
            Location = location;
            Quantity = quantity;
        }

        public int Quantity { get; }
        public Location Location { get; }

    }

}
