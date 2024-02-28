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

using MaterialFoundation.MaterialColorUtilities.Utils;
using System;

namespace MaterialFoundation.MaterialColorUtilities.Contrast;

/// <summary>Color science for contrast utilities.
///
/// <para>Utility methods for calculating contrast given two colors, or calculating a color given one
/// color and a contrast ratio.</para>
///
/// <para>Contrast ratio is calculated using XYZ's Y. When linearized to match human perception, Y
/// becomes HCT's tone and L*a*b*'s' L*.</para></summary>
public static class Contrast
{
    /// <summary>The minimum contrast ratio of two colors.
    /// Contrast ratio equation = lighter + 5 / darker + 5, if lighter == darker, ratio == 1.</summary>
    public const double RatioMin = 1.0;

    /// <summary>The maximum contrast ratio of two colors.
    /// Contrast ratio equation = lighter + 5 / darker + 5. Lighter and darker scale from 0 to 100.
    /// If lighter == 100, darker = 0, ratio == 21.</summary>
    public const double RatioMax = 21.0;
    public const double Ratio3_0 = 3.0;
    public const double Ratio4_5 = 4.5;
    public const double Ratio7_0 = 7.0;

    /// <summary>Given a color and a contrast ratio to reach, the luminance of a color that reaches that ratio
    /// with the color can be calculated. However, that luminance may not contrast as desired, i.e. the
    /// contrast ratio of the input color and the returned luminance may not reach the contrast ratio
    /// asked for.
    ///
    /// When the desired contrast ratio and the result contrast ratio differ by more than this amount,
    /// an error value should be returned, or the method should be documented as 'unsafe', meaning,
    /// it will return a valid luminance but that luminance may not meet the requested contrast ratio.
    ///
    /// 0.04 selected because it ensures the resulting ratio rounds to the same tenth.</summary>
    private const double ConstantRatioEpsilon = 0.04;

    /// <summary>Color spaces that measure luminance, such as Y in XYZ, L* in L*a*b*, or T in HCT, are known as
    /// perceptually accurate color spaces.
    ///
    /// To be displayed, they must gamut map to a "display space", one that has a defined limit on the
    /// number of colors. Display spaces include sRGB, more commonly understood  as RGB/HSL/HSV/HSB.
    /// Gamut mapping is undefined and not defined by the color space. Any gamut mapping algorithm must
    /// choose how to sacrifice accuracy in hue, saturation, and/or lightness.
    ///
    /// A principled solution is to maintain lightness, thus maintaining contrast/a11y, maintain hue,
    /// thus maintaining aesthetic intent, and reduce chroma until the color is in gamut.
    ///
    /// HCT chooses this solution, but, that doesn't mean it will _exactly_ matched desired lightness,
    /// if only because RGB is quantized: RGB is expressed as a set of integers: there may be an RGB
    /// color with, for example, 47.892 lightness, but not 47.891.
    ///
    /// To allow for this inherent incompatibility between perceptually accurate color spaces and
    /// display color spaces, methods that take a contrast ratio and luminance, and return a luminance
    /// that reaches that contrast ratio for the input luminance, purposefully darken/lighten their
    /// result such that the desired contrast ratio will be reached even if inaccuracy is introduced.
    ///
    /// 0.4 is generous, ex. HCT requires much less delta. It was chosen because it provides a rough
    /// guarantee that as long as a perceptual color space gamut maps lightness such that the resulting
    /// lightness rounds to the same as the requested, the desired contrast ratio will be reached.</summary>
    private const double LuminanceGamutMapTolerance = 0.4;

    /// <summary>Contrast ratio is a measure of legibility, its used to compare the lightness of two colors.
    /// This method is used commonly in industry due to its use by WCAG.
    ///
    /// <para>To compare lightness, the colors are expressed in the XYZ color space, where Y is lightness,
    /// also known as relative luminance.</para>
    ///
    /// <para>The equation is ratio = lighter Y + 5 / darker Y + 5.</para></summary>
    public static double RatioOfYs(double y1, double y2)
    {
        double lighter = Math.Max(y1, y2);
        double darker = (lighter == y2) ? y1 : y2;
        return (lighter + 5.0) / (darker + 5.0);
    }

    ///  <summary>Contrast ratio of two tones. T in HCT, L* in L*a*b*. Also known as luminance or perpectual
    /// luminance.
    ///
    /// <para>Contrast ratio is defined using Y in XYZ, relative luminance. However, relative luminance is
    /// linear to number of photons, not to perception of lightness. Perceptual luminance, L* in
    /// L*a*b*, T in HCT, is. Designers prefer color spaces with perceptual luminance since they're
    /// accurate to the eye.</para>
    ///
    /// <para>Y and L* are pure functions of each other, so it possible to use perceptually accurate color
    /// spaces, and measure contrast, and measure contrast in a much more understandable way: instead
    /// of a ratio, a linear difference. This allows a designer to determine what they need to adjust a
    /// color's lightness to in order to reach their desired contrast, instead of guessing & checking
    /// with hex codes.</para></summary>
    public static double RatioOfTones(double t1, double t2)
    {
        return RatioOfYs(ColorUtils.YFromLstar(t1), ColorUtils.YFromLstar(t2));
    }

    /// <summary>Returns T in HCT, L* in L*a*b* >= tone parameter that ensures ratio with input T/L*. Returns -1
    /// if ratio cannot be achieved.</summary>
    /// <param name="tone">Tone return value must contrast with.</param>
    /// <param name="ratio">Desired contrast ratio of return value and tone parameter.</param>
    public static double Lighter(double tone, double ratio)
    {
        if (tone < 0.0 || tone > 100.0)
        {
            return -1.0;
        }
        // Invert the contrast ratio equation to determine lighter Y given a ratio and darker Y.
        double darkY = ColorUtils.YFromLstar(tone);
        double lightY = ratio * (darkY + 5.0) - 5.0;
        if (lightY < 0.0 || lightY > 100.0)
        {
            return -1.0;
        }
        double realContrast = RatioOfYs(lightY, darkY);
        double delta = Math.Abs(realContrast - ratio);
        if (realContrast < ratio && delta > ConstantRatioEpsilon)
        {
            return -1.0;
        }

        double returnValue = ColorUtils.LstarFromY(lightY) + LuminanceGamutMapTolerance;
        // NOMUTANTS--important validation step; functions it is calling may change implementation.
        if (returnValue < 0 || returnValue > 100)
        {
            return -1.0;
        }
        return returnValue;
    }

    /// <summary>Tone >= tone parameter that ensures ratio. 100 if ratio cannot be achieved.
    ///
    /// <para>This method is unsafe because the returned value is guaranteed to be in bounds, but, the in
    /// bounds return value may not reach the desired ratio.</para></summary>
    /// <param name="tone">Tone return value must contrast with.</param>
    /// <param name="ratio">Desired contrast ratio of return value and tone parameter.</param>
    public static double LighterUnsafe(double tone, double ratio)
    {
        double lighterSafe = Lighter(tone, ratio);
        return lighterSafe < 0.0 ? 100.0 : lighterSafe;
    }

    /// <summary>Returns T in HCT, L* in L*a*b* <= tone parameter that ensures ratio with input T/L*. Returns -1
    /// if ratio cannot be achieved.</summary>
    /// <param name="tone">Tone return value must contrast with.</param>
    /// <param name="ratio">Desired contrast ratio of return value and tone parameter.</param>
    public static double Darker(double tone, double ratio)
    {
        if (tone < 0.0 || tone > 100.0)
        {
            return -1.0;
        }
        // Invert the contrast ratio equation to determine darker Y given a ratio and lighter Y.
        double lightY = ColorUtils.YFromLstar(tone);
        double darkY = ((lightY + 5.0) / ratio) - 5.0;
        if (darkY < 0.0 || darkY > 100.0)
        {
            return -1.0;
        }
        double realContrast = RatioOfYs(lightY, darkY);
        double delta = Math.Abs(realContrast - ratio);
        if (realContrast < ratio && delta > ConstantRatioEpsilon)
        {
            return -1.0;
        }

        // For information on 0.4 constant, see comment in lighter(tone, ratio).
        double returnValue = ColorUtils.LstarFromY(darkY) - LuminanceGamutMapTolerance;
        // NOMUTANTS--important validation step; functions it is calling may change implementation.
        if (returnValue < 0 || returnValue > 100)
        {
            return -1.0;
        }
        return returnValue;
    }

    /// <summary>Tone <= tone parameter that ensures ratio. 0 if ratio cannot be achieved.
    ///
    /// <para>This method is unsafe because the returned value is guaranteed to be in bounds, but, the in
    /// bounds return value may not reach the desired ratio.</para></summary>
    /// <param name="tone">Tone return value must contrast with.</param>
    /// <param name="ratio">Desired contrast ratio of return value and tone parameter.</param>
    public static double DarkerUnsafe(double tone, double ratio)
    {
        double darkerSafe = Darker(tone, ratio);
        return Math.Max(0.0, darkerSafe);
    }
}
