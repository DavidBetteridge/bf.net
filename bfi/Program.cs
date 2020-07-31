using System;
using System.Collections.Generic;

namespace bfi
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("bf.Net");
            Console.WriteLine("");
            Console.WriteLine("Example Program:");
            Console.WriteLine("++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.");
            Console.WriteLine("");
            Console.WriteLine("Type EXIT to quit.");
            Console.WriteLine("");
            Console.WriteLine("");

            while (true)
            {
                Console.Write(">::  ");
                var instructions = Console.ReadLine();
                if (instructions == "EXIT") return;

                var tape = new Tape();
                var dataPointer = 0;
                var instructionPointer = 0;

                var codeOk = true;
                var openBrackets = new Stack<int>();
                var openToClose = new Dictionary<int, int>();
                var closeToOpen = new Dictionary<int, int>();
                for (int i = 0; i < instructions.Length; i++)
                {
                    var currentInstruction = instructions[i];
                    switch (currentInstruction)
                    {
                        case '[':
                            openBrackets.Push(i);
                            break;
                        case ']':
                            var closeLocation = i;
                            if (openBrackets.Count == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Closing ] bracket at position {i} has no matching open branket [");
                                Console.ResetColor();
                                codeOk = false;
                                break;
                            }

                            var openLocation = openBrackets.Pop();
                            openToClose.Add(openLocation, closeLocation);
                            closeToOpen.Add(closeLocation, openLocation);
                            break;
                    }
                }

                while (openBrackets.Count != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Open [ bracket at position {openBrackets.Pop()} has no matching closing branket ]");
                    Console.ResetColor();
                    codeOk = false;
                }

                if (!codeOk) continue;

                while (instructionPointer < instructions.Length)
                {
                    var currentInstruction = instructions[instructionPointer];

                    switch (currentInstruction)
                    {
                        case '<':
                            dataPointer--;
                            break;
                        case '>':
                            dataPointer++;
                            break;
                        case '+':
                            tape.Write(dataPointer, (byte)(tape.Read(dataPointer) + 1));
                            break;
                        case '-':
                            tape.Write(dataPointer, (byte)(tape.Read(dataPointer) - 1));
                            break;
                        case '.':
                            Console.Write((char)tape.Read(dataPointer));
                            break;
                        case ',':
                            tape.Write(dataPointer, (byte)Console.ReadKey().KeyChar);
                            break;
                        case '[':
                            if (tape.Read(dataPointer) == 0)
                            {
                                instructionPointer = openToClose[instructionPointer];
                            }
                            break;
                        case ']':
                            if (tape.Read(dataPointer) != 0)
                            {
                                instructionPointer = closeToOpen[instructionPointer];
                            }
                            break;
                    }

                    instructionPointer++;

                }
            }
        }
    }
}
