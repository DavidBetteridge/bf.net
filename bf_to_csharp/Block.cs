using System.Collections.Generic;
using System.Text;

namespace bf_to_csharp
{
    class Block : IInstruction
    {
        private List<IInstruction> _instructions = new List<IInstruction>();

        public Block Parent { get; internal set; }

        public Block(Block parent = null)
        {
            Parent = parent;
        }

        public void Add(IInstruction instruction)
        {
            _instructions.Add(instruction);
        }
        public void EmitCSharp(StringBuilder sb, int indents)
        {
            if (Parent is object)
            {
                sb.AppendLine(new string('\t', indents) + "while (tape[dataPointer] != 0) {");

                foreach (var instruction in _instructions)
                {
                    instruction.EmitCSharp(sb, indents + 1);
                }

                sb.AppendLine(new string('\t', indents) + "}");
            }
            else
            {
                foreach (var instruction in _instructions)
                {
                    instruction.EmitCSharp(sb, indents);
                }
            }
        }
    }

}
