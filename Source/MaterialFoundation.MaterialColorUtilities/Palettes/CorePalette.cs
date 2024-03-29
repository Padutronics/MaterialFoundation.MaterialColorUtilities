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

using System;

namespace MaterialFoundation.MaterialColorUtilities.Palettes;

/// <summary>An intermediate concept between the key color for a UI theme, and a full color scheme. 5 sets of
/// tones are generated, all except one use the same hue as the key color, and all vary in chroma.</summary>
public sealed class CorePalette
{
    private CorePalette(int argb, bool isContent)
    {
        Hct.Hct hct = Hct.Hct.FromInt(argb);
        double hue = hct.Hue;
        double chroma = hct.Chroma;
        if (isContent)
        {
            A1 = TonalPalette.FromHueAndChroma(hue, chroma);
            A2 = TonalPalette.FromHueAndChroma(hue, chroma / 3.0);
            A3 = TonalPalette.FromHueAndChroma(hue + 60.0, chroma / 2.0);
            N1 = TonalPalette.FromHueAndChroma(hue, Math.Min(chroma / 12.0, 4.0));
            N2 = TonalPalette.FromHueAndChroma(hue, Math.Min(chroma / 6.0, 8.0));
        }
        else
        {
            A1 = TonalPalette.FromHueAndChroma(hue, Math.Max(48.0, chroma));
            A2 = TonalPalette.FromHueAndChroma(hue, 16.0);
            A3 = TonalPalette.FromHueAndChroma(hue + 60.0, 24.0);
            N1 = TonalPalette.FromHueAndChroma(hue, 4.0);
            N2 = TonalPalette.FromHueAndChroma(hue, 8.0);
        }
        Error = TonalPalette.FromHueAndChroma(25, 84.0);
    }

    public TonalPalette A1 { get; }

    public TonalPalette A2 { get; }

    public TonalPalette A3 { get; }

    public TonalPalette N1 { get; }

    public TonalPalette N2 { get; }

    public TonalPalette Error { get; }

    /// <summary>Create key tones from a color.</summary>
    /// <param name="argb">ARGB representation of a color</param>
    public static CorePalette Of(int argb)
    {
        return new CorePalette(argb, false);
    }

    /// <summary>Create content key tones from a color.</summary>
    /// <param name="argb">ARGB representation of a color</param>
    public static CorePalette ContentOf(int argb)
    {
        return new CorePalette(argb, true);
    }
}
