namespace bf
{
    class Decrease : IInstruction
    {
        public Decrease(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }

}
