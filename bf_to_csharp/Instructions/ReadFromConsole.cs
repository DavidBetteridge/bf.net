namespace bf
{
    class ReadFromConsole : IInstruction
    {
        public ReadFromConsole(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }

}
