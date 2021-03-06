﻿using System.Globalization;
using System.Runtime.CompilerServices;

namespace Nanomesh
{
    public static class TextUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDouble(this string text)
        {
            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(this string text)
        {
            return float.Parse(text, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this string text)
        {
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
    }
}