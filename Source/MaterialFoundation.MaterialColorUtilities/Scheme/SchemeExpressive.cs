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

/// <summary>A playful theme - the source color's hue does not appear in the theme.</summary>
public class SchemeExpressive : DynamicScheme
{
    private static readonly double[] Hues = [0, 21, 51, 121, 151, 191, 271, 321, 360];
    private static readonly double[] SecondaryRotations = [45, 95, 45, 20, 45, 90, 45, 45, 45];
    private static readonly double[] TertiaryRotations = [120, 120, 20, 45, 20, 15, 20, 120, 120];

    public SchemeExpressive(Hct.Hct sourceColorHct, bool isDark, double contrastLevel) :
        base(
            sourceColorHct,
            Variant.Expressive,
            isDark,
            contrastLevel,
            TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 240.0), 40.0),
            TonalPalette.FromHueAndChroma(DynamicScheme.GetRotatedHue(sourceColorHct, Hues, SecondaryRotations), 24.0),
            TonalPalette.FromHueAndChroma(DynamicScheme.GetRotatedHue(sourceColorHct, Hues, TertiaryRotations), 32.0),
            TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 15.0), 8.0),
            TonalPalette.FromHueAndChroma(MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 15.0), 12.0)
        )
    {
    }
}
