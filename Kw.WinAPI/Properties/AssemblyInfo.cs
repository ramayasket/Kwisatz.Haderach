using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kw.WinAPI")]
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyDescription("Windows API functions and structures")]
[assembly: Guid("00000000-0006-11e8-9876-00055d74a52d")]

/*
    New version scheme as of 1.5.2020:

    Concept.Product.Classes.Minor

    Concept:    conceptual/strategic changes
    Product:    product-wide changes
    Classes:    new/changed classes or APIs
    Minor:      minor changes

    Kw.WinAPI change history
    ===============================================================================
    Date        Version        Comments
    ===============================================================================
    29.05.2020    1.2.0.0        New version scheme
    29.05.2020    1.2.0.1        Old icon tag is back
    29.05.2020    1.2.0.2        No icon whatsoever
    24.06.2020    1.3.0.0        signing changed to .SNK
    12.07.2020    1.3.2.0        VK as ushort and other input structures/enums
    13.07.2020    1.3.2.1        Minor changes to input types
    02.09.2020    2.0.0.0        Kwisatz.Haderach 2.0
    14.06.2021    2.1.0.0        C# 9.0, PostMessage, SendMessage
    
*/
