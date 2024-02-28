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

namespace MaterialFoundation.MaterialColorUtilities.Hct;

/// <summary>CAM16, a color appearance model. Colors are not just defined by their hex code, but rather, a hex
/// code and viewing conditions.
///
/// <para>CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*, b*, or jstar,
/// astar, bstar in code. CAM16-UCS is included in the CAM16 specification, and should be used when
/// measuring distances between colors.</para>
///
/// <para>In traditional color spaces, a color can be identified solely by the observer's measurement of
/// the color. Color appearance models such as CAM16 also use information about the environment where
/// the color was observed, known as the viewing conditions.</para>
///
/// <para>For example, white under the traditional assumption of a midday sun white point is accurately
/// measured as a slightly chromatic blue by CAM16. (roughly, hue 203, chroma 3, lightness 100)</para></summary>
public sealed class Cam16
{
    /// <summary>Transforms XYZ color space coordinates to 'cone'/'RGB' responses in CAM16.</summary>
    public static readonly double[][] XyzToCam16Rgb = [
        [0.401288, 0.650173, -0.051461],
        [-0.250268, 1.204414, 0.045854],
        [-0.002079, 0.048952, 0.953127]
    ];

    /// <summary>Transforms 'cone'/'RGB' responses in CAM16 to XYZ color space coordinates.</summary>
    private static readonly double[][] Cam16RgbToXyz = [
        [1.8620678, -1.0112547, 0.14918678],
        [0.38752654, 0.62144744, -0.00897398],
        [-0.01584150, -0.03412294, 1.0499644]
    ];

    /// <summary>CAM16 color dimensions, see getters for documentation.</summary>
    private readonly double hue;
    private readonly double chroma;
    private readonly double j;
    private readonly double q;
    private readonly double m;
    private readonly double s;

    /// <summary>Coordinates in UCS space. Used to determine color distance, like delta E equations in L*a*b*.</summary>
    private readonly double jstar;
    private readonly double astar;
    private readonly double bstar;

    /// <summary>Avoid allocations during conversion by pre-allocating an array.</summary>
    private readonly double[] tempArray = new double[] { 0.0, 0.0, 0.0 };

    /// <summary>CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*, b*, or jstar,
    /// astar, bstar in code. CAM16-UCS is included in the CAM16 specification, and is used to measure
    /// distances between colors.</summary>
    public double Distance(Cam16 other)
    {
        double dJ = GetJstar() - other.GetJstar();
        double dA = GetAstar() - other.GetAstar();
        double dB = GetBstar() - other.GetBstar();
        double dEPrime = Math.Sqrt(dJ * dJ + dA * dA + dB * dB);
        double dE = 1.41 * Math.Pow(dEPrime, 0.63);
        return dE;
    }

    /// <summary>Hue in CAM16</summary>
    public double GetHue()
    {
        return hue;
    }

    /// <summary>Chroma in CAM16</summary>
    public double GetChroma()
    {
        return chroma;
    }

    /// <summary>Lightness in CAM16</summary>
    public double GetJ()
    {
        return j;
    }

    /// <summary>Brightness in CAM16.
    ///
    /// <para>Prefer lightness, brightness is an absolute quantity. For example, a sheet of white paper is
    /// much brighter viewed in sunlight than in indoor light, but it is the lightest object under any
    /// lighting.</para></summary>
    public double GetQ()
    {
        return q;
    }

    /// <summary>Colorfulness in CAM16.
    ///
    /// <para>Prefer chroma, colorfulness is an absolute quantity. For example, a yellow toy car is much
    /// more colorful outside than inside, but it has the same chroma in both environments.</para></summary>
    public double GetM()
    {
        return m;
    }

    /// <summary>Saturation in CAM16.
    ///
    /// <para>Colorfulness in proportion to brightness. Prefer chroma, saturation measures colorfulness
    /// relative to the color's own brightness, where chroma is colorfulness relative to white.</para></summary>
    public double GetS()
    {
        return s;
    }

    /// <summary>Lightness coordinate in CAM16-UCS</summary>
    public double GetJstar()
    {
        return jstar;
    }

    /// <summary>a* coordinate in CAM16-UCS</summary>
    public double GetAstar()
    {
        return astar;
    }

    /// <summary>b* coordinate in CAM16-UCS</summary>
    public double GetBstar()
    {
        return bstar;
    }

    /// <summary>All of the CAM16 dimensions can be calculated from 3 of the dimensions, in the following
    /// combinations: - {j or q} and {c, m, or s} and hue - jstar, astar, bstar Prefer using a static
    /// method that constructs from 3 of those dimensions. This constructor is intended for those
    /// methods to use to return all possible dimensions.</summary>
    /// <param name="hue">for example, red, orange, yellow, green, etc.</param>
    /// <param name="chroma">informally, colorfulness / color intensity. like saturation in HSL, except
    /// perceptually accurate.</param>
    /// <param name="j">lightness</param>
    /// <param name="q">brightness; ratio of lightness to white point's lightness</param>
    /// <param name="m">colorfulness</param>
    /// <param name="s">saturation; ratio of chroma to white point's chroma</param>
    /// <param name="jstar">CAM16-UCS J coordinate</param>
    /// <param name="astar">CAM16-UCS a coordinate</param>
    /// <param name="bstar">CAM16-UCS b coordinate</param>
    private Cam16(double hue, double chroma, double j, double q, double m, double s, double jstar, double astar, double bstar)
    {
        this.hue = hue;
        this.chroma = chroma;
        this.j = j;
        this.q = q;
        this.m = m;
        this.s = s;
        this.jstar = jstar;
        this.astar = astar;
        this.bstar = bstar;
    }

    /// <summary>Create a CAM16 color from a color, assuming the color was viewed in default viewing conditions.</summary>
    /// <param name="argb">ARGB representation of a color.</param>
    public static Cam16 FromInt(int argb)
    {
        return FromIntInViewingConditions(argb, ViewingConditions.Default);
    }

    /// <summary>Create a CAM16 color from a color in defined viewing conditions.</summary>
    /// <param name="argb">ARGB representation of a color.</param>
    /// <param name="viewingConditions">Information about the environment where the color was observed.</param>
    /// <remarks>The RGB => XYZ conversion matrix elements are derived scientific constants. While the values
    /// may differ at runtime due to floating point imprecision, keeping the values the same, and
    /// accurate, across implementations takes precedence.</remarks>
    private static Cam16 FromIntInViewingConditions(int argb, ViewingConditions viewingConditions)
    {
        // Transform ARGB int to XYZ
        int red = (argb & 0x00ff0000) >> 16;
        int green = (argb & 0x0000ff00) >> 8;
        int blue = argb & 0x000000ff;
        double redL = ColorUtils.Linearized(red);
        double greenL = ColorUtils.Linearized(green);
        double blueL = ColorUtils.Linearized(blue);
        double x = 0.41233895 * redL + 0.35762064 * greenL + 0.18051042 * blueL;
        double y = 0.2126 * redL + 0.7152 * greenL + 0.0722 * blueL;
        double z = 0.01932141 * redL + 0.11916382 * greenL + 0.95034478 * blueL;

        return FromXyzInViewingConditions(x, y, z, viewingConditions);
    }

    public static Cam16 FromXyzInViewingConditions(double x, double y, double z, ViewingConditions viewingConditions)
    {
        // Transform XYZ to 'cone'/'rgb' responses
        double[][] matrix = XyzToCam16Rgb;
        double rT = x * matrix[0][0] + y * matrix[0][1] + z * matrix[0][2];
        double gT = x * matrix[1][0] + y * matrix[1][1] + z * matrix[1][2];
        double bT = x * matrix[2][0] + y * matrix[2][1] + z * matrix[2][2];

        // Discount illuminant
        double rD = viewingConditions.GetRgbD()[0] * rT;
        double gD = viewingConditions.GetRgbD()[1] * gT;
        double bD = viewingConditions.GetRgbD()[2] * bT;

        // Chromatic adaptation
        double rAF = Math.Pow(viewingConditions.GetFl() * Math.Abs(rD) / 100.0, 0.42);
        double gAF = Math.Pow(viewingConditions.GetFl() * Math.Abs(gD) / 100.0, 0.42);
        double bAF = Math.Pow(viewingConditions.GetFl() * Math.Abs(bD) / 100.0, 0.42);
        double rA = Math.Sign(rD) * 400.0 * rAF / (rAF + 27.13);
        double gA = Math.Sign(gD) * 400.0 * gAF / (gAF + 27.13);
        double bA = Math.Sign(bD) * 400.0 * bAF / (bAF + 27.13);

        // redness-greenness
        double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
        // yellowness-blueness
        double b = (rA + gA - 2.0 * bA) / 9.0;

        // auxiliary components
        double u = (20.0 * rA + 20.0 * gA + 21.0 * bA) / 20.0;
        double p2 = (40.0 * rA + 20.0 * gA + bA) / 20.0;

        // hue
        double atan2 = Math.Atan2(b, a);
        double atanDegrees = MathUtils.ToDegrees(atan2);
        double hue = atanDegrees < 0
            ? atanDegrees + 360.0
            : atanDegrees >= 360 ? atanDegrees - 360.0 : atanDegrees;
        double hueRadians = MathUtils.ToRadians(hue);

        // achromatic response to color
        double ac = p2 * viewingConditions.GetNbb();

        // CAM16 lightness and brightness
        double j = 100.0 * Math.Pow(ac / viewingConditions.GetAw(), viewingConditions.GetC() * viewingConditions.GetZ());
        double q = 4.0 / viewingConditions.GetC() * Math.Sqrt(j / 100.0) * (viewingConditions.GetAw() + 4.0) * viewingConditions.GetFlRoot();

        // CAM16 chroma, colorfulness, and saturation.
        double huePrime = (hue < 20.14) ? hue + 360 : hue;
        double eHue = 0.25 * (Math.Cos(MathUtils.ToRadians(huePrime) + 2.0) + 3.8);
        double p1 = 50000.0 / 13.0 * eHue * viewingConditions.GetNc() * viewingConditions.GetNcb();
        double t = p1 * double.Hypot(a, b) / (u + 0.305);
        double alpha = Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.GetN()), 0.73) * Math.Pow(t, 0.9);
        // CAM16 chroma, colorfulness, saturation
        double c = alpha * Math.Sqrt(j / 100.0);
        double m = c * viewingConditions.GetFlRoot();
        double s = 50.0 * Math.Sqrt(alpha * viewingConditions.GetC() / (viewingConditions.GetAw() + 4.0));

        // CAM16-UCS components
        double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
        double mstar = 1.0 / 0.0228 * double.LogP1(0.0228 * m);
        double astar = mstar * Math.Cos(hueRadians);
        double bstar = mstar * Math.Sin(hueRadians);

        return new Cam16(hue, c, j, q, m, s, jstar, astar, bstar);
    }

    /// <param name="j">CAM16 lightness</param>
    /// <param name="c">CAM16 chroma</param>
    /// <param name="h">CAM16 hue</param>
    private static Cam16 FromJch(double j, double c, double h)
    {
        return FromJchInViewingConditions(j, c, h, ViewingConditions.Default);
    }

    /// <param name="j">CAM16 lightness</param>
    /// <param name="c">CAM16 chroma</param>
    /// <param name="h">CAM16 hue</param>
    /// <param name="viewingConditions">Information about the environment where the color was observed.</param>
    private static Cam16 FromJchInViewingConditions(double j, double c, double h, ViewingConditions viewingConditions)
    {
        double q = 4.0 / viewingConditions.GetC() * Math.Sqrt(j / 100.0) * (viewingConditions.GetAw() + 4.0) * viewingConditions.GetFlRoot();
        double m = c * viewingConditions.GetFlRoot();
        double alpha = c / Math.Sqrt(j / 100.0);
        double s = 50.0 * Math.Sqrt(alpha * viewingConditions.GetC() / (viewingConditions.GetAw() + 4.0));

        double hueRadians = MathUtils.ToRadians(h);
        double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
        double mstar = 1.0 / 0.0228 * double.LogP1(0.0228 * m);
        double astar = mstar * Math.Cos(hueRadians);
        double bstar = mstar * Math.Sin(hueRadians);
        return new Cam16(h, c, j, q, m, s, jstar, astar, bstar);
    }

    /// <summary>Create a CAM16 color from CAM16-UCS coordinates.</summary>
    /// <param name="jstar">CAM16-UCS lightness.</param>
    /// <param name="astar">CAM16-UCS a dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the Y
    /// axis.</param>
    /// <param name="bstar">CAM16-UCS b dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the X
    /// axis.</param>
    public static Cam16 FromUcs(double jstar, double astar, double bstar)
    {
        return FromUcsInViewingConditions(jstar, astar, bstar, ViewingConditions.Default);
    }

    /// <summary>Create a CAM16 color from CAM16-UCS coordinates in defined viewing conditions.</summary>
    /// <param name="jstar">CAM16-UCS lightness.</param>
    /// <param name="astar">CAM16-UCS a dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the Y
    /// axis.</param>
    /// <param name="bstar">CAM16-UCS b dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the X
    /// axis.</param>
    /// <param name="viewingConditions">Information about the environment where the color was observed.</param>
    public static Cam16 FromUcsInViewingConditions(double jstar, double astar, double bstar, ViewingConditions viewingConditions)
    {
        double m = double.Hypot(astar, bstar);
        double m2 = double.ExpM1(m * 0.0228) / 0.0228;
        double c = m2 / viewingConditions.GetFlRoot();
        double h = Math.Atan2(bstar, astar) * (180.0 / Math.PI);
        if (h < 0.0)
        {
            h += 360.0;
        }
        double j = jstar / (1.0 - (jstar - 100.0) * 0.007);
        return FromJchInViewingConditions(j, c, h, viewingConditions);
    }

    /// <summary>ARGB representation of the color. Assumes the color was viewed in default viewing conditions,
    /// which are near-identical to the default viewing conditions for sRGB.</summary>
    public int ToInt()
    {
        return Viewed(ViewingConditions.Default);
    }

    /// <summary>ARGB representation of the color, in defined viewing conditions.</summary>
    /// <param name="viewingConditions">Information about the environment where the color will be viewed.</param>
    /// <returns>ARGB representation of color</returns>
    private int Viewed(ViewingConditions viewingConditions)
    {
        double[] xyz = XyzInViewingConditions(viewingConditions, tempArray);
        return ColorUtils.ArgbFromXyz(xyz[0], xyz[1], xyz[2]);
    }

    public double[] XyzInViewingConditions(ViewingConditions viewingConditions, double[]? returnArray)
    {
        double alpha = (GetChroma() == 0.0 || GetJ() == 0.0) ? 0.0 : GetChroma() / Math.Sqrt(GetJ() / 100.0);

        double t = Math.Pow(alpha / Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.GetN()), 0.73), 1.0 / 0.9);
        double hRad = MathUtils.ToRadians(GetHue());

        double eHue = 0.25 * (Math.Cos(hRad + 2.0) + 3.8);
        double ac = viewingConditions.GetAw() * Math.Pow(GetJ() / 100.0, 1.0 / viewingConditions.GetC() / viewingConditions.GetZ());
        double p1 = eHue * (50000.0 / 13.0) * viewingConditions.GetNc() * viewingConditions.GetNcb();
        double p2 = ac / viewingConditions.GetNbb();

        double hSin = Math.Sin(hRad);
        double hCos = Math.Cos(hRad);

        double gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);
        double a = gamma * hCos;
        double b = gamma * hSin;
        double rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
        double gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
        double bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;

        double rCBase = Math.Max(0, 27.13 * Math.Abs(rA) / (400.0 - Math.Abs(rA)));
        double rC = Math.Sign(rA) * (100.0 / viewingConditions.GetFl()) * Math.Pow(rCBase, 1.0 / 0.42);
        double gCBase = Math.Max(0, 27.13 * Math.Abs(gA) / (400.0 - Math.Abs(gA)));
        double gC = Math.Sign(gA) * (100.0 / viewingConditions.GetFl()) * Math.Pow(gCBase, 1.0 / 0.42);
        double bCBase = Math.Max(0, 27.13 * Math.Abs(bA) / (400.0 - Math.Abs(bA)));
        double bC = Math.Sign(bA) * (100.0 / viewingConditions.GetFl()) * Math.Pow(bCBase, 1.0 / 0.42);
        double rF = rC / viewingConditions.GetRgbD()[0];
        double gF = gC / viewingConditions.GetRgbD()[1];
        double bF = bC / viewingConditions.GetRgbD()[2];

        double[][] matrix = Cam16RgbToXyz;
        double x = rF * matrix[0][0] + gF * matrix[0][1] + bF * matrix[0][2];
        double y = rF * matrix[1][0] + gF * matrix[1][1] + bF * matrix[1][2];
        double z = rF * matrix[2][0] + gF * matrix[2][1] + bF * matrix[2][2];

        if (returnArray != null)
        {
            returnArray[0] = x;
            returnArray[1] = y;
            returnArray[2] = z;
            return returnArray;
        }
        else
        {
            return new double[] { x, y, z };
        }
    }
}
