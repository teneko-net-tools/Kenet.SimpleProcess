using System.Threading.Tasks;

namespace Kenet.SimpleProcess.Sleep;

internal class Program
{
    public static async Task Main(string[] args)
    {
        await Task.Delay(-1);
    }
}