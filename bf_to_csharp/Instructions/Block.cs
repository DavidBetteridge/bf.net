using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace bf
{
    class Block : IInstruction
    {
        private readonly List<IInstruction> _instructions = new List<IInstruction>();

        public Block Parent { get; internal set; }
        public Location Location { get; }

        public ReadOnlyCollection<IInstruction> Instructions => _instructions.AsReadOnly();

        public Block(Location location, Block parent = null)
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
