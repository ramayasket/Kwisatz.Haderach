namespace Kw.Micro.Errors
{
    /// <summary>
    /// Thrown when some code is deemed invalid.
    /// </summary>
    public class CodeValidationException : Exception
    {
        public CodeValidationException(string message) : base(message) { }
    }
}
