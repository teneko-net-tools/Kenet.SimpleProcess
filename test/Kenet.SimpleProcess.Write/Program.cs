using System;

namespace Kenet.SimpleProcess.Write;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("");

        Console.Write("");
        Console.Write("\0");

        foreach (var letter in "Hello")
        {
            Console.Write(letter);
        }
    }
}