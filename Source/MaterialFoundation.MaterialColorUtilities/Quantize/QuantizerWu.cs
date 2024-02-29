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

namespace MaterialFoundation.MaterialColorUtilities.Quantize;

/// <summary>An image quantizer that divides the image's pixels into clusters by recursively cutting an RGB
/// cube, based on the weight of pixels in each area of the cube.
///
/// <para>The algorithm was described by Xiaolin Wu in Graphic Gems II, published in 1991.</para></summary>
public sealed class QuantizerWu : IQuantizer
{
    /// <summary>A histogram of all the input colors is constructed. It has the shape of a
    /// cube. The cube would be too large if it contained all 16 million colors:
    /// historical best practice is to use 5 bits  of the 8 in each channel,
    /// reducing the histogram to a volume of ~32,000.</summary>
    private const int IndexBits = 5;
    private const int IndexCount = 33; // ((1 << IndexBits) + 1)
    private const int TotalSize = 35937; // IndexCount * IndexCount * IndexCount

    private int[] weights = [];
    private int[] momentsR = [];
    private int[] momentsG = [];
    private int[] momentsB = [];
    private double[] moments = [];
    private Box[] cubes = [];

    private static int GetIndex(int r, int g, int b)
    {
        return (r << (IndexBits * 2)) + (r << (IndexBits + 1)) + r + (g << IndexBits) + g + b;
    }

    private static int Volume(Box cube, int[] moment)
    {
        return moment[GetIndex(cube.r1, cube.g1, cube.b1)] -
            moment[GetIndex(cube.r1, cube.g1, cube.b0)] -
            moment[GetIndex(cube.r1, cube.g0, cube.b1)] +
            moment[GetIndex(cube.r1, cube.g0, cube.b0)] -
            moment[GetIndex(cube.r0, cube.g1, cube.b1)] +
            moment[GetIndex(cube.r0, cube.g1, cube.b0)] +
            moment[GetIndex(cube.r0, cube.g0, cube.b1)] -
            moment[GetIndex(cube.r0, cube.g0, cube.b0)];
    }

    private static int Bottom(Box cube, Direction direction, int[] moment)
    {
        return direction switch
        {
            Direction.Red => -moment[GetIndex(cube.r0, cube.g1, cube.b1)] +
                moment[GetIndex(cube.r0, cube.g1, cube.b0)] +
                moment[GetIndex(cube.r0, cube.g0, cube.b1)] -
                moment[GetIndex(cube.r0, cube.g0, cube.b0)],
            Direction.Green => -moment[GetIndex(cube.r1, cube.g0, cube.b1)] +
                moment[GetIndex(cube.r1, cube.g0, cube.b0)] +
                moment[GetIndex(cube.r0, cube.g0, cube.b1)] -
                moment[GetIndex(cube.r0, cube.g0, cube.b0)],
            Direction.Blue => -moment[GetIndex(cube.r1, cube.g1, cube.b0)] +
                moment[GetIndex(cube.r1, cube.g0, cube.b0)] +
                moment[GetIndex(cube.r0, cube.g1, cube.b0)] -
                moment[GetIndex(cube.r0, cube.g0, cube.b0)],
            _ => throw new ArgumentException("unexpected direction " + direction),
        };
    }

    private static int Top(Box cube, Direction direction, int position, int[] moment)
    {
        return direction switch
        {
            Direction.Red => moment[GetIndex(position, cube.g1, cube.b1)] -
                moment[GetIndex(position, cube.g1, cube.b0)] -
                moment[GetIndex(position, cube.g0, cube.b1)] +
                moment[GetIndex(position, cube.g0, cube.b0)],
            Direction.Green => moment[GetIndex(cube.r1, position, cube.b1)] -
                moment[GetIndex(cube.r1, position, cube.b0)] -
                moment[GetIndex(cube.r0, position, cube.b1)] +
                moment[GetIndex(cube.r0, position, cube.b0)],
            Direction.Blue => moment[GetIndex(cube.r1, cube.g1, position)] -
                moment[GetIndex(cube.r1, cube.g0, position)] -
                moment[GetIndex(cube.r0, cube.g1, position)] +
                moment[GetIndex(cube.r0, cube.g0, position)],
            _ => throw new ArgumentException("unexpected direction " + direction),
        };
    }

    public QuantizerResult Quantize(int[] pixels, int colorCount)
    {
        QuantizerResult mapResult = new QuantizerMap().Quantize(pixels, colorCount);
        ConstructHistogram(mapResult.ColorToCount);
        CreateMoments();
        CreateBoxesResult createBoxesResult = CreateBoxes(colorCount);
        ICollection<int> colors = CreateResult(createBoxesResult.resultCount);
        var resultMap = new Dictionary<int, int>();
        foreach (int color in colors)
        {
            resultMap.Add(color, 0);
        }
        return new QuantizerResult(resultMap);
    }

    private void ConstructHistogram(IDictionary<int, int> pixels)
    {
        weights = new int[TotalSize];
        momentsR = new int[TotalSize];
        momentsG = new int[TotalSize];
        momentsB = new int[TotalSize];
        moments = new double[TotalSize];

        foreach (KeyValuePair<int, int> pair in pixels)
        {
            int pixel = pair.Key;
            int count = pair.Value;
            int red = ColorUtils.RedFromArgb(pixel);
            int green = ColorUtils.GreenFromArgb(pixel);
            int blue = ColorUtils.BlueFromArgb(pixel);
            int bitsToRemove = 8 - IndexBits;
            int iR = (red >> bitsToRemove) + 1;
            int iG = (green >> bitsToRemove) + 1;
            int iB = (blue >> bitsToRemove) + 1;
            int index = GetIndex(iR, iG, iB);
            weights[index] += count;
            momentsR[index] += red * count;
            momentsG[index] += green * count;
            momentsB[index] += blue * count;
            moments[index] += count * (red * red + green * green + blue * blue);
        }
    }

    private void CreateMoments()
    {
        for (int r = 1; r < IndexCount; ++r)
        {
            int[] area = new int[IndexCount];
            int[] areaR = new int[IndexCount];
            int[] areaG = new int[IndexCount];
            int[] areaB = new int[IndexCount];
            double[] area2 = new double[IndexCount];

            for (int g = 1; g < IndexCount; ++g)
            {
                int line = 0;
                int lineR = 0;
                int lineG = 0;
                int lineB = 0;
                double line2 = 0.0;
                for (int b = 1; b < IndexCount; ++b)
                {
                    int index = GetIndex(r, g, b);
                    line += weights[index];
                    lineR += momentsR[index];
                    lineG += momentsG[index];
                    lineB += momentsB[index];
                    line2 += moments[index];

                    area[b] += line;
                    areaR[b] += lineR;
                    areaG[b] += lineG;
                    areaB[b] += lineB;
                    area2[b] += line2;

                    int previousIndex = GetIndex(r - 1, g, b);
                    weights[index] = weights[previousIndex] + area[b];
                    momentsR[index] = momentsR[previousIndex] + areaR[b];
                    momentsG[index] = momentsG[previousIndex] + areaG[b];
                    momentsB[index] = momentsB[previousIndex] + areaB[b];
                    moments[index] = moments[previousIndex] + area2[b];
                }
            }
        }
    }

    private CreateBoxesResult CreateBoxes(int maxColorCount)
    {
        cubes = new Box[maxColorCount];
        for (int i = 0; i < maxColorCount; i++)
        {
            cubes[i] = new Box();
        }
        double[] volumeVariance = new double[maxColorCount];
        Box firstBox = cubes[0];
        firstBox.r1 = IndexCount - 1;
        firstBox.g1 = IndexCount - 1;
        firstBox.b1 = IndexCount - 1;

        int generatedColorCount = maxColorCount;
        int next = 0;
        for (int i = 1; i < maxColorCount; i++)
        {
            if (Cut(cubes[next], cubes[i]))
            {
                volumeVariance[next] = (cubes[next].vol > 1) ? Variance(cubes[next]) : 0.0;
                volumeVariance[i] = (cubes[i].vol > 1) ? Variance(cubes[i]) : 0.0;
            }
            else
            {
                volumeVariance[next] = 0.0;
                i--;
            }

            next = 0;

            double temp = volumeVariance[0];
            for (int j = 1; j <= i; j++)
            {
                if (volumeVariance[j] > temp)
                {
                    temp = volumeVariance[j];
                    next = j;
                }
            }
            if (temp <= 0.0)
            {
                generatedColorCount = i + 1;
                break;
            }
        }

        return new CreateBoxesResult(maxColorCount, generatedColorCount);
    }

    private ICollection<int> CreateResult(int colorCount)
    {
        var colors = new List<int>();
        for (int i = 0; i < colorCount; ++i)
        {
            Box cube = cubes[i];
            int weight = Volume(cube, weights);
            if (weight > 0)
            {
                int r = Volume(cube, momentsR) / weight;
                int g = Volume(cube, momentsG) / weight;
                int b = Volume(cube, momentsB) / weight;
                int color = (255 << 24) | ((r & 0x0ff) << 16) | ((g & 0x0ff) << 8) | (b & 0x0ff);
                colors.Add(color);
            }
        }
        return colors;
    }

    private double Variance(Box cube)
    {
        int dr = Volume(cube, momentsR);
        int dg = Volume(cube, momentsG);
        int db = Volume(cube, momentsB);
        double xx = moments[GetIndex(cube.r1, cube.g1, cube.b1)] -
            moments[GetIndex(cube.r1, cube.g1, cube.b0)] -
            moments[GetIndex(cube.r1, cube.g0, cube.b1)] +
            moments[GetIndex(cube.r1, cube.g0, cube.b0)] -
            moments[GetIndex(cube.r0, cube.g1, cube.b1)] +
            moments[GetIndex(cube.r0, cube.g1, cube.b0)] +
            moments[GetIndex(cube.r0, cube.g0, cube.b1)] -
            moments[GetIndex(cube.r0, cube.g0, cube.b0)];

        int hypotenuse = dr * dr + dg * dg + db * db;
        int volume = Volume(cube, weights);
        return xx - hypotenuse / (double)volume;
    }

    private bool Cut(Box one, Box two)
    {
        int wholeR = Volume(one, momentsR);
        int wholeG = Volume(one, momentsG);
        int wholeB = Volume(one, momentsB);
        int wholeW = Volume(one, weights);

        MaximizeResult maxRResult = Maximize(one, Direction.Red, one.r0 + 1, one.r1, wholeR, wholeG, wholeB, wholeW);
        MaximizeResult maxGResult = Maximize(one, Direction.Green, one.g0 + 1, one.g1, wholeR, wholeG, wholeB, wholeW);
        MaximizeResult maxBResult = Maximize(one, Direction.Blue, one.b0 + 1, one.b1, wholeR, wholeG, wholeB, wholeW);
        Direction cutDirection;
        double maxR = maxRResult.maximum;
        double maxG = maxGResult.maximum;
        double maxB = maxBResult.maximum;
        if (maxR >= maxG && maxR >= maxB)
        {
            if (maxRResult.cutLocation < 0)
            {
                return false;
            }
            cutDirection = Direction.Red;
        }
        else if (maxG >= maxR && maxG >= maxB)
        {
            cutDirection = Direction.Green;
        }
        else
        {
            cutDirection = Direction.Blue;
        }

        two.r1 = one.r1;
        two.g1 = one.g1;
        two.b1 = one.b1;

        switch (cutDirection)
        {
            case Direction.Red:
                one.r1 = maxRResult.cutLocation;
                two.r0 = one.r1;
                two.g0 = one.g0;
                two.b0 = one.b0;
                break;
            case Direction.Green:
                one.g1 = maxGResult.cutLocation;
                two.r0 = one.r0;
                two.g0 = one.g1;
                two.b0 = one.b0;
                break;
            case Direction.Blue:
                one.b1 = maxBResult.cutLocation;
                two.r0 = one.r0;
                two.g0 = one.g0;
                two.b0 = one.b1;
                break;
        }

        one.vol = (one.r1 - one.r0) * (one.g1 - one.g0) * (one.b1 - one.b0);
        two.vol = (two.r1 - two.r0) * (two.g1 - two.g0) * (two.b1 - two.b0);

        return true;
    }

    private MaximizeResult Maximize(Box cube, Direction direction, int first, int last, int wholeR, int wholeG, int wholeB, int wholeW)
    {
        int bottomR = Bottom(cube, direction, momentsR);
        int bottomG = Bottom(cube, direction, momentsG);
        int bottomB = Bottom(cube, direction, momentsB);
        int bottomW = Bottom(cube, direction, weights);

        double max = 0.0;
        int cut = -1;

        int halfR = 0;
        int halfG = 0;
        int halfB = 0;
        int halfW = 0;
        for (int i = first; i < last; i++)
        {
            halfR = bottomR + Top(cube, direction, i, momentsR);
            halfG = bottomG + Top(cube, direction, i, momentsG);
            halfB = bottomB + Top(cube, direction, i, momentsB);
            halfW = bottomW + Top(cube, direction, i, weights);
            if (halfW == 0)
            {
                continue;
            }

            double tempNumerator = halfR * halfR + halfG * halfG + halfB * halfB;
            double tempDenominator = halfW;
            double temp = tempNumerator / tempDenominator;

            halfR = wholeR - halfR;
            halfG = wholeG - halfG;
            halfB = wholeB - halfB;
            halfW = wholeW - halfW;
            if (halfW == 0)
            {
                continue;
            }

            tempNumerator = halfR * halfR + halfG * halfG + halfB * halfB;
            tempDenominator = halfW;
            temp += tempNumerator / tempDenominator;

            if (temp > max)
            {
                max = temp;
                cut = i;
            }
        }
        return new MaximizeResult(cut, max);
    }

    private enum Direction
    {
        Red,
        Green,
        Blue
    }

    private sealed class MaximizeResult
    {
        // < 0 if cut impossible
        public int cutLocation;
        public double maximum;

        public MaximizeResult(int cut, double max)
        {
            cutLocation = cut;
            maximum = max;
        }
    }

    private sealed class CreateBoxesResult
    {
        public int requestedCount;
        public int resultCount;

        public CreateBoxesResult(int requestedCount, int resultCount)
        {
            this.requestedCount = requestedCount;
            this.resultCount = resultCount;
        }
    }

    private sealed class Box
    {
        public int r0 = 0;
        public int r1 = 0;
        public int g0 = 0;
        public int g1 = 0;
        public int b0 = 0;
        public int b1 = 0;
        public int vol = 0;
    }
}
