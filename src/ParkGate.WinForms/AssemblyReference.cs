using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ParkGate.ArchitectureTests")]

namespace ParkGate.WinForms;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
