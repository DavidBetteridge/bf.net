using System.Text;

namespace bf
{
    class MoveRight : IInstruction
    {
        public MoveRight(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }

}
