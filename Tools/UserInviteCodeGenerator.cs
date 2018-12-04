using System;
using System.Linq;

namespace Tools
{
    public static class UserInviteCodeGenerator
    {
        public const int LENGTH = 16;
        public static Random RANDOM = new Random();
        const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate() => new string(Enumerable.Repeat(ALPHABET, LENGTH).Select(s => s[RANDOM.Next(s.Length)]).ToArray());
    }
}
