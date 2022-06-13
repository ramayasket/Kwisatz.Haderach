using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kw.Common")]
[assembly: AssemblyVersion("2.1.7.1")]
[assembly: AssemblyDescription("All purpose shared classes")]
[assembly: Guid("00000000-0001-11e8-9876-00055d74a52d")]

/*
    New version scheme as of 1.5.2020:

    Concept.Product.Classes.Minor

    Concept:    conceptual/strategic changes
    Product:    product-wide changes
    Classes:    new classes or APIs
    Minor:      minor changes

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
    02.09.2020    2.0.0.0        Kwisatz.Haderach 2.0
                                 Added Communications
    14.06.2021    2.1.0.0        C# 9.0
    25.06.2021    2.1.1.0        Qizarate.GetResource
    25.08.2021    2.1.1.1        PrimeAccumulator visible
    25.02.2022    2.1.2.0        SafeTake extension
    15.03.2022    2.1.6.0        SafeSelect extensions
    21.03.2022    2.1.6.1        In, Out nullable args
    25.04.2022    2.1.7.0        ZSpitz + OneOf
    13.06.2022    2.1.7.1        SetFieldValue
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
