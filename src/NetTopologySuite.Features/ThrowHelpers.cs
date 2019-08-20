using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Features
{
    internal static class ThrowHelpers
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedExceptionForReadOnlyCollection() => throw new NotSupportedException("Collection is read-only.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowKeyNotFoundException() => throw new KeyNotFoundException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentExceptionForDuplicateKey() => throw new ArgumentException("An item with the same key has already been added.", "key");
    }
}
