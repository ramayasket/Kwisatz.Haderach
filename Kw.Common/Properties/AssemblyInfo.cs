using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kw.Common")]
[assembly: AssemblyVersion("1.3.1.0")]
[assembly: AssemblyDescription("All purpose shared classes")]
[assembly: Guid("00000000-0001-11e8-9876-00055d74a52d")]

/*
    New version scheme as of 1.5.2020:

    Concept.Product.Classes.Minor

    Concept:    conceptual/strategic changes
    Product:    product-wide changes
    Classes:    new classes or APIs
    Minor:        minor changes

    Kw.Common change history
    ===============================================================================
    Date        Version        Comments
    ===============================================================================
    18.05.2020    1.2.1.0        Typed randomizer
    23.05.2020    1.2.2.0        Stream helper
    29.05.2020    1.2.2.1        New icon format
    29.05.2020    1.2.2.2        Old icon format back
    29.05.2020    1.2.2.3        No icon whatsoever
    29.05.2020    1.2.3.0        GZip helper
    24.06.2020    1.3.0.0        signing changed to .SNK
    28.06.2020    1.3.0.0        NetStandard 2.0
    28.06.2020    1.3.1.0        Dynamic -> JDynamic
*/

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
