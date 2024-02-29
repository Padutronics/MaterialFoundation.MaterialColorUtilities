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
using MaterialFoundation.MaterialColorUtilities.Scheme;
using MaterialFoundation.MaterialColorUtilities.Utils;
using System;
using System.Collections.Generic;

namespace MaterialFoundation.MaterialColorUtilities.DynamicColor;

/// <summary>A color that adjusts itself based on UI state, represented by DynamicScheme.
///
/// <para>This color automatically adjusts to accommodate a desired contrast level, or other adjustments
/// such as differing in light mode versus dark mode, or what the theme is, or what the color that
/// produced the theme is, etc.</para>
///
/// <para>Colors without backgrounds do not change tone when contrast changes. Colors with backgrounds
/// become closer to their background as contrast lowers, and further when contrast increases.</para>
///
/// <para>Prefer the static constructors. They provide a much more simple interface, such as requiring
/// just a hexcode, or just a hexcode and a background.</para>
///
/// <para>Ultimately, each component necessary for calculating a color, adjusting it for a desired
/// contrast level, and ensuring it has a certain lightness/tone difference from another color, is
/// provided by a function that takes a DynamicScheme and returns a value. This ensures ultimate
/// flexibility, any desired behavior of a color for any design system, but it usually unnecessary.
/// See the default constructor for more information.</para></summary>
public sealed class DynamicColor
{
    public readonly string name;
    public readonly Func<DynamicScheme, TonalPalette> palette;
    public readonly Func<DynamicScheme, double> tone;
    public readonly bool isBackground;
    public readonly Func<DynamicScheme, DynamicColor>? background;
    public readonly Func<DynamicScheme, DynamicColor>? secondBackground;
    public readonly ContrastCurve? contrastCurve;
    public readonly Func<DynamicScheme, ToneDeltaPair>? toneDeltaPair;
    public readonly Func<DynamicScheme, double>? opacity;
    private readonly IDictionary<DynamicScheme, Hct.Hct> hctCache = new Dictionary<DynamicScheme, Hct.Hct>();

    /// <summary>A constructor for DynamicColor.
    ///
    /// <para>_Strongly_ prefer using one of the convenience constructors. This class is arguably too
    /// flexible to ensure it can support any scenario. Functional arguments allow overriding without
    /// risks that come with subclasses.</para>
    ///
    /// <para>For example, the default behavior of adjust tone at max contrast to be at a 7.0 ratio with
    /// its background is principled and matches accessibility guidance. That does not mean it's the
    /// desired approach for _every_ design system, and every color pairing, always, in every case.</para>
    ///
    /// <para>For opaque colors (colors with alpha = 100%).</para></summary>
    /// <param name="name">The name of the dynamic color.</param>
    /// <param name="palette">Function that provides a TonalPalette given DynamicScheme. A TonalPalette is
    /// defined by a hue and chroma, so this replaces the need to specify hue/chroma. By providing
    /// a tonal palette, when contrast adjustments are made, intended chroma can be preserved.</param>
    /// <param name="tone">Function that provides a tone, given a DynamicScheme.</param>
    /// <param name="isBackground">Whether this dynamic color is a background, with some other color as the
    /// foreground.</param>
    /// <param name="background">The background of the dynamic color (as a function of a `DynamicScheme`), if
    /// it exists.</param>
    /// <param name="secondBackground">A second background of the dynamic color (as a function of a
    /// `DynamicScheme`), if it exists.</param>
    /// <param name="contrastCurve">A `ContrastCurve` object specifying how its contrast against its
    /// background should behave in various contrast levels options.</param>
    /// <param name="toneDeltaPair">A `ToneDeltaPair` object specifying a tone delta constraint between two
    /// colors. One of them must be the color being constructed.</param>
    public DynamicColor(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, bool isBackground, Func<DynamicScheme, DynamicColor>? background, Func<DynamicScheme, DynamicColor>? secondBackground, ContrastCurve? contrastCurve, Func<DynamicScheme, ToneDeltaPair>? toneDeltaPair)
    {
        this.name = name;
        this.palette = palette;
        this.tone = tone;
        this.isBackground = isBackground;
        this.background = background;
        this.secondBackground = secondBackground;
        this.contrastCurve = contrastCurve;
        this.toneDeltaPair = toneDeltaPair;
        this.opacity = null;
    }

    /// <summary>A constructor for DynamicColor.
    ///
    /// <para>_Strongly_ prefer using one of the convenience constructors. This class is arguably too
    /// flexible to ensure it can support any scenario. Functional arguments allow overriding without
    /// risks that come with subclasses.</para>
    ///
    /// <para>For example, the default behavior of adjust tone at max contrast to be at a 7.0 ratio with
    /// its background is principled and matches accessibility guidance. That does not mean it's the
    /// desired approach for _every_ design system, and every color pairing, always, in every case.</para>
    ///
    /// <para>For opaque colors (colors with alpha = 100%).</para></summary>
    /// <param name="name">The name of the dynamic color.</param>
    /// <param name="palette">Function that provides a TonalPalette given DynamicScheme. A TonalPalette is
    /// defined by a hue and chroma, so this replaces the need to specify hue/chroma. By providing
    /// a tonal palette, when contrast adjustments are made, intended chroma can be preserved.</param>
    /// <param name="tone">Function that provides a tone, given a DynamicScheme.</param>
    /// <param name="isBackground">Whether this dynamic color is a background, with some other color as the
    /// foreground.</param>
    /// <param name="background">The background of the dynamic color (as a function of a `DynamicScheme`), if
    /// it exists.</param>
    /// <param name="secondBackground">A second background of the dynamic color (as a function of a
    /// `DynamicScheme`), if it exists.</param>
    /// <param name="contrastCurve">A `ContrastCurve` object specifying how its contrast against its
    /// background should behave in various contrast levels options.</param>
    /// <param name="toneDeltaPair">A `ToneDeltaPair` object specifying a tone delta constraint between two
    /// colors. One of them must be the color being constructed.</param>
    /// <param name="opacity">A function returning the opacity of a color, as a number between 0 and 1.</param>
    public DynamicColor(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, bool isBackground, Func<DynamicScheme, DynamicColor>? background, Func<DynamicScheme, DynamicColor>? secondBackground, ContrastCurve? contrastCurve, Func<DynamicScheme, ToneDeltaPair>? toneDeltaPair, Func<DynamicScheme, double>? opacity)
    {
        this.name = name;
        this.palette = palette;
        this.tone = tone;
        this.isBackground = isBackground;
        this.background = background;
        this.secondBackground = secondBackground;
        this.contrastCurve = contrastCurve;
        this.toneDeltaPair = toneDeltaPair;
        this.opacity = opacity;
    }

    /// <summary>A convenience constructor for DynamicColor.
    ///
    /// <para>_Strongly_ prefer using one of the convenience constructors. This class is arguably too
    /// flexible to ensure it can support any scenario. Functional arguments allow overriding without
    /// risks that come with subclasses.</para>
    ///
    /// <para>For example, the default behavior of adjust tone at max contrast to be at a 7.0 ratio with
    /// its background is principled and matches accessibility guidance. That does not mean it's the
    /// desired approach for _every_ design system, and every color pairing, always, in every case.</para>
    ///
    /// <para>For opaque colors (colors with alpha = 100%).</para>
    ///
    /// <para>For colors that are not backgrounds, and do not have backgrounds.</para></summary>
    /// <param name="name">The name of the dynamic color.</param>
    /// <param name="palette">Function that provides a TonalPalette given DynamicScheme. A TonalPalette is
    /// defined by a hue and chroma, so this replaces the need to specify hue/chroma. By providing
    /// a tonal palette, when contrast adjustments are made, intended chroma can be preserved.</param>
    /// <param name="tone">Function that provides a tone, given a DynamicScheme.</param>
    public static DynamicColor FromPalette(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone)
    {
        return new DynamicColor(
            name,
            palette,
            tone,
            isBackground: false,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    /// <summary>A convenience constructor for DynamicColor.
    ///
    /// <para>_Strongly_ prefer using one of the convenience constructors. This class is arguably too
    /// flexible to ensure it can support any scenario. Functional arguments allow overriding without
    /// risks that come with subclasses.</para>
    ///
    /// <para>For example, the default behavior of adjust tone at max contrast to be at a 7.0 ratio with
    /// its background is principled and matches accessibility guidance. That does not mean it's the
    /// desired approach for _every_ design system, and every color pairing, always, in every case.</para>
    ///
    /// <para>For opaque colors (colors with alpha = 100%).</para>
    ///
    /// <para>For colors that do not have backgrounds.</para></summary>
    /// <param name="name">The name of the dynamic color.</param>
    /// <param name="palette">Function that provides a TonalPalette given DynamicScheme. A TonalPalette is
    /// defined by a hue and chroma, so this replaces the need to specify hue/chroma. By providing
    /// a tonal palette, when contrast adjustments are made, intended chroma can be preserved.</param>
    /// <param name="tone">Function that provides a tone, given a DynamicScheme.</param>
    /// <param name="isBackground">Whether this dynamic color is a background, with some other color as the
    /// foreground.</param>
    public static DynamicColor FromPalette(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, bool isBackground)
    {
        return new DynamicColor(
            name,
            palette,
            tone,
            isBackground,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    /// <summary>Create a DynamicColor from a hex code.
    ///
    /// <para>Result has no background; thus no support for increasing/decreasing contrast for a11y.</para></summary>
    /// <param name="name">The name of the dynamic color.</param>
    /// <param name="argb">The source color from which to extract the hue and chroma.</param>
    public static DynamicColor FromArgb(string name, int argb)
    {
        Hct.Hct hct = Hct.Hct.FromInt(argb);
        TonalPalette palette = TonalPalette.FromInt(argb);
        return FromPalette(name, s => palette, s => hct.Tone);
    }

    /// <summary>Given a background tone, find a foreground tone, while ensuring they reach a contrast ratio
    /// that is as close to ratio as possible.</summary>
    public static double ForegroundTone(double bgTone, double ratio)
    {
        double lighterTone = Contrast.Contrast.LighterUnsafe(bgTone, ratio);
        double darkerTone = Contrast.Contrast.DarkerUnsafe(bgTone, ratio);
        double lighterRatio = Contrast.Contrast.RatioOfTones(lighterTone, bgTone);
        double darkerRatio = Contrast.Contrast.RatioOfTones(darkerTone, bgTone);
        bool preferLighter = TonePrefersLightForeground(bgTone);

        if (preferLighter)
        {
            // "Neglible difference" handles an edge case where the initial contrast ratio is high
            // (ex. 13.0), and the ratio passed to the function is that high ratio, and both the lighter
            // and darker ratio fails to pass that ratio.
            //
            // This was observed with Tonal Spot's On Primary Container turning black momentarily between
            // high and max contrast in light mode. PC's standard tone was T90, OPC's was T10, it was
            // light mode, and the contrast level was 0.6568521221032331.
            bool negligibleDifference = Math.Abs(lighterRatio - darkerRatio) < 0.1 && lighterRatio < ratio && darkerRatio < ratio;
            if (lighterRatio >= ratio || lighterRatio >= darkerRatio || negligibleDifference)
            {
                return lighterTone;
            }
            else
            {
                return darkerTone;
            }
        }
        else
        {
            return darkerRatio >= ratio || darkerRatio >= lighterRatio ? darkerTone : lighterTone;
        }
    }

    /// <summary>Adjust a tone down such that white has 4.5 contrast, if the tone is reasonably close to
    /// supporting it.</summary>
    public static double EnableLightForeground(double tone)
    {
        if (TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone))
        {
            return 49.0;
        }
        return tone;
    }

    /// <summary>People prefer white foregrounds on ~T60-70. Observed over time, and also by Andrew Somers
    /// during research for APCA.
    ///
    /// <para>T60 used as to create the smallest discontinuity possible when skipping down to T49 in order
    /// to ensure light foregrounds.</para>
    ///
    /// <para>Since `tertiaryContainer` in dark monochrome scheme requires a tone of 60, it should not be
    /// adjusted. Therefore, 60 is excluded here.</para></summary>
    public static bool TonePrefersLightForeground(double tone)
    {
        return Math.Round(tone) < 60;
    }

    /// <summary>Tones less than ~T50 always permit white at 4.5 contrast.</summary>
    public static bool ToneAllowsLightForeground(double tone)
    {
        return Math.Round(tone) <= 49;
    }

    /// <summary>Returns an ARGB integer (i.e. a hex code).</summary>
    /// <param name="scheme">Defines the conditions of the user interface, for example, whether or not it is
    /// dark mode or light mode, and what the desired contrast level is.</param>
    public int GetArgb(DynamicScheme scheme)
    {
        int argb = GetHct(scheme).ToInt();
        if (opacity == null)
        {
            return argb;
        }
        double percentage = opacity(scheme);
        int alpha = MathUtils.ClampInt(0, 255, (int)Math.Round(percentage * 255));
        return (argb & 0x00ffffff) | (alpha << 24);
    }

    /// <summary>Returns an HCT object.</summary>
    /// <param name="scheme">Defines the conditions of the user interface, for example, whether or not it is
    /// dark mode or light mode, and what the desired contrast level is.</param>
    public Hct.Hct GetHct(DynamicScheme scheme)
    {
        if (hctCache.TryGetValue(scheme, out Hct.Hct? cachedAnswer))
        {
            return cachedAnswer;
        }
        // This is crucial for aesthetics: we aren't simply the taking the standard color
        // and changing its tone for contrast. Rather, we find the tone for contrast, then
        // use the specified chroma from the palette to construct a new color.
        //
        // For example, this enables colors with standard tone of T90, which has limited chroma, to
        // "recover" intended chroma as contrast increases.
        double tone = GetTone(scheme);
        Hct.Hct answer = palette(scheme).GetHct(tone);
        if (hctCache.Count > 4)
        {
            hctCache.Clear();
        }
        hctCache.Add(scheme, answer);
        return answer;
    }

    /// <summary>Returns the tone in HCT, ranging from 0 to 100, of the resolved color given scheme.</summary>
    public double GetTone(DynamicScheme scheme)
    {
        bool decreasingContrast = scheme.ContrastLevel < 0;

        // Case 1: dual foreground, pair of colors with delta constraint.
        if (toneDeltaPair != null)
        {
            ToneDeltaPair toneDeltaPair = this.toneDeltaPair(scheme);
            DynamicColor roleA = toneDeltaPair.RoleA;
            DynamicColor roleB = toneDeltaPair.RoleB;
            double delta = toneDeltaPair.Delta;
            TonePolarity polarity = toneDeltaPair.Polarity;
            bool stayTogether = toneDeltaPair.StayTogether;

            DynamicColor bg = background!(scheme);
            double bgTone = bg.GetTone(scheme);

            bool aIsNearer = polarity == TonePolarity.Nearer || polarity == TonePolarity.Lighter && !scheme.IsDark || polarity == TonePolarity.Darker && scheme.IsDark;
            DynamicColor nearer = aIsNearer ? roleA : roleB;
            DynamicColor farther = aIsNearer ? roleB : roleA;
            bool amNearer = name.Equals(nearer.name);
            double expansionDir = scheme.IsDark ? 1 : -1;

            // 1st round: solve to min, each
            double nContrast = nearer.contrastCurve!.Get(scheme.ContrastLevel);
            double fContrast = farther.contrastCurve!.Get(scheme.ContrastLevel);

            // If a color is good enough, it is not adjusted.
            // Initial and adjusted tones for `nearer`
            double nInitialTone = nearer.tone(scheme);
            double nTone = Contrast.Contrast.RatioOfTones(bgTone, nInitialTone) >= nContrast
                ? nInitialTone
                : ForegroundTone(bgTone, nContrast);
            // Initial and adjusted tones for `farther`
            double fInitialTone = farther.tone(scheme);
            double fTone = Contrast.Contrast.RatioOfTones(bgTone, fInitialTone) >= fContrast
                ? fInitialTone
                : ForegroundTone(bgTone, fContrast);

            if (decreasingContrast)
            {
                // If decreasing contrast, adjust color to the "bare minimum"
                // that satisfies contrast.
                nTone = ForegroundTone(bgTone, nContrast);
                fTone = ForegroundTone(bgTone, fContrast);
            }

            // If constraint is not satisfied, try another round.
            if ((fTone - nTone) * expansionDir < delta)
            {
                // 2nd round: expand farther to match delta.
                fTone = MathUtils.ClampDouble(0, 100, nTone + delta * expansionDir);
                // If constraint is not satisfied, try another round.
                if ((fTone - nTone) * expansionDir < delta)
                {
                    // 3rd round: contract nearer to match delta.
                    nTone = MathUtils.ClampDouble(0, 100, fTone - delta * expansionDir);
                }
            }

            // Avoids the 50-59 awkward zone.
            if (50 <= nTone && nTone < 60)
            {
                // If `nearer` is in the awkward zone, move it away, together with
                // `farther`.
                if (expansionDir > 0)
                {
                    nTone = 60;
                    fTone = Math.Max(fTone, nTone + delta * expansionDir);
                }
                else
                {
                    nTone = 49;
                    fTone = Math.Min(fTone, nTone + delta * expansionDir);
                }
            }
            else if (50 <= fTone && fTone < 60)
            {
                if (stayTogether)
                {
                    // Fixes both, to avoid two colors on opposite sides of the "awkward
                    // zone".
                    if (expansionDir > 0)
                    {
                        nTone = 60;
                        fTone = Math.Max(fTone, nTone + delta * expansionDir);
                    }
                    else
                    {
                        nTone = 49;
                        fTone = Math.Min(fTone, nTone + delta * expansionDir);
                    }
                }
                else
                {
                    // Not required to stay together; fixes just one.
                    if (expansionDir > 0)
                    {
                        fTone = 60;
                    }
                    else
                    {
                        fTone = 49;
                    }
                }
            }

            // Returns `nTone` if this color is `nearer`, otherwise `fTone`.
            return amNearer ? nTone : fTone;
        }
        else
        {
            // Case 2: No contrast pair; just solve for itself.
            double answer = tone(scheme);

            if (background == null)
            {
                return answer; // No adjustment for colors with no background.
            }

            double bgTone = background(scheme).GetTone(scheme);

            double desiredRatio = contrastCurve!.Get(scheme.ContrastLevel);

            if (Contrast.Contrast.RatioOfTones(bgTone, answer) >= desiredRatio)
            {
                // Don't "improve" what's good enough.
            }
            else
            {
                // Rough improvement.
                answer = ForegroundTone(bgTone, desiredRatio);
            }

            if (decreasingContrast)
            {
                answer = ForegroundTone(bgTone, desiredRatio);
            }

            if (isBackground && 50 <= answer && answer < 60)
            {
                // Must adjust
                if (Contrast.Contrast.RatioOfTones(49, bgTone) >= desiredRatio)
                {
                    answer = 49;
                }
                else
                {
                    answer = 60;
                }
            }

            if (secondBackground != null)
            {
                // Case 3: Adjust for dual backgrounds.

                double bgTone1 = background(scheme).GetTone(scheme);
                double bgTone2 = secondBackground(scheme).GetTone(scheme);

                double upper = Math.Max(bgTone1, bgTone2);
                double lower = Math.Min(bgTone1, bgTone2);

                if (Contrast.Contrast.RatioOfTones(upper, answer) >= desiredRatio && Contrast.Contrast.RatioOfTones(lower, answer) >= desiredRatio)
                {
                    return answer;
                }

                // The darkest light tone that satisfies the desired ratio,
                // or -1 if such ratio cannot be reached.
                double lightOption = Contrast.Contrast.Lighter(upper, desiredRatio);

                // The lightest dark tone that satisfies the desired ratio,
                // or -1 if such ratio cannot be reached.
                double darkOption = Contrast.Contrast.Darker(lower, desiredRatio);

                // Tones suitable for the foreground.
                var availables = new List<double>();
                if (lightOption != -1)
                {
                    availables.Add(lightOption);
                }
                if (darkOption != -1)
                {
                    availables.Add(darkOption);
                }

                bool prefersLight = TonePrefersLightForeground(bgTone1) || TonePrefersLightForeground(bgTone2);
                if (prefersLight)
                {
                    return (lightOption == -1) ? 100 : lightOption;
                }
                if (availables.Count == 1)
                {
                    return availables[0];
                }
                return (darkOption == -1) ? 0 : darkOption;
            }

            return answer;
        }
    }
}
