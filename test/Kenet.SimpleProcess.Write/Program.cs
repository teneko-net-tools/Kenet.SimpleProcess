using System;
using System.Threading.Tasks;

namespace Kenet.SimpleProcess.Write;

internal class Program
{
    public static async Task Main(string[] args)
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