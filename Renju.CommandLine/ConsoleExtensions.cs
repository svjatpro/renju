namespace Renju.CommandLine
{
    internal static class ConsoleExtensions
    {
        public static void SetColor( this ColorStyle color )
        {
            Console.BackgroundColor = color.Background;
            Console.ForegroundColor = color.Foreground;
        }
    }
}
