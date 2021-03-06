﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bf
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Please supply folder,  target and reference parameters");
                return;
            }

            var fileToCreate = string.Empty;
            var references = new List<string>();
            var projectFolder = string.Empty;
            var releaseMode = true;

            for (int argumentNumber = 0; argumentNumber < args.Length; argumentNumber++)
            {
                if (args[argumentNumber] == "/o")
                {
                    argumentNumber++;
                    fileToCreate = args[argumentNumber];
                }
                else if (args[argumentNumber] == "/r")
                {
                    argumentNumber++;
                    references.Add(args[argumentNumber]);
                }
                else if (args[argumentNumber] == "-c" || args[argumentNumber] == "--configuration")
                {
                    argumentNumber++;
                    releaseMode = args[argumentNumber].Equals("release", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    projectFolder = args[argumentNumber];
                }
            }


            var sourceCodeFile = Path.Combine(projectFolder, "main.bf");
            

            if (!File.Exists(sourceCodeFile))
            {
                Console.WriteLine($"The source code file {sourceCodeFile} does not exist.");
                return;
            }

            if (releaseMode)
                Console.WriteLine("Building in release mode");
            else
                Console.WriteLine("Building in debug mode");

            var projectName = Path.GetFileNameWithoutExtension(fileToCreate);

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

            ILGenerator.Emit(projectName, fileToCreate, rootBlock, releaseMode, references, sourceCodeFile);

        }

        private static Block Lower(Block originalBlock)
        {
            var newBlock = new Block(originalBlock.Location);
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

                        newBlock.Add(new Label(instruction.Location, $"topOfLoop{labelNumber}"));
                        newBlock.Add(new ConditionalJump(instruction.Location, $"endOfLoop{labelNumber}"));
                        LowerBlock(block, newBlock);
                        newBlock.Add(new Jump(instruction.Location, $"topOfLoop{labelNumber}"));
                        newBlock.Add(new Label(instruction.Location, $"endOfLoop{labelNumber}"));
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
                Console.WriteLine($"Error at position {error.Location.StartColumn} - {error.Description}");
            }
            Console.ResetColor();
        }



        private static Block ParseSourceCode(string sourceCode, List<Error> errors)
        {
            var rootBlock = new Block(new Location(0));
            var currentBlock = rootBlock;
            var location = 1;
            foreach (var instruction in sourceCode)
            {
                switch (instruction)
                {
                    case '>':
                        currentBlock.Add(new MoveRight(new Location(location)));
                        break;
                    case '<':
                        currentBlock.Add(new MoveLeft(new Location(location)));
                        break;
                    case '+':
                        currentBlock.Add(new Increase(new Location(location)));
                        break;
                    case '-':
                        currentBlock.Add(new Decrease(new Location(location)));
                        break;
                    case '.':
                        currentBlock.Add(new WriteToConsole(new Location(location)));
                        break;
                    case ',':
                        currentBlock.Add(new ReadFromConsole(new Location(location)));
                        break;
                    case '[':
                        var newBlock = new Block(new Location(location), currentBlock);
                        currentBlock.Add(newBlock);
                        currentBlock = newBlock;
                        break;
                    case ']':
                        if (currentBlock.Parent is null)
                        {
                            errors.Add(new Error(new Location(location), "] does not have a matching ["));
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
