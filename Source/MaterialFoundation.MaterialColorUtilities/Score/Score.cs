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

namespace MaterialFoundation.MaterialColorUtilities.Score;

/// <summary>Given a large set of colors, remove colors that are unsuitable for a UI theme, and rank the rest
/// based on suitability.
///
/// <para>Enables use of a high cluster count for image quantization, thus ensuring colors aren't
/// muddied, while curating the high cluster count to a much smaller number of appropriate choices.</para></summary>
public static class Score
{
    private const double TargetChroma = 48.0; // A1 Chroma
    private const double WeightProportion = 0.7;
    private const double WeightChromaAbove = 0.3;
    private const double WeightChromaBelow = 0.1;
    private const double CutoffChroma = 5.0;
    private const double CutoffExcitedProportion = 0.01;

    public static ICollection<int> GetScore(IDictionary<int, int> colorsToPopulation)
    {
        // Fallback color is Google Blue.
        return GetScore(colorsToPopulation, 4, unchecked((int)0xff4285f4), true);
    }

    public static ICollection<int> GetScore(IDictionary<int, int> colorsToPopulation, int desired)
    {
        return GetScore(colorsToPopulation, desired, unchecked((int)0xff4285f4), true);
    }

    public static ICollection<int> GetScore(IDictionary<int, int> colorsToPopulation, int desired, int fallbackColorArgb)
    {
        return GetScore(colorsToPopulation, desired, fallbackColorArgb, true);
    }

    /// <summary>Given a map with keys of colors and values of how often the color appears, rank the colors
    /// based on suitability for being used for a UI theme.</summary>
    /// <param name="colorsToPopulation">map with keys of colors and values of how often the color appears,
    /// usually from a source image.</param>
    /// <param name="desired">max count of colors to be returned in the list.</param>
    /// <param name="fallbackColorArgb">color to be returned if no other options available.</param>
    /// <param name="filter">whether to filter out undesireable combinations.</param>
    /// <returns>Colors sorted by suitability for a UI theme. The most suitable color is the first item,
    /// the least suitable is the last. There will always be at least one color returned. If all
    /// the input colors were not suitable for a theme, a default fallback color will be provided,
    /// Google Blue.</returns>
    public static ICollection<int> GetScore(IDictionary<int, int> colorsToPopulation, int desired, int fallbackColorArgb, bool filter)
    {
        // Get the HCT color for each Argb value, while finding the per hue count and
        // total count.
        var colorsHct = new List<Hct.Hct>();
        int[] huePopulation = new int[360];
        double populationSum = 0.0;
        foreach (KeyValuePair<int, int> entry in colorsToPopulation)
        {
            Hct.Hct hct = Hct.Hct.FromInt(entry.Key);
            colorsHct.Add(hct);
            int hue = (int)Math.Floor(hct.Hue);
            huePopulation[hue] += entry.Value;
            populationSum += entry.Value;
        }

        // Hues with more usage in neighboring 30 degree slice get a larger number.
        double[] hueExcitedProportions = new double[360];
        for (int hue = 0; hue < 360; hue++)
        {
            double proportion = huePopulation[hue] / populationSum;
            for (int i = hue - 14; i < hue + 16; i++)
            {
                int neighborHue = MathUtils.SanitizeDegreesInt(i);
                hueExcitedProportions[neighborHue] += proportion;
            }
        }

        // Scores each HCT color based on usage and chroma, while optionally
        // filtering out values that do not have enough chroma or usage.
        var scoredHcts = new List<ScoredHct>();
        foreach (Hct.Hct hct in colorsHct)
        {
            int hue = MathUtils.SanitizeDegreesInt((int)Math.Round(hct.Hue));
            double proportion = hueExcitedProportions[hue];
            if (filter && (hct.Chroma < CutoffChroma || proportion <= CutoffExcitedProportion))
            {
                continue;
            }

            double proportionScore = proportion * 100.0 * WeightProportion;
            double chromaWeight = hct.Chroma < TargetChroma ? WeightChromaBelow : WeightChromaAbove;
            double chromaScore = (hct.Chroma - TargetChroma) * chromaWeight;
            double score = proportionScore + chromaScore;
            scoredHcts.Add(new ScoredHct(hct, score));
        }
        // Sorted so that colors with higher scores come first.
        scoredHcts.Sort(new ScoredComparator());

        // Iterates through potential hue differences in degrees in order to select
        // the colors with the largest distribution of hues possible. Starting at
        // 90 degrees(maximum difference for 4 colors) then decreasing down to a
        // 15 degree minimum.
        var chosenColors = new List<Hct.Hct>();
        for (int differenceDegrees = 90; differenceDegrees >= 15; differenceDegrees--)
        {
            chosenColors.Clear();
            foreach (ScoredHct entry in scoredHcts)
            {
                Hct.Hct hct = entry.hct;
                bool hasDuplicateHue = false;
                foreach (Hct.Hct chosenHct in chosenColors)
                {
                    if (MathUtils.DifferenceDegrees(hct.Hue, chosenHct.Hue) < differenceDegrees)
                    {
                        hasDuplicateHue = true;
                        break;
                    }
                }
                if (!hasDuplicateHue)
                {
                    chosenColors.Add(hct);
                }
                if (chosenColors.Count >= desired)
                {
                    break;
                }
            }
            if (chosenColors.Count >= desired)
            {
                break;
            }
        }
        var colors = new List<int>();
        if (chosenColors.Count == 0)
        {
            colors.Add(fallbackColorArgb);
        }
        foreach (Hct.Hct chosenHct in chosenColors)
        {
            colors.Add(chosenHct.ToInt());
        }
        return colors;
    }

    private sealed class ScoredHct
    {
        public readonly Hct.Hct hct;
        public readonly double score;

        public ScoredHct(Hct.Hct hct, double score)
        {
            this.hct = hct;
            this.score = score;
        }
    }

    private sealed class ScoredComparator : IComparer<ScoredHct>
    {
        public int Compare(ScoredHct? entry1, ScoredHct? entry2)
        {
            return entry2!.score.CompareTo(entry1!.score);
        }
    }
}
