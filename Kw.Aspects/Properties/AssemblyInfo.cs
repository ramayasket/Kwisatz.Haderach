using System.Reflection;
using System.Runtime.InteropServices;

/*

	Kw.Aspects change history
	===============================================================================
	Date		Version		Comments
	===============================================================================
	04.05.2019	1.0.2.5		PostSharp 6.2.4-rc
	24.05.2019	1.0.2.6		PostSharp 6.2.6 + GetTotalMemoryAttribute
	16.06.2019	1.0.2.7		SingleInstanceAttribute (error)
	23.06.2019	1.0.2.8		SingleInstanceAttribute + CatchReturnAttribute (obsolete)
	23.06.2019	1.0.2.9		GuardedAttribute
	26.06.2019	1.0.2.11	GuardedAttribute + handler
	09.07.2019	1.0.2.13	InterceptedAttribute + Interceptors + consistent attribute names
*/

[assembly: AssemblyTitle("Kw.Aspects")]
[assembly: AssemblyVersion("1.0.2.13")]
[assembly: AssemblyDescription("All purpose aspects")]
[assembly: Guid("00000000-0002-11e8-9876-00055d74a52d")]
