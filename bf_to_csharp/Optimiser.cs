﻿namespace bf_to_csharp
{
    class Optimiser
    {
        public static Block Optimise(Block rootBlock, Block parentBlock)
        {
            var newBlock = new Block(parentBlock);

            var previous = default(IInstruction);
            foreach (var instruction in rootBlock.Instructions)
            {
                switch (instruction)
                {
                    case Block block:
                        if (previous is object) newBlock.Add(previous);
                        newBlock.Add(Optimise(block, newBlock));
                        previous = null;
                        break;

                    case WriteToConsole w:
                    case ReadFromConsole r:
                        if (previous is object) newBlock.Add(previous);
                        newBlock.Add(instruction);
                        previous = null;
                        break;

                    case MoveLeft moveLeft:
                        previous = CombineMoves(newBlock, previous, -1);
                        break;

                    case MoveRight moveRight:
                        previous = CombineMoves(newBlock, previous, +1);
                        break;

                    case Decrease decrease:
                        previous = CombineIncreases(newBlock, previous, -1);
                        break;

                    case Increase increase:
                        previous = CombineIncreases(newBlock, previous, +1);
                        break;

                    default:
                        break;
                }
            }

            if (previous is object) newBlock.Add(previous);

            return newBlock;
        }

        private static IInstruction CombineIncreases(Block newBlock, IInstruction previous, int offset)
        {
            if (previous is IncreaseCell increaseCell)
            {
                previous = new IncreaseCell(increaseCell.Quantity + offset);
            }
            else
            {
                if (previous is object) newBlock.Add(previous);
                previous = new IncreaseCell(offset);
            }

            return previous;
        }

        private static IInstruction CombineMoves(Block newBlock, IInstruction previous, int offset)
        {
            if (previous is Move move)
            {
                previous = new Move(move.Quantity + offset);
            }
            else
            {
                if (previous is object) newBlock.Add(previous);
                previous = new Move(offset);
            }

            return previous;
        }

    }
}