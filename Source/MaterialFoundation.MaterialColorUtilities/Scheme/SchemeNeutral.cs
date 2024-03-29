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

namespace MaterialFoundation.MaterialColorUtilities.Scheme;

/// <summary>A theme that's slightly more chromatic than monochrome, which is purely black / white / gray.</summary>
public class SchemeNeutral : DynamicScheme
{
    public SchemeNeutral(Hct.Hct sourceColorHct, bool isDark, double contrastLevel) :
        base(
            sourceColorHct,
            Variant.Neutral,
            isDark,
            contrastLevel,
            TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 12.0),
            TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 8.0),
            TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 16.0),
            TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0),
            TonalPalette.FromHueAndChroma(sourceColorHct.Hue, 2.0)
        )
    {
    }
}
