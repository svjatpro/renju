using Renju.CommandLine.Configuration;

namespace Renju.CommandLine;

internal class Program
{
    private static int Main( string[] args )
    {
        try
        {
            var cli = CliParser.Parse( args );
            if ( cli.Help )
            {
                Console.WriteLine( Usage.Text );
                return 0;
            }

            var file = ConfigFileLoader.Load( cli.ConfigPath );
            var config = ConfigResolver.Resolve( file, cli.Config );

            new ConsoleGame( config ).Run();
            return 0;
        }
        catch ( ConfigException e )
        {
            Console.Error.WriteLine( $"error: {e.Message}" );
            Console.Error.WriteLine( "run 'renju --help' for usage" );
            return 1;
        }
    }
}
