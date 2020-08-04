namespace bf_to_csharp
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
