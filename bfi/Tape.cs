using System;

namespace bfi
{
    class Tape
    {
        private byte[] _negative = new byte[2];
        private byte[] _positive = new byte[2];

        public byte Read(int address)
        {
            if (address >= 0)
            {
                if (address >= _positive.Length)
                {
                    return 0;
                }
                return _positive[address];
            }
            else
            {
                var index = -address + 1;
                if (index >= _negative.Length)
                {
                    return 0;
                }
                return _negative[index];
            }
        }

        public void Write(int address, byte value)
        {
            if (address >= 0)
            {
                while (address >= _positive.Length)
                {
                    Array.Resize(ref _positive, _positive.Length * 2);
                    // Console.WriteLine($"Postive array resized to {_positive.Length}");
                }

                _positive[address] = value;
            }
            else
            {
                var index = -address + 1;

                while (index >= _negative.Length)
                {
                    Array.Resize(ref _negative, _negative.Length * 2);
                    // Console.WriteLine($"Negative array resized to {_negative.Length}");
                }

                _negative[index] = value;
            }
        }
    }
}
