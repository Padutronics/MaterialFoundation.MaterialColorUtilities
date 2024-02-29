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
using System.Collections.Generic;
using System.Linq;

namespace MaterialFoundation.MaterialColorUtilities.Temperature;

/// <summary>Design utilities using color temperature theory.
///
/// <para>Analogous colors, complementary color, and cache to efficiently, lazily, generate data for
/// calculations when needed.</para></summary>
public sealed class TemperatureCache
{
    private readonly Hct.Hct input;

    private Hct.Hct? precomputedComplement;
    private IList<Hct.Hct>? precomputedHctsByTemp;
    private IList<Hct.Hct>? precomputedHctsByHue;
    private IDictionary<Hct.Hct, double>? precomputedTempsByHct;

    /// <summary>Create a cache that allows calculation of ex. complementary and analogous colors.</summary>
    /// <param name="input">Color to find complement/analogous colors of. Any colors will have the same tone,
    /// and chroma as the input color, modulo any restrictions due to the other hues having lower
    /// limits on chroma.</param>
    public TemperatureCache(Hct.Hct input)
    {
        this.input = input;
    }

    /// <summary>A color that complements the input color aesthetically.
    ///
    /// <para>In art, this is usually described as being across the color wheel. History of this shows
    /// intent as a color that is just as cool-warm as the input color is warm-cool.</para></summary>
    public Hct.Hct GetComplement()
    {
        if (precomputedComplement != null)
        {
            return precomputedComplement;
        }

        double coldestHue = GetColdest().GetHue();
        double coldestTemp = GetTempsByHct()[GetColdest()];

        double warmestHue = GetWarmest().GetHue();
        double warmestTemp = GetTempsByHct()[GetWarmest()];
        double range = warmestTemp - coldestTemp;
        bool startHueIsColdestToWarmest = IsBetween(input.GetHue(), coldestHue, warmestHue);
        double startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
        double endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;
        double directionOfRotation = 1.0;
        double smallestError = 1000.0;
        Hct.Hct answer = GetHctsByHue()[(int)Math.Round(input.GetHue())];

        double complementRelativeTemp = 1.0 - GetRelativeTemperature(input);
        // Find the color in the other section, closest to the inverse percentile
        // of the input color. This is the complement.
        for (double hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0)
        {
            double hue = MathUtils.SanitizeDegreesDouble(startHue + directionOfRotation * hueAddend);
            if (!IsBetween(hue, startHue, endHue))
            {
                continue;
            }
            Hct.Hct possibleAnswer = GetHctsByHue()[(int)Math.Round(hue)];
            double relativeTemp = (GetTempsByHct()[possibleAnswer] - coldestTemp) / range;
            double error = Math.Abs(complementRelativeTemp - relativeTemp);
            if (error < smallestError)
            {
                smallestError = error;
                answer = possibleAnswer;
            }
        }
        precomputedComplement = answer;
        return precomputedComplement;
    }

    /// <summary>5 colors that pair well with the input color.
    ///
    /// <para>The colors are equidistant in temperature and adjacent in hue.</para></summary>
    public List<Hct.Hct> GetAnalogousColors()
    {
        return GetAnalogousColors(5, 12);
    }

    /// <summary>A set of colors with differing hues, equidistant in temperature.
    ///
    /// <para>In art, this is usually described as a set of 5 colors on a color wheel divided into 12
    /// sections. This method allows provision of either of those values.</para>
    ///
    /// <para>Behavior is undefined when count or divisions is 0. When divisions < count, colors repeat.</para></summary>
    /// <param name="count">The number of colors to return, includes the input color.</param>
    /// <param name="divisions">The number of divisions on the color wheel.</param>
    public List<Hct.Hct> GetAnalogousColors(int count, int divisions)
    {
        // The starting hue is the hue of the input color.
        int startHue = (int)Math.Round(input.GetHue());
        Hct.Hct startHct = GetHctsByHue()[startHue];
        double lastTemp = GetRelativeTemperature(startHct);

        var allColors = new List<Hct.Hct>
        {
            startHct
        };

        double absoluteTotalTempDelta = 0.0;
        for (int i = 0; i < 360; i++)
        {
            int hue = MathUtils.SanitizeDegreesInt(startHue + i);
            Hct.Hct hct = GetHctsByHue()[hue];
            double temp = GetRelativeTemperature(hct);
            double tempDelta = Math.Abs(temp - lastTemp);
            lastTemp = temp;
            absoluteTotalTempDelta += tempDelta;
        }

        int hueAddend = 1;
        double tempStep = absoluteTotalTempDelta / (double)divisions;
        double totalTempDelta = 0.0;
        lastTemp = GetRelativeTemperature(startHct);
        while (allColors.Count < divisions)
        {
            int hue = MathUtils.SanitizeDegreesInt(startHue + hueAddend);
            Hct.Hct hct = GetHctsByHue()[hue];
            double temp = GetRelativeTemperature(hct);
            double tempDelta = Math.Abs(temp - lastTemp);
            totalTempDelta += tempDelta;

            double desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
            bool indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
            int indexAddend = 1;
            // Keep adding this hue to the answers until its temperature is
            // insufficient. This ensures consistent behavior when there aren't
            // `divisions` discrete steps between 0 and 360 in hue with `tempStep`
            // delta in temperature between them.
            //
            // For example, white and black have no analogues: there are no other
            // colors at T100/T0. Therefore, they should just be added to the array
            // as answers.
            while (indexSatisfied && allColors.Count < divisions)
            {
                allColors.Add(hct);
                desiredTotalTempDeltaForIndex = (allColors.Count + indexAddend) * tempStep;
                indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                indexAddend++;
            }
            lastTemp = temp;
            hueAddend++;

            if (hueAddend > 360)
            {
                while (allColors.Count < divisions)
                {
                    allColors.Add(hct);
                }
                break;
            }
        }

        var answers = new List<Hct.Hct>
        {
            input
        };

        int ccwCount = (int)Math.Floor(((double)count - 1.0) / 2.0);
        for (int i = 1; i < ccwCount + 1; i++)
        {
            int index = 0 - i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index = index % allColors.Count;
            }
            answers.Insert(0, allColors[index]);
        }

        int cwCount = count - ccwCount - 1;
        for (int i = 1; i < cwCount + 1; i++)
        {
            int index = i;
            while (index < 0)
            {
                index = allColors.Count + index;
            }
            if (index >= allColors.Count)
            {
                index = index % allColors.Count;
            }
            answers.Add(allColors[index]);
        }

        return answers;
    }

    /// <summary>Temperature relative to all colors with the same chroma and tone.</summary>
    /// <param name="hct">HCT to find the relative temperature of.</param>
    /// <returns>Value on a scale from 0 to 1.</returns>
    public double GetRelativeTemperature(Hct.Hct hct)
    {
        double range = GetTempsByHct()[GetWarmest()] - GetTempsByHct()[GetColdest()];
        double differenceFromColdest = GetTempsByHct()[hct] - GetTempsByHct()[GetColdest()];
        // Handle when there's no difference in temperature between warmest and
        // coldest: for example, at T100, only one color is available, white.
        if (range == 0.0)
        {
            return 0.5;
        }
        return differenceFromColdest / range;
    }

    /// <summary>Value representing cool-warm factor of a color. Values below 0 are considered cool, above,
    /// warm.
    ///
    /// <para>Color science has researched emotion and harmony, which art uses to select colors. Warm-cool
    /// is the foundation of analogous and complementary colors. See: - Li-Chen Ou's Chapter 19 in
    /// Handbook of Color Psychology (2015). - Josef Albers' Interaction of Color chapters 19 and 21.</para>
    ///
    /// <para>Implementation of Ou, Woodcock and Wright's algorithm, which uses Lab/LCH color space.
    /// Return value has these properties:
    /// - Values below 0 are cool, above 0 are warm.
    /// - Lower bound: -9.66. Chroma is infinite. Assuming max of Lab chroma 130.
    /// - Upper bound: 8.61. Chroma is infinite. Assuming max of Lab chroma 130.</para></summary>
    public static double RawTemperature(Hct.Hct color)
    {
        double[] lab = ColorUtils.LabFromArgb(color.ToInt());
        double hue = MathUtils.SanitizeDegreesDouble(MathUtils.ToDegrees(Math.Atan2(lab[2], lab[1])));
        double chroma = double.Hypot(lab[1], lab[2]);
        return -0.5 + 0.02 * Math.Pow(chroma, 1.07) * Math.Cos(MathUtils.ToRadians(MathUtils.SanitizeDegreesDouble(hue - 50.0)));
    }

    /// <summary>Coldest color with same chroma and tone as input.</summary>
    private Hct.Hct GetColdest()
    {
        return GetHctsByTemp()[0];
    }

    /// <summary>HCTs for all colors with the same chroma/tone as the input.
    ///
    /// <para>Sorted by hue, ex. index 0 is hue 0.</para></summary>
    private IList<Hct.Hct> GetHctsByHue()
    {
        if (precomputedHctsByHue != null)
        {
            return precomputedHctsByHue;
        }
        var hcts = new List<Hct.Hct>();
        for (double hue = 0.0; hue <= 360.0; hue += 1.0)
        {
            Hct.Hct colorAtHue = Hct.Hct.From(hue, input.GetChroma(), input.GetTone());
            hcts.Add(colorAtHue);
        }
        precomputedHctsByHue = hcts.AsReadOnly();
        return precomputedHctsByHue;
    }

    /// <summary>HCTs for all colors with the same chroma/tone as the input.
    ///
    /// <para>Sorted from coldest first to warmest last.</para></summary>
    private IList<Hct.Hct> GetHctsByTemp()
    {
        if (precomputedHctsByTemp != null)
        {
            return precomputedHctsByTemp;
        }

        var hcts = new List<Hct.Hct>(GetHctsByHue())
        {
            input
        };
        hcts = hcts
            .Select(hct => new { hct, temp = GetTempsByHct()[hct] })
            .OrderBy(x => x.temp)
            .Select(x => x.hct)
            .ToList();
        precomputedHctsByTemp = hcts;
        return precomputedHctsByTemp;
    }

    /// <summary>Keys of HCTs in getHctsByTemp, values of raw temperature.</summary>
    private IDictionary<Hct.Hct, double> GetTempsByHct()
    {
        if (precomputedTempsByHct != null)
        {
            return precomputedTempsByHct;
        }

        var allHcts = new List<Hct.Hct>(GetHctsByHue())
        {
            input
        };

        var temperaturesByHct = new Dictionary<Hct.Hct, double>();
        foreach (Hct.Hct hct in allHcts)
        {
            temperaturesByHct.Add(hct, RawTemperature(hct));
        }

        precomputedTempsByHct = temperaturesByHct;
        return precomputedTempsByHct;
    }

    /// <summary>Warmest color with same chroma and tone as input.</summary>
    private Hct.Hct GetWarmest()
    {
        return GetHctsByTemp()[GetHctsByTemp().Count - 1];
    }

    /// <summary>Determines if an angle is between two other angles, rotating clockwise.</summary>
    private static bool IsBetween(double angle, double a, double b)
    {
        if (a < b)
        {
            return a <= angle && angle <= b;
        }
        return a <= angle || angle <= b;
    }
}
