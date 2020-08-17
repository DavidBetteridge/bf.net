namespace bf
{
    class ConditionalJump : IInstruction
    {
        public ConditionalJump(string targetLabelName)
        {
            TargetLabelName = targetLabelName;
        }

        public string TargetLabelName { get; }


    }

}
