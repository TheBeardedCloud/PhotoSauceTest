// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable IDE1006 // Naming Styles

#if DRAWING_SHIM_COLORCONVERTER
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace System.Drawing.ColorShim
{
    internal static class ColorTable
    {
        private static readonly Lazy<Dictionary<string, Color>> s_colorConstants = new Lazy<Dictionary<string, Color>>(GetColors);

        private static Dictionary<string, Color> GetColors()
        {
            var dict = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
            FillConstants(dict);
            return dict;
        }

        private static void FillConstants(Dictionary<string, Color> colors)
        {
#if DRAWING_SHIM_COLOR
            for (int i = (int)KnownColor.Transparent; i <= (int)KnownColor.YellowGreen; i++)
                colors[KnownColorTable.KnownColorToName((KnownColor)i)] = new Color((KnownColor)i);
#else
            foreach (var prop in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.PropertyType == typeof(Color)))
                colors[prop.Name] = (Color)prop.GetValue(null);
#endif
        }

        internal static Dictionary<string, Color> Colors => s_colorConstants.Value;

        internal static bool TryGetNamedColor(string name, out Color result) => Colors.TryGetValue(name, out result);

        internal static bool IsKnownNamedColor(string name) => Colors.ContainsKey(name);
    }
}
#endif