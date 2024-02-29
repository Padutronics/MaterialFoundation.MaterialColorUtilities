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

namespace MaterialFoundation.MaterialColorUtilities.Hct;

/// <summary>A color system built using CAM16 hue and chroma, and L* from L*a*b*.
///
/// <para>Using L* creates a link between the color system, contrast, and thus accessibility. Contrast
/// ratio depends on relative luminance, or Y in the XYZ color space. L*, or perceptual luminance can
/// be calculated from Y.</para>
///
/// <para>Unlike Y, L* is linear to human perception, allowing trivial creation of accurate color tones.</para>
///
/// <para>Unlike contrast ratio, measuring contrast in L* is linear, and simple to calculate. A
/// difference of 40 in HCT tone guarantees a contrast ratio >= 3.0, and a difference of 50
/// guarantees a contrast ratio >= 4.5.</para>
///
/// <para>HCT, hue, chroma, and tone. A color system that provides a perceptually accurate color
/// measurement system that can also accurately render what colors will appear as in different
/// lighting environments.</para></summary>
public sealed class Hct
{
    private int argb;

    private Hct(int argb)
    {
        SetInternalState(argb);
    }

    public double Hue { get; private set; }

    public double Chroma { get; private set; }

    public double Tone { get; private set; }

    /// <summary>Create an HCT color from hue, chroma, and tone.</summary>
    /// <param name="hue">0 <= hue < 360; invalid values are corrected.</param>
    /// <param name="chroma">0 <= chroma < ?; Informally, colorfulness. The color returned may be lower than
    /// the requested chroma. Chroma has a different maximum for any given hue and tone.</param>
    /// <param name="tone">0 <= tone <= 100; invalid values are corrected.</param>
    /// <returns>HCT representation of a color in default viewing conditions.</returns>
    public static Hct From(double hue, double chroma, double tone)
    {
        int argb = HctSolver.SolveToInt(hue, chroma, tone);
        return new Hct(argb);
    }

    /// <summary>Create an HCT color from a color.</summary>
    /// <param name="argb">ARGB representation of a color.</param>
    /// <returns>HCT representation of a color in default viewing conditions</returns>
    public static Hct FromInt(int argb)
    {
        return new Hct(argb);
    }

    public int ToInt()
    {
        return argb;
    }

    /// <summary>Set the hue of this color. Chroma may decrease because chroma has a different maximum for any
    /// given hue and tone.</summary>
    /// <param name="newHue">0 <= newHue < 360; invalid values are corrected.</param>
    public void SetHue(double newHue)
    {
        SetInternalState(HctSolver.SolveToInt(newHue, Chroma, Tone));
    }

    /// <summary>Set the chroma of this color. Chroma may decrease because chroma has a different maximum for
    /// any given hue and tone.</summary>
    /// <param name="newChroma">0 <= newChroma < ?</param>
    public void SetChroma(double newChroma)
    {
        SetInternalState(HctSolver.SolveToInt(Hue, newChroma, Tone));
    }

    /// <summary>Set the tone of this color. Chroma may decrease because chroma has a different maximum for any
    /// given hue and tone.</summary>
    /// <param name="newTone">0 <= newTone <= 100; invalid valids are corrected.</param>
    public void SetTone(double newTone)
    {
        SetInternalState(HctSolver.SolveToInt(Hue, Chroma, newTone));
    }

    /// <summary>Translate a color into different ViewingConditions.
    ///
    /// <para>Colors change appearance. They look different with lights on versus off, the same color, as
    /// in hex code, on white looks different when on black. This is called color relativity, most
    /// famously explicated by Josef Albers in Interaction of Color.</para>
    ///
    /// <para>In color science, color appearance models can account for this and calculate the appearance
    /// of a color in different settings. HCT is based on CAM16, a color appearance model, and uses it
    /// to make these calculations.</para>
    ///
    /// <para>See ViewingConditions.make for parameters affecting color appearance.</para></summary>
    public Hct InViewingConditions(ViewingConditions vc)
    {
        // 1. Use CAM16 to find XYZ coordinates of color in specified VC.
        Cam16 cam16 = Cam16.FromInt(ToInt());
        double[] viewedInVc = cam16.XyzInViewingConditions(vc, null);

        // 2. Create CAM16 of those XYZ coordinates in default VC.
        Cam16 recastInVc = Cam16.FromXyzInViewingConditions(viewedInVc[0], viewedInVc[1], viewedInVc[2], ViewingConditions.Default);

        // 3. Create HCT from:
        // - CAM16 using default VC with XYZ coordinates in specified VC.
        // - L* converted from Y in XYZ coordinates in specified VC.
        return From(recastInVc.Hue, recastInVc.Chroma, ColorUtils.LstarFromY(viewedInVc[1]));
    }

    private void SetInternalState(int argb)
    {
        this.argb = argb;
        Cam16 cam = Cam16.FromInt(argb);
        Hue = cam.Hue;
        Chroma = cam.Chroma;
        Tone = ColorUtils.LstarFromArgb(argb);
    }
}
