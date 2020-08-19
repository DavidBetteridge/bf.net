using System.Text;

namespace bf
{
    class Increase : IInstruction
    {
        public Increase(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }

}
