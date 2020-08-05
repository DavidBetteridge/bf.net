using System;
using System.Collections.Generic;

namespace HelloWorld
{
    class Program
    {
        static void Main()
        {
            var tape = new byte[10000];
            var dataPointer = 5000;
			tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
			dataPointer+=1;
			tape[dataPointer] = (byte)Console.ReadKey().KeyChar;
			Console.Write((char)tape[dataPointer]);
			loopStart_0:
			if (tape[dataPointer] == 0) goto loopEnd_0;
			tape[dataPointer]-=1;
			goto loopStart_0;
			loopEnd_0:
			dataPointer-=1;
			Console.Write((char)tape[dataPointer]);
			loopStart_1:
			if (tape[dataPointer] == 0) goto loopEnd_1;
			tape[dataPointer]-=1;
			goto loopStart_1;
			loopEnd_1:
			tape[dataPointer]+=8;
			loopStart_2:
			if (tape[dataPointer] == 0) goto loopEnd_2;
			dataPointer+=1;
			tape[dataPointer]+=4;
			loopStart_3:
			if (tape[dataPointer] == 0) goto loopEnd_3;
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
			goto loopStart_3;
			loopEnd_3:
			dataPointer+=1;
			tape[dataPointer]+=1;
			dataPointer+=1;
			tape[dataPointer]+=1;
			dataPointer+=1;
			tape[dataPointer]-=1;
			dataPointer+=2;
			tape[dataPointer]+=1;
			loopStart_4:
			if (tape[dataPointer] == 0) goto loopEnd_4;
			dataPointer-=1;
			goto loopStart_4;
			loopEnd_4:
			dataPointer-=1;
			tape[dataPointer]-=1;
			goto loopStart_2;
			loopEnd_2:
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