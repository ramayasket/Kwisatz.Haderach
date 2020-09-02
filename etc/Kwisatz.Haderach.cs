using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Andrique")]
[assembly: AssemblyProduct("Kwisatz.Haderach")]
[assembly: AssemblyCopyright("Copyright © Andrei Samoylov 2012-2020")]
[assembly: ComVisible(false)]

/*
    New version scheme as of 1.5.2020:

    Concept.Product.Classes.Minor

    Concept:    conceptual/strategic changes
    Product:    product-wide changes
    Classes:    new/changed classes or APIs
    Minor:      minor changes

    Kwisatz.Haderach change history
    ===============================================================================
    Date        Version        Comments
    ===============================================================================
    24.03.2018    1.0.0.1        Start of versioning
    29.03.2018    1.0.0.2        NuGet packaging preparation
    07.04.2018    1.0.0.3        FrameworkUtils
    15.07.2018    1.0.0.4        Kw.Common as NuGet package
    15.07.2018    1.0.0.5        Removed unneeded dependency
    15.07.2018    1.0.0.6        Changed description & icon
    15.07.2018    1.0.0.7        Guarded
    15.07.2018    1.0.0.8        Execute -> SafeExecute
    15.07.2018    1.0.0.9        WinAPI package
    15.07.2018    1.0.0.10       Package tags
    16.07.2018    1.0.0.12       More WinAPI imports: GetCursorPos, etc.
    20.07.2018    1.0.0.14       More WinAPI imports: SetWindowPos, etc.
    20.07.2018    1.0.0.15       Kw.Windows.Forms as NuGet package
    01.08.2018    1.0.1.0        Signed Kw.Common
    01.08.2018    1.0.1.1        Signed Kw.Common w/o password
    01.08.2018    1.0.1.2        Signed Kw.Common w/password again
    13.11.2018    1.0.1.4        Concat()
    16.04.2019    1.0.1.5        DynamicResources
    19.04.2019    1.0.1.6        NTSTATUS
    22.04.2019    1.0.1.7        NonNullArgument
    23.04.2019    1.0.1.8        Signed Kw.Aspects
    23.04.2019    1.0.2.0        Fixed references
    15.04.2020    1.0.3.0        HTML parser
    01.05.2020    1.1.0.0        New version scheme + AES (Rijndael) crypting
    02.05.2020    1.1.0.1        Rijndael crypting changed from strings to byte[]
    11.05.2020    1.2.0.0        Framework version 4.7 + added Kw.Common.BitStringConverter
    ===============================================================================
    After 1.2.0.0, versions are maintained individually for assemblies
*/
