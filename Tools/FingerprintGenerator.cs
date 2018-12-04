using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    public static class FingerprintGenerator
    {
        public const int LENGTH = 32;
        public static Random RANDOM = new Random();
        const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate() => new string(Enumerable.Repeat(ALPHABET, LENGTH).Select(s => s[RANDOM.Next(s.Length)]).ToArray());
    }

}
