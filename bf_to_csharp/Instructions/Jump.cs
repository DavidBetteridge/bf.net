namespace bf
{
    class Jump : IInstruction
    {
        public Jump(string targetLabelName)
        {
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }


    }

}
