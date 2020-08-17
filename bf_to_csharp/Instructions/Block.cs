using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace bf
{
    class Block : IInstruction
    {
        private List<IInstruction> _instructions = new List<IInstruction>();

        public Block Parent { get; internal set; }
        public int Location { get; }

        public ReadOnlyCollection<IInstruction> Instructions => _instructions.AsReadOnly();

        public Block(Block parent = null, int location = 0)
        {
            Parent = parent;
            Location = location;
        }

        public void Add(IInstruction instruction)
        {
            _instructions.Add(instruction);
        }
       
    }

}
