using System;

namespace Kw.Common
{
    /// <summary>
    /// HRESULT's severity value.
    /// </summary>
    public enum SEVERITY : uint
    {
        SUCCESS = 0x00000000,
        ERROR = 0x80000000,
    }

    /// <summary>
    /// COM error code.
    /// </summary>
    public struct HRESULT
    {
        public override string ToString()
        {
            return SEVERITY.SUCCESS == Severity ? "SUCCESS" : $"{Facility:X}:{Code:X}";
        }

        /// <summary>
        /// Facility's status code.
        /// </summary>
        public ushort Code
        {
            get { return (ushort)(_value & 0xffff); }
            set { _value = (_value & 0xffff0000) | (uint)value & 0xffff; }
        }

        /// <summary>
        /// Indicates the system service that is responsible for the error.
        /// </summary>
        public ushort Facility
        {
            get { return (ushort)((_value & 0x07ff0000) >> 16); }
            set
            {
                if (0 != (value & 0xf800))
                    throw new ArgumentOutOfRangeException(nameof(value), "Expected value in range 0x0 - 0x07ff.");

                _value = (_value & 0xf800ffff) | (uint)value << 16;
            }
        }

        /// <summary>
        /// Indicates success/failure.
        /// </summary>
        public SEVERITY Severity
        {
            get { return (SEVERITY)(_value & (uint)SEVERITY.ERROR); }
            set { _value = (_value & 0x7fffffff) | (uint)value; }
        }

        private uint _value;

        /// <summary>
        /// Initializes a new instance of HRESULT structure with Int32 value.
        /// </summary>
        public HRESULT(int value)
        {
            _value = (uint)value;
        }

        /// <summary>
        /// Initializes a new instance of HRESULT structure with UInt32 value.
        /// </summary>
        public HRESULT(uint value)
        {
            _value = value;
        }

        /// <summary>
        /// Converts HRESULT to Int32 value.
        /// </summary>
        /// <param name="hr">HRESULT to convert.</param>
        public static implicit operator int(HRESULT hr)
        {
            return (int)hr._value;
        }

        /// <summary>
        /// Converts HRESULT to UInt32 value.
        /// </summary>
        /// <param name="hr">HRESULT to convert.</param>
        public static implicit operator uint(HRESULT hr)
        {
            return hr._value;
        }

        /// <summary>
        /// Converts Int32 value to HRESULT.
        /// </summary>
        /// <param name="value">Int32 value to convert.</param>
        public static implicit operator HRESULT(int value)
        {
            return new HRESULT(value);
        }

        /// <summary>
        /// Converts UInt32 value to HRESULT.
        /// </summary>
        /// <param name="value">UInt32 value to convert.</param>
        public static implicit operator HRESULT(uint value)
        {
            return new HRESULT(value);
        }
    }
}
