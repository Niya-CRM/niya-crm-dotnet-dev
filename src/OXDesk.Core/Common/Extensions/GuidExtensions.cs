using System;

namespace OXDesk.Core.Common.Extensions
{
    /// <summary>
    /// Provides helpers for working with Guid conversions.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts an integer value into a deterministic Guid.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>A Guid representing the provided value.</returns>
        public static Guid ToGuid(this int value)
        {
            Span<byte> bytes = stackalloc byte[16];
            if (!BitConverter.TryWriteBytes(bytes, value))
            {
                throw new InvalidOperationException("Unable to convert integer value to Guid bytes.");
            }

            return new Guid(bytes);
        }
    }
}
