namespace bf
{
    class MoveLeft : IInstruction
    {
        public MoveLeft(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }

}
