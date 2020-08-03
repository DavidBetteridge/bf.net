using System;
using System.Collections.Generic;

namespace Echo
{
    class Program
    {
        static void Main(string[] args)
        {
            var tape = new byte[10000];
            var dataPointer = 5000;
			tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
			Console.Write((char)tape[dataPointer]);
			Console.Write((char)tape[dataPointer]);
			Console.Write((char)tape[dataPointer]);
			Console.Write((char)tape[dataPointer]);
			Console.Write((char)tape[dataPointer]);
        }
    }
}