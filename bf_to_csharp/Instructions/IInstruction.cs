using System.Text;

namespace bf_to_csharp
{
    interface IInstruction
    {
        void EmitCSharp(StringBuilder sb, int indents);
        public int Location { get; }
    }

}
