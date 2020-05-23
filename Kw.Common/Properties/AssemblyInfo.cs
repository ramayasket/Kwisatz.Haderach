using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Kw.Common;

[assembly: AssemblyTitle("Kw.Common")]
[assembly: AssemblyVersion("1.2.2.0")]
[assembly: AssemblyDescription("All purpose shared classes")]
[assembly: Guid("00000000-0001-11e8-9876-00055d74a52d")]

//
//	This assembly is subject to constitution checks.
//
[assembly: Constitution]

/// <summary>
/// Module initialization.
/// </summary>
// ReSharper disable once InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
internal class __init
{
#if DEBUG
    public const string BUILT_AS = "Debug";
#else
    public const string BUILT_AS = "Release";
#endif 
}
