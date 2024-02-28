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

using MaterialFoundation.MaterialColorUtilities.Dislike;
using MaterialFoundation.MaterialColorUtilities.Palettes;
using MaterialFoundation.MaterialColorUtilities.Temperature;
using System;

namespace MaterialFoundation.MaterialColorUtilities.Scheme;

/// <summary>A scheme that places the source color in Scheme.primaryContainer.
///
/// <para>Primary Container is the source color, adjusted for color relativity. It maintains constant
/// appearance in light mode and dark mode. This adds ~5 tone in light mode, and subtracts ~5 tone in
/// dark mode.</para>
///
/// <para>Tertiary Container is the complement to the source color, using TemperatureCache. It also
/// maintains constant appearance.</para></summary>
public class SchemeFidelity : DynamicScheme
{
    public SchemeFidelity(Hct.Hct sourceColorHct, bool isDark, double contrastLevel) :
        base(
            sourceColorHct,
            Variant.FIDELITY,
            isDark,
            contrastLevel,
            TonalPalette.fromHueAndChroma(sourceColorHct.getHue(), sourceColorHct.getChroma()),
            TonalPalette.fromHueAndChroma(sourceColorHct.getHue(), Math.Max(sourceColorHct.getChroma() - 32.0, sourceColorHct.getChroma() * 0.5)),
            TonalPalette.fromHct(DislikeAnalyzer.fixIfDisliked(new TemperatureCache(sourceColorHct).getComplement())),
            TonalPalette.fromHueAndChroma(sourceColorHct.getHue(), sourceColorHct.getChroma() / 8.0),
            TonalPalette.fromHueAndChroma(sourceColorHct.getHue(), (sourceColorHct.getChroma() / 8.0) + 4.0)
        )
    {
    }
}
