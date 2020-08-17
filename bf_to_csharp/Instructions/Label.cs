namespace bf
{
    class Label : IInstruction
    {
        public Label(string labelName)
        {
            LabelName = labelName;
        }

        public string LabelName { get; }


    }

}
