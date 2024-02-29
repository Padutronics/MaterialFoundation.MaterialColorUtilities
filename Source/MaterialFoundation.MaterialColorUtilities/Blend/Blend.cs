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

using MaterialFoundation.MaterialColorUtilities.Hct;
using MaterialFoundation.MaterialColorUtilities.Utils;
using System;

namespace MaterialFoundation.MaterialColorUtilities.Blend;

/// <summary>Functions for blending in HCT and CAM16.</summary>
public static class Blend
{
    /// <summary>Blend the design color's HCT hue towards the key color's HCT hue, in a way that leaves the
    /// original color recognizable and recognizably shifted towards the key color.</summary>
    /// <param name="designColor">ARGB representation of an arbitrary color.</param>
    /// <param name="sourceColor">ARGB representation of the main theme color.</param>
    /// <returns>The design color with a hue shifted towards the system's color, a slightly
    /// warmer/cooler variant of the design color's hue.</returns>
    public static int Harmonize(int designColor, int sourceColor)
    {
        Hct.Hct fromHct = Hct.Hct.FromInt(designColor);
        Hct.Hct toHct = Hct.Hct.FromInt(sourceColor);
        double differenceDegrees = MathUtils.DifferenceDegrees(fromHct.Hue, toHct.Hue);
        double rotationDegrees = Math.Min(differenceDegrees * 0.5, 15.0);
        double outputHue = MathUtils.SanitizeDegreesDouble(fromHct.Hue + rotationDegrees * MathUtils.RotationDirection(fromHct.Hue, toHct.Hue));
        return Hct.Hct.From(outputHue, fromHct.Chroma, fromHct.Tone).ToInt();
    }

    /// <summary>Blends hue from one color into another. The chroma and tone of the original color are
    /// maintained.</summary>
    /// <param name="from">ARGB representation of color</param>
    /// <param name="to">ARGB representation of color</param>
    /// <param name="amount">how much blending to perform; 0.0 >= and <= 1.0</param>
    /// <returns>from, with a hue blended towards to. Chroma and tone are constant.</returns>
    public static int HctHue(int from, int to, double amount)
    {
        int ucs = Cam16Ucs(from, to, amount);
        Cam16 ucsCam = Cam16.FromInt(ucs);
        Cam16 fromCam = Cam16.FromInt(from);
        Hct.Hct blended = Hct.Hct.From(ucsCam.Hue, fromCam.Chroma, ColorUtils.LstarFromArgb(from));
        return blended.ToInt();
    }

    /// <summary>Blend in CAM16-UCS space.</summary>
    /// <param name="from">ARGB representation of color</param>
    /// <param name="to">ARGB representation of color</param>
    /// <param name="amount">how much blending to perform; 0.0 >= and <= 1.0</param>
    /// <returns>from, blended towards to. Hue, chroma, and tone will change.</returns>
    public static int Cam16Ucs(int from, int to, double amount)
    {
        Cam16 fromCam = Cam16.FromInt(from);
        Cam16 toCam = Cam16.FromInt(to);
        double fromJ = fromCam.Jstar;
        double fromA = fromCam.Astar;
        double fromB = fromCam.Bstar;
        double toJ = toCam.Jstar;
        double toA = toCam.Astar;
        double toB = toCam.Bstar;
        double jstar = fromJ + (toJ - fromJ) * amount;
        double astar = fromA + (toA - fromA) * amount;
        double bstar = fromB + (toB - fromB) * amount;
        return Cam16.FromUcs(jstar, astar, bstar).ToInt();
    }
}
