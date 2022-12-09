using System;
using System.Threading.Tasks;

namespace Kenet.SimpleProcess.Dummy;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0) {
            return;
        }

        if (args[0] == "write") {
            Write();
        } else if (args[0] == "sleep") {
            await Task.Delay(-1);
        } else if (args[0] == "error") {
            Error();
        }
    }

    private static void Write()
    {
        Console.WriteLine("");

        Console.Write("");
        Console.Write("\0");

        foreach (var letter in "Hello") {
            Console.Write(letter);
        }
    }

    private static void Error()
    {
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Error.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        Console.Out.WriteLine("test f dsfsdfs fsd fsd fsdf sd fsd test f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsdtest f dsfsdfs fsd fsd fsdf sd fsd");
        
    }
}
