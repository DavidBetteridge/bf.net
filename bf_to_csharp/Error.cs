namespace bf
{
    class Error
    {
        public Error(int location, string description)
        {
            Location = location;
            Description = description;
        }

        public int Location { get; }
        public string Description { get; }
    }

}
