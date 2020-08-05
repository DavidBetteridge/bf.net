using System;
using System.Collections.Generic;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var tape = new byte[10000];
            var dataPointer = 5000;
			tape[dataPointer]+=8;
			topOfLoop0:
			if (tape[dataPointer] == 0) goto endOfLoop0;
			dataPointer+=1;
			tape[dataPointer]+=4;
			topOfLoop1:
			if (tape[dataPointer] == 0) goto endOfLoop1;
			dataPointer+=1;
			tape[dataPointer]+=2;
			dataPointer+=1;
			tape[dataPointer]+=3;
			dataPointer+=1;
			tape[dataPointer]+=3;
			dataPointer+=1;
			tape[dataPointer]+=1;
			dataPointer-=4;
			tape[dataPointer]-=1;
			goto topOfLoop1;
			endOfLoop1:
			while (tape[dataPointer] != 0) {
				dataPointer+=1;
				tape[dataPointer]+=2;
				dataPointer+=1;
				tape[dataPointer]+=3;
				dataPointer+=1;
				tape[dataPointer]+=3;
				dataPointer+=1;
				tape[dataPointer]+=1;
				dataPointer-=4;
				tape[dataPointer]-=1;
			}
			dataPointer+=1;
			tape[dataPointer]+=1;
			dataPointer+=1;
			tape[dataPointer]+=1;
			dataPointer+=1;
			tape[dataPointer]-=1;
			dataPointer+=2;
			tape[dataPointer]+=1;
			topOfLoop2:
			if (tape[dataPointer] == 0) goto endOfLoop2;
			dataPointer-=1;
			goto topOfLoop2;
			endOfLoop2:
			while (tape[dataPointer] != 0) {
				dataPointer-=1;
			}
			dataPointer-=1;
			tape[dataPointer]-=1;
			goto topOfLoop0;
			endOfLoop0:
			while (tape[dataPointer] != 0) {
				dataPointer+=1;
				tape[dataPointer]+=4;
				while (tape[dataPointer] != 0) {
					dataPointer+=1;
					tape[dataPointer]+=2;
					dataPointer+=1;
					tape[dataPointer]+=3;
					dataPointer+=1;
					tape[dataPointer]+=3;
					dataPointer+=1;
					tape[dataPointer]+=1;
					dataPointer-=4;
					tape[dataPointer]-=1;
				}
				dataPointer+=1;
				tape[dataPointer]+=1;
				dataPointer+=1;
				tape[dataPointer]+=1;
				dataPointer+=1;
				tape[dataPointer]-=1;
				dataPointer+=2;
				tape[dataPointer]+=1;
				while (tape[dataPointer] != 0) {
					dataPointer-=1;
				}
				dataPointer-=1;
				tape[dataPointer]-=1;
			}
			dataPointer+=2;
			Console.Write((char)tape[dataPointer]);
			dataPointer+=1;
			tape[dataPointer]-=3;
			Console.Write((char)tape[dataPointer]);
			tape[dataPointer]+=7;
			Console.Write((char)tape[dataPointer]);
			Console.Write((char)tape[dataPointer]);
			tape[dataPointer]+=3;
			Console.Write((char)tape[dataPointer]);
			dataPointer+=2;
			Console.Write((char)tape[dataPointer]);
			dataPointer-=1;
			tape[dataPointer]-=1;
			Console.Write((char)tape[dataPointer]);
			dataPointer-=1;
			Console.Write((char)tape[dataPointer]);
			tape[dataPointer]+=3;
			Console.Write((char)tape[dataPointer]);
			tape[dataPointer]-=6;
			Console.Write((char)tape[dataPointer]);
			tape[dataPointer]-=8;
			Console.Write((char)tape[dataPointer]);
			dataPointer+=2;
			tape[dataPointer]+=1;
			Console.Write((char)tape[dataPointer]);
			dataPointer+=1;
			tape[dataPointer]+=2;
			Console.Write((char)tape[dataPointer]);
            Console.ReadKey();
        }
    }
}