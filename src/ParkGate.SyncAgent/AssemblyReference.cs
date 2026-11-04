using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ParkGate.Infrastructure.Tests")]

namespace ParkGate.SyncAgent;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
