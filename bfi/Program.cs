using System;

namespace bfi
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("bf.Net");
            Console.WriteLine("");

            while (true)
            {
                Console.WriteLine(">");
                var instructions = Console.ReadLine();
                if (instructions == "EXIT") return;


                //var instructions = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
                var tape = new byte[10000];
                var dataPointer = 5000;
                var instructionPointer = 0;

                while (instructionPointer < instructions.Length)
                {
                    var currentInstruction = instructions[instructionPointer];

                    switch (currentInstruction)
                    {
                        case '<':
                            dataPointer--;
                            instructionPointer++;
                            break;
                        case '>':
                            dataPointer++;
                            instructionPointer++;
                            break;
                        case '+':
                            tape[dataPointer] = (byte)(tape[dataPointer] + 1);
                            instructionPointer++;
                            break;
                        case '-':
                            tape[dataPointer] = (byte)(tape[dataPointer] - 1);
                            instructionPointer++;
                            break;
                        case '.':
                            Console.Write((char)tape[dataPointer]);
                            instructionPointer++;
                            break;
                        case ',':
                            tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
                            instructionPointer++;
                            break;
                        case '[':
                            if (tape[dataPointer] == 0)
                            {
                                var openBrackets = 1;
                                while (openBrackets != 0)
                                {
                                    instructionPointer++;
                                    if (instructions[instructionPointer] == '[')
                                    {
                                        openBrackets++;
                                    }
                                    else if (instructions[instructionPointer] == ']')
                                    {
                                        openBrackets--;
                                    }
                                }
                            }
                            instructionPointer++;
                            break;
                        case ']':
                            if (tape[dataPointer] != 0)
                            {
                                var openBrackets = 1;
                                while (openBrackets != 0)
                                {
                                    instructionPointer--;
                                    if (instructions[instructionPointer] == '[')
                                    {
                                        openBrackets--;
                                    }
                                    else if (instructions[instructionPointer] == ']')
                                    {
                                        openBrackets++;
                                    }
                                }
                            }
                            instructionPointer++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
