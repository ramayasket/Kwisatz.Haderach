using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kw.Aspects")]
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyDescription("All purpose aspects")]
[assembly: Guid("00000000-0002-11e8-9876-00055d74a52d")]

/*
    New version scheme as of 1.5.2020:

    Concept.Product.Classes.Minor

    Concept:    conceptual/strategic changes
    Product:    product-wide changes
    Classes:    new/changed classes or APIs
    Minor:      minor changes

    Kw.Aspects change history
    ===============================================================================
    Date        Version        Comments
    ===============================================================================
    04.05.2019    1.0.2.5        PostSharp 6.2.4-rc
    24.05.2019    1.0.2.6        PostSharp 6.2.6 + GetTotalMemoryAttribute
    16.06.2019    1.0.2.7        SingleInstanceAttribute (error)
    23.06.2019    1.0.2.8        SingleInstanceAttribute + CatchReturnAttribute (obsolete)
    23.06.2019    1.0.2.9        GuardedAttribute
    26.06.2019    1.0.2.11       GuardedAttribute + handler
    09.07.2019    1.0.2.13       InterceptedAttribute + Interceptors + consistent attribute names
    10.07.2019    1.0.2.14       Removed debug writelines ))
    12.07.2019    1.0.2.15       WhenNonNullAttribute + interceptor
    15.11.2019    1.0.3.0        Kw.Common 1.0.3
    28.06.2020    1.3.0.0        Kw.Common 1.3.0 + .SNK signing
    02.09.2020    2.0.0.0        Kwisatz.Haderach 2.0
    14.06.2021    2.1.0.0        C# 9.0, PostSharp 6.9.6
*/
