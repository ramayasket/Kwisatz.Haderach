using System.Text;

namespace Kw.Common.ZSpitz
{
    public static class CharExtensions {
        public static void AppendTo(this char c, StringBuilder sb) => sb.Append(c);
    }
}
