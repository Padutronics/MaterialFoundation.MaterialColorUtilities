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
using System.Collections.Generic;

namespace MaterialFoundation.MaterialColorUtilities.Palettes;

/// <summary>A convenience class for retrieving colors that are constant in hue and chroma, but vary in tone.</summary>
public sealed class TonalPalette
{
    private readonly IDictionary<int, int> cache = new Dictionary<int, int>();

    private TonalPalette(double hue, double chroma, Hct.Hct keyColor)
    {
        Hue = hue;
        Chroma = chroma;
        KeyColor = keyColor;
    }

    /// <summary>The chroma of the Tonal Palette, in HCT. Ranges from 0 to ~130 (for sRGB gamut).</summary>
    public double Chroma { get; }

    /// <summary>The hue of the Tonal Palette, in HCT. Ranges from 0 to 360.</summary>
    public double Hue { get; }

    /// <summary>The key color is the first tone, starting from T50, that matches the palette's chroma.</summary>
    public Hct.Hct KeyColor { get; }

    /// <summary>Create tones using the HCT hue and chroma from a color.</summary>
    /// <param name="argb">ARGB representation of a color</param>
    /// <returns>Tones matching that color's hue and chroma.</returns>
    public static TonalPalette FromInt(int argb)
    {
        return FromHct(Hct.Hct.FromInt(argb));
    }

    /// <summary>Create tones using a HCT color.</summary>
    /// <param name="hct">HCT representation of a color.</param>
    /// <returns>Tones matching that color's hue and chroma.</returns>
    public static TonalPalette FromHct(Hct.Hct hct)
    {
        return new TonalPalette(hct.Hue, hct.Chroma, hct);
    }

    /// <summary>Create tones from a defined HCT hue and chroma.</summary>
    /// <param name="hue">HCT hue</param>
    /// <param name="chroma">HCT chroma</param>
    /// <returns>Tones matching hue and chroma.</returns>
    public static TonalPalette FromHueAndChroma(double hue, double chroma)
    {
        return new TonalPalette(hue, chroma, CreateKeyColor(hue, chroma));
    }

    /// <summary>The key color is the first tone, starting from T50, matching the given hue and chroma.</summary>
    private static Hct.Hct CreateKeyColor(double hue, double chroma)
    {
        double startTone = 50.0;
        Hct.Hct smallestDeltaHct = Hct.Hct.From(hue, chroma, startTone);
        double smallestDelta = Math.Abs(smallestDeltaHct.Chroma - chroma);
        // Starting from T50, check T+/-delta to see if they match the requested
        // chroma.
        //
        // Starts from T50 because T50 has the most chroma available, on
        // average. Thus it is most likely to have a direct answer and minimize
        // iteration.
        for (double delta = 1.0; delta < 50.0; delta += 1.0)
        {
            // Termination condition rounding instead of minimizing delta to avoid
            // case where requested chroma is 16.51, and the closest chroma is 16.49.
            // Error is minimized, but when rounded and displayed, requested chroma
            // is 17, key color's chroma is 16.
            if (Math.Round(chroma) == Math.Round(smallestDeltaHct.Chroma))
            {
                return smallestDeltaHct;
            }

            Hct.Hct hctAdd = Hct.Hct.From(hue, chroma, startTone + delta);
            double hctAddDelta = Math.Abs(hctAdd.Chroma - chroma);
            if (hctAddDelta < smallestDelta)
            {
                smallestDelta = hctAddDelta;
                smallestDeltaHct = hctAdd;
            }

            Hct.Hct hctSubtract = Hct.Hct.From(hue, chroma, startTone - delta);
            double hctSubtractDelta = Math.Abs(hctSubtract.Chroma - chroma);
            if (hctSubtractDelta < smallestDelta)
            {
                smallestDelta = hctSubtractDelta;
                smallestDeltaHct = hctSubtract;
            }
        }

        return smallestDeltaHct;
    }

    /// <summary>Create an ARGB color with HCT hue and chroma of this Tones instance, and the provided HCT tone.</summary>
    /// <param name="tone">HCT tone, measured from 0 to 100.</param>
    /// <returns>ARGB representation of a color with that tone.</returns>
    public int Tone(int tone)
    {
        if (!cache.TryGetValue(tone, out int color))
        {
            color = Hct.Hct.From(Hue, Chroma, tone).ToInt();
            cache.Add(tone, color);
        }
        return color;
    }

    /// <summary>Given a tone, use hue and chroma of palette to create a color, and return it as HCT.</summary>
    public Hct.Hct GetHct(double tone)
    {
        return Hct.Hct.From(Hue, Chroma, tone);
    }
}
