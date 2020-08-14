﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bf_to_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Please supply the path to the source code file.");
                return;
            }

            var filenameToCreate = string.Empty;
            var references = new List<string>();
            var sourceCodeFile = string.Empty;
            var releaseMode = true;
            for (int argNumber = 0; argNumber < args.Length; argNumber++)
            {
                if (args[argNumber] == "/o")
                {
                    argNumber++;
                    filenameToCreate = args[argNumber];
                }
                else if (args[argNumber] == "/r")
                {
                    argNumber++;
                    references.Add(args[argNumber]);
                }
                else
                {
                    sourceCodeFile = args[argNumber];
                }
            }

            if (!File.Exists(sourceCodeFile))
            {
                Console.WriteLine($"The source code file {sourceCodeFile} does not exist.");
                return;
            }

            var projectName = Path.GetFileNameWithoutExtension(filenameToCreate);
            Console.WriteLine("Building " + projectName + "...");

            var sourceCode = File.ReadAllText(sourceCodeFile);

            var errors = new List<Error>();
            var rootBlock = ParseSourceCode(sourceCode, errors);

            if (errors.Any())
            {
                DisplayErrors(errors);
                return;
            }

            var optimisedCode = Optimiser.Optimise(rootBlock, null);

            CheckForEmptyLoops(optimisedCode, errors);
            if (errors.Any())
            {
                DisplayErrors(errors);
                return;
            }

            rootBlock = releaseMode switch
            {
                true => Lower(optimisedCode),
                false => Lower(rootBlock),
            };

            ILGenerator.Emit(projectName, filenameToCreate, rootBlock, releaseMode, references);
        }

        private static Block Lower(Block originalBlock)
        {
            var newBlock = new Block();
            var nextLabelNumber = 0;

            LowerBlock(originalBlock, newBlock);

            return newBlock;

            void LowerBlock(Block originalBlock, Block newBlock)
            {
                foreach (var instruction in originalBlock.Instructions)
                {
                    if (instruction is Block block)
                    {
                        /* while (tape[dataPointer] != 0)
                         * {
                         *  do stuff
                         * }
                         * 
                         * topOfLoop1:
                         * if (tape[dataPointer] == 0) goto endOfLoop1
                         * do stuff
                         * goto topOfLoop1
                         * endOfLoop1:
                         */
                        var labelNumber = nextLabelNumber;
                        nextLabelNumber++;

                        newBlock.Add(new Label($"topOfLoop{labelNumber}"));
                        newBlock.Add(new ConditionalJump($"endOfLoop{labelNumber}"));
                        LowerBlock(block, newBlock);
                        newBlock.Add(new Jump($"topOfLoop{labelNumber}"));
                        newBlock.Add(new Label($"endOfLoop{labelNumber}"));
                    }
                    newBlock.Add(instruction);
                }
            }
        }

        private static void CheckForEmptyLoops(Block rootBlock, List<Error> errors)
        {
            foreach (var loop in rootBlock.Instructions.OfType<Block>())
            {
                if (loop.Instructions.Any())
                {
                    CheckForEmptyLoops(loop, errors);
                }
                else
                {
                    errors.Add(new Error(loop.Location, "Empty loop - either will do nothing or loop infinitely"));
                }
            }
        }

        private static void DisplayErrors(List<Error> errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors.OrderBy(e => e.Location))
            {
                Console.WriteLine($"Error at position {error.Location} - {error.Description}");
            }
            Console.ResetColor();
        }

        private static Block ParseSourceCode(string sourceCode, List<Error> errors)
        {
            var rootBlock = new Block();
            var currentBlock = rootBlock;
            var location = 1;
            foreach (var instruction in sourceCode)
            {
                switch (instruction)
                {
                    case '>':
                        currentBlock.Add(new MoveRight());
                        break;
                    case '<':
                        currentBlock.Add(new MoveLeft());
                        break;
                    case '+':
                        currentBlock.Add(new Increase());
                        break;
                    case '-':
                        currentBlock.Add(new Decrease());
                        break;
                    case '.':
                        currentBlock.Add(new WriteToConsole());
                        break;
                    case ',':
                        currentBlock.Add(new ReadFromConsole());
                        break;
                    case '[':
                        var newBlock = new Block(currentBlock, location);
                        currentBlock.Add(newBlock);
                        currentBlock = newBlock;
                        break;
                    case ']':
                        if (currentBlock.Parent is null)
                        {
                            errors.Add(new Error(location, "] does not have a matching ["));
                        }
                        else
                        {
                            currentBlock = currentBlock.Parent;
                        }
                        break;
                    default:
                        break;
                }
                location++;
            }

            while (currentBlock.Parent is object)
            {
                errors.Add(new Error(currentBlock.Location, "[ does not have a matching ]"));
                currentBlock = currentBlock.Parent;
            }

            return rootBlock;
        }
    }
}
