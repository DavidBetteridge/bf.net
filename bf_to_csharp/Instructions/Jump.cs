namespace bf
{
    class Jump : IInstruction
    {
        public Jump(Location location, string targetLabelName)
        {
            Location = location;
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }

        public Location Location { get; }

    }

}
