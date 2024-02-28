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

/**
 * CAM16, a color appearance model. Colors are not just defined by their hex code, but rather, a hex
 * code and viewing conditions.
 *
 * <p>CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*, b*, or jstar,
 * astar, bstar in code. CAM16-UCS is included in the CAM16 specification, and should be used when
 * measuring distances between colors.
 *
 * <p>In traditional color spaces, a color can be identified solely by the observer's measurement of
 * the color. Color appearance models such as CAM16 also use information about the environment where
 * the color was observed, known as the viewing conditions.
 *
 * <p>For example, white under the traditional assumption of a midday sun white point is accurately
 * measured as a slightly chromatic blue by CAM16. (roughly, hue 203, chroma 3, lightness 100)
 */
public sealed class Cam16
{
    // Transforms XYZ color space coordinates to 'cone'/'RGB' responses in CAM16.
    public static readonly double[][] XYZ_TO_CAM16RGB = [
        [0.401288, 0.650173, -0.051461],
        [-0.250268, 1.204414, 0.045854],
        [-0.002079, 0.048952, 0.953127]
    ];

    // Transforms 'cone'/'RGB' responses in CAM16 to XYZ color space coordinates.
    private static readonly double[][] CAM16RGB_TO_XYZ = [
        [1.8620678, -1.0112547, 0.14918678],
        [0.38752654, 0.62144744, -0.00897398],
        [-0.01584150, -0.03412294, 1.0499644]
    ];

    // CAM16 color dimensions, see getters for documentation.
    private readonly double hue;
    private readonly double chroma;
    private readonly double j;
    private readonly double q;
    private readonly double m;
    private readonly double s;

    // Coordinates in UCS space. Used to determine color distance, like delta E equations in L*a*b*.
    private readonly double jstar;
    private readonly double astar;
    private readonly double bstar;

    // Avoid allocations during conversion by pre-allocating an array.
    private readonly double[] tempArray = new double[] { 0.0, 0.0, 0.0 };

    /**
     * CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*, b*, or jstar,
     * astar, bstar in code. CAM16-UCS is included in the CAM16 specification, and is used to measure
     * distances between colors.
     */
    public double distance(Cam16 other)
    {
        double dJ = getJstar() - other.getJstar();
        double dA = getAstar() - other.getAstar();
        double dB = getBstar() - other.getBstar();
        double dEPrime = Math.Sqrt(dJ * dJ + dA * dA + dB * dB);
        double dE = 1.41 * Math.Pow(dEPrime, 0.63);
        return dE;
    }

    /** Hue in CAM16 */
    public double getHue()
    {
        return hue;
    }

    /** Chroma in CAM16 */
    public double getChroma()
    {
        return chroma;
    }

    /** Lightness in CAM16 */
    public double getJ()
    {
        return j;
    }

    /**
     * Brightness in CAM16.
     *
     * <p>Prefer lightness, brightness is an absolute quantity. For example, a sheet of white paper is
     * much brighter viewed in sunlight than in indoor light, but it is the lightest object under any
     * lighting.
     */
    public double getQ()
    {
        return q;
    }

    /**
     * Colorfulness in CAM16.
     *
     * <p>Prefer chroma, colorfulness is an absolute quantity. For example, a yellow toy car is much
     * more colorful outside than inside, but it has the same chroma in both environments.
     */
    public double getM()
    {
        return m;
    }

    /**
     * Saturation in CAM16.
     *
     * <p>Colorfulness in proportion to brightness. Prefer chroma, saturation measures colorfulness
     * relative to the color's own brightness, where chroma is colorfulness relative to white.
     */
    public double getS()
    {
        return s;
    }

    /** Lightness coordinate in CAM16-UCS */
    public double getJstar()
    {
        return jstar;
    }

    /** a* coordinate in CAM16-UCS */
    public double getAstar()
    {
        return astar;
    }

    /** b* coordinate in CAM16-UCS */
    public double getBstar()
    {
        return bstar;
    }

    /**
     * All of the CAM16 dimensions can be calculated from 3 of the dimensions, in the following
     * combinations: - {j or q} and {c, m, or s} and hue - jstar, astar, bstar Prefer using a static
     * method that constructs from 3 of those dimensions. This constructor is intended for those
     * methods to use to return all possible dimensions.
     *
     * @param hue for example, red, orange, yellow, green, etc.
     * @param chroma informally, colorfulness / color intensity. like saturation in HSL, except
     *     perceptually accurate.
     * @param j lightness
     * @param q brightness; ratio of lightness to white point's lightness
     * @param m colorfulness
     * @param s saturation; ratio of chroma to white point's chroma
     * @param jstar CAM16-UCS J coordinate
     * @param astar CAM16-UCS a coordinate
     * @param bstar CAM16-UCS b coordinate
     */
    private Cam16(
        double hue,
        double chroma,
        double j,
        double q,
        double m,
        double s,
        double jstar,
        double astar,
        double bstar)
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

    /**
     * Create a CAM16 color from a color, assuming the color was viewed in default viewing conditions.
     *
     * @param argb ARGB representation of a color.
     */
    public static Cam16 fromInt(int argb)
    {
        return fromIntInViewingConditions(argb, ViewingConditions.DEFAULT);
    }

    /**
     * Create a CAM16 color from a color in defined viewing conditions.
     *
     * @param argb ARGB representation of a color.
     * @param viewingConditions Information about the environment where the color was observed.
     */
    // The RGB => XYZ conversion matrix elements are derived scientific constants. While the values
    // may differ at runtime due to floating point imprecision, keeping the values the same, and
    // accurate, across implementations takes precedence.
    static Cam16 fromIntInViewingConditions(int argb, ViewingConditions viewingConditions)
    {
        // Transform ARGB int to XYZ
        int red = (argb & 0x00ff0000) >> 16;
        int green = (argb & 0x0000ff00) >> 8;
        int blue = (argb & 0x000000ff);
        double redL = ColorUtils.linearized(red);
        double greenL = ColorUtils.linearized(green);
        double blueL = ColorUtils.linearized(blue);
        double x = 0.41233895 * redL + 0.35762064 * greenL + 0.18051042 * blueL;
        double y = 0.2126 * redL + 0.7152 * greenL + 0.0722 * blueL;
        double z = 0.01932141 * redL + 0.11916382 * greenL + 0.95034478 * blueL;

        return fromXyzInViewingConditions(x, y, z, viewingConditions);
    }

    public static Cam16 fromXyzInViewingConditions(
        double x, double y, double z, ViewingConditions viewingConditions)
    {
        // Transform XYZ to 'cone'/'rgb' responses
        double[][] matrix = XYZ_TO_CAM16RGB;
        double rT = (x * matrix[0][0]) + (y * matrix[0][1]) + (z * matrix[0][2]);
        double gT = (x * matrix[1][0]) + (y * matrix[1][1]) + (z * matrix[1][2]);
        double bT = (x * matrix[2][0]) + (y * matrix[2][1]) + (z * matrix[2][2]);

        // Discount illuminant
        double rD = viewingConditions.getRgbD()[0] * rT;
        double gD = viewingConditions.getRgbD()[1] * gT;
        double bD = viewingConditions.getRgbD()[2] * bT;

        // Chromatic adaptation
        double rAF = Math.Pow(viewingConditions.getFl() * Math.Abs(rD) / 100.0, 0.42);
        double gAF = Math.Pow(viewingConditions.getFl() * Math.Abs(gD) / 100.0, 0.42);
        double bAF = Math.Pow(viewingConditions.getFl() * Math.Abs(bD) / 100.0, 0.42);
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
        double atanDegrees = MathUtils.toDegrees(atan2);
        double hue =
            atanDegrees < 0
                ? atanDegrees + 360.0
                : atanDegrees >= 360 ? atanDegrees - 360.0 : atanDegrees;
        double hueRadians = MathUtils.toRadians(hue);

        // achromatic response to color
        double ac = p2 * viewingConditions.getNbb();

        // CAM16 lightness and brightness
        double j =
            100.0
                * Math.Pow(
                    ac / viewingConditions.getAw(),
                    viewingConditions.getC() * viewingConditions.getZ());
        double q =
            4.0
                / viewingConditions.getC()
                * Math.Sqrt(j / 100.0)
                * (viewingConditions.getAw() + 4.0)
                * viewingConditions.getFlRoot();

        // CAM16 chroma, colorfulness, and saturation.
        double huePrime = (hue < 20.14) ? hue + 360 : hue;
        double eHue = 0.25 * (Math.Cos(MathUtils.toRadians(huePrime) + 2.0) + 3.8);
        double p1 = 50000.0 / 13.0 * eHue * viewingConditions.getNc() * viewingConditions.getNcb();
        double t = p1 * double.Hypot(a, b) / (u + 0.305);
        double alpha =
            Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.getN()), 0.73) * Math.Pow(t, 0.9);
        // CAM16 chroma, colorfulness, saturation
        double c = alpha * Math.Sqrt(j / 100.0);
        double m = c * viewingConditions.getFlRoot();
        double s =
            50.0 * Math.Sqrt((alpha * viewingConditions.getC()) / (viewingConditions.getAw() + 4.0));

        // CAM16-UCS components
        double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
        double mstar = 1.0 / 0.0228 * double.LogP1(0.0228 * m);
        double astar = mstar * Math.Cos(hueRadians);
        double bstar = mstar * Math.Sin(hueRadians);

        return new Cam16(hue, c, j, q, m, s, jstar, astar, bstar);
    }

    /**
     * @param j CAM16 lightness
     * @param c CAM16 chroma
     * @param h CAM16 hue
     */
    static Cam16 fromJch(double j, double c, double h)
    {
        return fromJchInViewingConditions(j, c, h, ViewingConditions.DEFAULT);
    }

    /**
     * @param j CAM16 lightness
     * @param c CAM16 chroma
     * @param h CAM16 hue
     * @param viewingConditions Information about the environment where the color was observed.
     */
    private static Cam16 fromJchInViewingConditions(
        double j, double c, double h, ViewingConditions viewingConditions)
    {
        double q =
            4.0
                / viewingConditions.getC()
                * Math.Sqrt(j / 100.0)
                * (viewingConditions.getAw() + 4.0)
                * viewingConditions.getFlRoot();
        double m = c * viewingConditions.getFlRoot();
        double alpha = c / Math.Sqrt(j / 100.0);
        double s =
            50.0 * Math.Sqrt((alpha * viewingConditions.getC()) / (viewingConditions.getAw() + 4.0));

        double hueRadians = MathUtils.toRadians(h);
        double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
        double mstar = 1.0 / 0.0228 * double.LogP1(0.0228 * m);
        double astar = mstar * Math.Cos(hueRadians);
        double bstar = mstar * Math.Sin(hueRadians);
        return new Cam16(h, c, j, q, m, s, jstar, astar, bstar);
    }

    /**
     * Create a CAM16 color from CAM16-UCS coordinates.
     *
     * @param jstar CAM16-UCS lightness.
     * @param astar CAM16-UCS a dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the Y
     *     axis.
     * @param bstar CAM16-UCS b dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the X
     *     axis.
     */
    public static Cam16 fromUcs(double jstar, double astar, double bstar)
    {

        return fromUcsInViewingConditions(jstar, astar, bstar, ViewingConditions.DEFAULT);
    }

    /**
     * Create a CAM16 color from CAM16-UCS coordinates in defined viewing conditions.
     *
     * @param jstar CAM16-UCS lightness.
     * @param astar CAM16-UCS a dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the Y
     *     axis.
     * @param bstar CAM16-UCS b dimension. Like a* in L*a*b*, it is a Cartesian coordinate on the X
     *     axis.
     * @param viewingConditions Information about the environment where the color was observed.
     */
    public static Cam16 fromUcsInViewingConditions(
        double jstar, double astar, double bstar, ViewingConditions viewingConditions)
    {

        double m = double.Hypot(astar, bstar);
        double m2 = double.ExpM1(m * 0.0228) / 0.0228;
        double c = m2 / viewingConditions.getFlRoot();
        double h = Math.Atan2(bstar, astar) * (180.0 / Math.PI);
        if (h < 0.0)
        {
            h += 360.0;
        }
        double j = jstar / (1.0 - (jstar - 100.0) * 0.007);
        return fromJchInViewingConditions(j, c, h, viewingConditions);
    }

    /**
     * ARGB representation of the color. Assumes the color was viewed in default viewing conditions,
     * which are near-identical to the default viewing conditions for sRGB.
     */
    public int toInt()
    {
        return viewed(ViewingConditions.DEFAULT);
    }

    /**
     * ARGB representation of the color, in defined viewing conditions.
     *
     * @param viewingConditions Information about the environment where the color will be viewed.
     * @return ARGB representation of color
     */
    int viewed(ViewingConditions viewingConditions)
    {
        double[] xyz = xyzInViewingConditions(viewingConditions, tempArray);
        return ColorUtils.argbFromXyz(xyz[0], xyz[1], xyz[2]);
    }

    public double[] xyzInViewingConditions(ViewingConditions viewingConditions, double[]? returnArray)
    {
        double alpha =
            (getChroma() == 0.0 || getJ() == 0.0) ? 0.0 : getChroma() / Math.Sqrt(getJ() / 100.0);

        double t =
            Math.Pow(
                alpha / Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.getN()), 0.73), 1.0 / 0.9);
        double hRad = MathUtils.toRadians(getHue());

        double eHue = 0.25 * (Math.Cos(hRad + 2.0) + 3.8);
        double ac =
            viewingConditions.getAw()
                * Math.Pow(getJ() / 100.0, 1.0 / viewingConditions.getC() / viewingConditions.getZ());
        double p1 = eHue * (50000.0 / 13.0) * viewingConditions.getNc() * viewingConditions.getNcb();
        double p2 = (ac / viewingConditions.getNbb());

        double hSin = Math.Sin(hRad);
        double hCos = Math.Cos(hRad);

        double gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);
        double a = gamma * hCos;
        double b = gamma * hSin;
        double rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
        double gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
        double bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;

        double rCBase = Math.Max(0, (27.13 * Math.Abs(rA)) / (400.0 - Math.Abs(rA)));
        double rC =
            Math.Sign(rA) * (100.0 / viewingConditions.getFl()) * Math.Pow(rCBase, 1.0 / 0.42);
        double gCBase = Math.Max(0, (27.13 * Math.Abs(gA)) / (400.0 - Math.Abs(gA)));
        double gC =
            Math.Sign(gA) * (100.0 / viewingConditions.getFl()) * Math.Pow(gCBase, 1.0 / 0.42);
        double bCBase = Math.Max(0, (27.13 * Math.Abs(bA)) / (400.0 - Math.Abs(bA)));
        double bC =
            Math.Sign(bA) * (100.0 / viewingConditions.getFl()) * Math.Pow(bCBase, 1.0 / 0.42);
        double rF = rC / viewingConditions.getRgbD()[0];
        double gF = gC / viewingConditions.getRgbD()[1];
        double bF = bC / viewingConditions.getRgbD()[2];

        double[][] matrix = CAM16RGB_TO_XYZ;
        double x = (rF * matrix[0][0]) + (gF * matrix[0][1]) + (bF * matrix[0][2]);
        double y = (rF * matrix[1][0]) + (gF * matrix[1][1]) + (bF * matrix[1][2]);
        double z = (rF * matrix[2][0]) + (gF * matrix[2][1]) + (bF * matrix[2][2]);

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
