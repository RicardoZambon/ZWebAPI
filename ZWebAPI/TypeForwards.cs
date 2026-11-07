using System.Runtime.CompilerServices;

// IExceptionReporter moved to ZDatabase so a core library can implement it without referencing
// ASP.NET. The forward keeps assemblies compiled against ZWebAPI 1.16 and 1.17 working.
[assembly: TypeForwardedTo(typeof(ZDatabase.Services.Interfaces.IExceptionReporter))]
