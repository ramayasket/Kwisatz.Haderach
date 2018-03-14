using Kw.Aspects;

/// <summary>
///     Presense of this class suppresses postsharp warning PS0131: The module 'XXX' does not contain any aspect or other
///     transformation.
/// </summary>
/// ReSharper disable once InconsistentNaming
internal class __PS0131
{
	[ExecutionTiming]
	private void nop()
	{
	}
}
