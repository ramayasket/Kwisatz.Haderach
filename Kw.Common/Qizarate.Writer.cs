using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
    /// <summary>
    /// Library-wide helper class.
    /// </summary>
    public static partial class Qizarate
    {
        internal class Writer : TextWriter
        {
            /// <inheritdoc />
            public override Encoding Encoding => Encoding.UTF8;

            /// <inheritdoc />
            public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

            /// <inheritdoc />
            public override string NewLine => Environment.NewLine;

            /// <inheritdoc />
            public override void WriteLine(string format, params object[] arg) => Debug.WriteLine(format, arg);

            /// <inheritdoc />
            public override void Write(string format, params object[] arg) => Debug.Write(string.Format(format, arg));

            /// <inheritdoc />
            public override void Write(ulong value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(uint value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(string format, object arg0, object arg1, object arg2) => Write(format, new [] {arg0, arg1, arg2 });

            /// <inheritdoc />
            public override void Write(string format, object arg0, object arg1) => Write(format, new[] { arg0, arg1 });

            /// <inheritdoc />
            public override void Write(string format, object arg0) => Write(format, new [] { arg0 });

            /// <inheritdoc />
            public override void Write(string value) => Write(value, new object[0]);

            /// <inheritdoc />
            public override void Write(object value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(long value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(int value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(double value) => Write(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void Write(decimal value) => Write(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void Write(char value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(bool value) => Write(value.ToString());

            /// <inheritdoc />
            public override void Write(float value) => Write(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void Write(char[] buffer, int index, int count) => throw new NotImplementedException();

            /// <inheritdoc />
            public override void Write(char[] buffer) => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteAsync(string value) => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteAsync(char value) => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteAsync(char[] buffer, int index, int count) => throw new NotImplementedException();

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0) => WriteLine(format, new[] { arg0 });

            /// <inheritdoc />
            public override void WriteLine(ulong value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(uint value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0, object arg1, object arg2) => WriteLine(format, new [] { arg0, arg1, arg2 });

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0, object arg1) => WriteLine(format, new[] { arg0, arg1, });

            /// <inheritdoc />
            public override void WriteLine(float value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void WriteLine() => Debug.Write(Environment.NewLine);

            /// <inheritdoc />
            public override void WriteLine(long value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(int value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(double value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void WriteLine(decimal value) => WriteLine(value.ToString(CultureInfo.InvariantCulture));

            /// <inheritdoc />
            public override void WriteLine(char[] buffer, int index, int count) => throw new NotImplementedException();

            /// <inheritdoc />
            public override void WriteLine(char[] buffer) => throw new NotImplementedException();

            /// <inheritdoc />
            public override void WriteLine(char value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(bool value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override void WriteLine(object value) => WriteLine(value.ToString());

            /// <inheritdoc />
            public override Task WriteLineAsync() => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteLineAsync(char value) => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteLineAsync(char[] buffer, int index, int count) => throw new NotImplementedException();

            /// <inheritdoc />
            public override Task WriteLineAsync(string value) => throw new NotImplementedException();
        }
    }
}
