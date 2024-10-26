using Rendzu.Core;

namespace Rendzu;

internal class Program
{
    private static void Main( string[] args )
    {
        var playerColor = args.Length > 0 && args[0] == "--white" ? Stone.White : Stone.Black;
        new ConsoleGame( playerColor, 19 ).Run();
    }
}
