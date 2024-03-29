/*
 * Copyright 2024 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using MaterialFoundation.MaterialColorUtilities.Palettes;
using MaterialFoundation.MaterialColorUtilities.Utils;

namespace MaterialFoundation.MaterialColorUtilities.Scheme;

/// <summary>Provides important settings for creating colors dynamically, and 6 color palettes. Requires: 1. A
/// color. (source color) 2. A theme. (Variant) 3. Whether or not its dark mode. 4. Contrast level.
/// (-1 to 1, currently contrast ratio 3.0 and 7.0)</summary>
public class DynamicScheme
{
    public DynamicScheme(Hct.Hct sourceColorHct, Variant variant, bool isDark, double contrastLevel, TonalPalette primaryPalette, TonalPalette secondaryPalette, TonalPalette tertiaryPalette, TonalPalette neutralPalette, TonalPalette neutralVariantPalette)
    {
        SourceColorArgb = sourceColorHct.ToInt();
        SourceColorHct = sourceColorHct;
        Variant = variant;
        IsDark = isDark;
        ContrastLevel = contrastLevel;

        PrimaryPalette = primaryPalette;
        SecondaryPalette = secondaryPalette;
        TertiaryPalette = tertiaryPalette;
        NeutralPalette = neutralPalette;
        NeutralVariantPalette = neutralVariantPalette;
        ErrorPalette = TonalPalette.FromHueAndChroma(25.0, 84.0);
    }

    public int SourceColorArgb { get; }

    public Hct.Hct SourceColorHct { get; }

    public Variant Variant { get; }

    public bool IsDark { get; }

    public double ContrastLevel { get; }

    public TonalPalette PrimaryPalette { get; }

    public TonalPalette SecondaryPalette { get; }

    public TonalPalette TertiaryPalette { get; }

    public TonalPalette NeutralPalette { get; }

    public TonalPalette NeutralVariantPalette { get; }

    public TonalPalette ErrorPalette { get; }

    /// <summary>Given a set of hues and set of hue rotations, locate which hues the source color's hue is
    /// between, apply the rotation at the same index as the first hue in the range, and return the
    /// rotated hue.</summary>
    /// <param name="sourceColorHct">The color whose hue should be rotated.</param>
    /// <param name="hues">A set of hues.</param>
    /// <param name="rotations">A set of hue rotations.</param>
    /// <returns>Color's hue with a rotation applied.</returns>
    public static double GetRotatedHue(Hct.Hct sourceColorHct, double[] hues, double[] rotations)
    {
        double sourceHue = sourceColorHct.Hue;
        if (rotations.Length == 1)
        {
            return MathUtils.SanitizeDegreesDouble(sourceHue + rotations[0]);
        }
        int size = hues.Length;
        for (int i = 0; i <= size - 2; i++)
        {
            double thisHue = hues[i];
            double nextHue = hues[i + 1];
            if (thisHue < sourceHue && sourceHue < nextHue)
            {
                return MathUtils.SanitizeDegreesDouble(sourceHue + rotations[i]);
            }
        }
        // If this statement executes, something is wrong, there should have been a rotation
        // found using the arrays.
        return sourceHue;
    }
}
