namespace bf
{
    class Label : IInstruction
    {
        public Label(Location location, string labelName)
        {
            Location = location;
            LabelName = labelName;
        }

        public string LabelName { get; }
        public Location Location { get; }


    }

}
