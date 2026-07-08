namespace Renju.CommandLine.Configuration;

/// <summary>Invalid CLI arguments or config file; message is user-facing (printed as "error: ...").</summary>
public class ConfigException( string message ) : Exception( message );
