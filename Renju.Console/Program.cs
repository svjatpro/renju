using Renju.Core;

namespace Renju.Console;

internal class Program
{
    private static void Main( string[] args )
    {
        var playerColor = args.Length > 0 && args[0] == "--white" ? Stone.White : Stone.Black;
        new ConsoleGame( playerColor, 19 ).Run();
    }
}