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
			tape[dataPointer]+=1;
			topOfLoop0:
			if (tape[dataPointer] == 0) goto endOfLoop0;
			tape[dataPointer]-=1;
			goto topOfLoop0;
			endOfLoop0:
			while (tape[dataPointer] != 0) {
				tape[dataPointer]-=1;
			}
			tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
			tape[dataPointer]+=1;
			dataPointer+=1;
			tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
			tape[dataPointer]-=1;
			Console.Write((char)tape[dataPointer]);
			dataPointer-=1;
			Console.Write((char)tape[dataPointer]);
            Console.ReadKey();
        }
    }
}