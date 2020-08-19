namespace bf
{
    class ConditionalJump : IInstruction
    {
        public ConditionalJump(Location location, string targetLabelName)
        {
            Location = location;
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }

        public Location Location { get; }

    }

}
