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

/**
 * An image quantizer that divides the image's pixels into clusters by recursively cutting an RGB
 * cube, based on the weight of pixels in each area of the cube.
 *
 * <p>The algorithm was described by Xiaolin Wu in Graphic Gems II, published in 1991.
 */
public sealed class QuantizerWu : Quantizer
{
    int[] weights = [];
    int[] momentsR = [];
    int[] momentsG = [];
    int[] momentsB = [];
    double[] moments = [];
    Box[] cubes = [];

    // A histogram of all the input colors is constructed. It has the shape of a
    // cube. The cube would be too large if it contained all 16 million colors:
    // historical best practice is to use 5 bits  of the 8 in each channel,
    // reducing the histogram to a volume of ~32,000.
    private const int INDEX_BITS = 5;
    private const int INDEX_COUNT = 33; // ((1 << INDEX_BITS) + 1)
    private const int TOTAL_SIZE = 35937; // INDEX_COUNT * INDEX_COUNT * INDEX_COUNT

    public QuantizerResult quantize(int[] pixels, int colorCount)
    {
        QuantizerResult mapResult = new QuantizerMap().quantize(pixels, colorCount);
        constructHistogram(mapResult.colorToCount);
        createMoments();
        CreateBoxesResult createBoxesResult = createBoxes(colorCount);
        ICollection<int> colors = createResult(createBoxesResult.resultCount);
        var resultMap = new Dictionary<int, int>();
        foreach (int color in colors)
        {
            resultMap.Add(color, 0);
        }
        return new QuantizerResult(resultMap);
    }

    static int getIndex(int r, int g, int b)
    {
        return (r << (INDEX_BITS * 2)) + (r << (INDEX_BITS + 1)) + r + (g << INDEX_BITS) + g + b;
    }

    void constructHistogram(IDictionary<int, int> pixels)
    {
        weights = new int[TOTAL_SIZE];
        momentsR = new int[TOTAL_SIZE];
        momentsG = new int[TOTAL_SIZE];
        momentsB = new int[TOTAL_SIZE];
        moments = new double[TOTAL_SIZE];

        foreach (KeyValuePair<int, int> pair in pixels)
        {
            int pixel = pair.Key;
            int count = pair.Value;
            int red = ColorUtils.redFromArgb(pixel);
            int green = ColorUtils.greenFromArgb(pixel);
            int blue = ColorUtils.blueFromArgb(pixel);
            int bitsToRemove = 8 - INDEX_BITS;
            int iR = (red >> bitsToRemove) + 1;
            int iG = (green >> bitsToRemove) + 1;
            int iB = (blue >> bitsToRemove) + 1;
            int index = getIndex(iR, iG, iB);
            weights[index] += count;
            momentsR[index] += (red * count);
            momentsG[index] += (green * count);
            momentsB[index] += (blue * count);
            moments[index] += (count * ((red * red) + (green * green) + (blue * blue)));
        }
    }

    void createMoments()
    {
        for (int r = 1; r < INDEX_COUNT; ++r)
        {
            int[] area = new int[INDEX_COUNT];
            int[] areaR = new int[INDEX_COUNT];
            int[] areaG = new int[INDEX_COUNT];
            int[] areaB = new int[INDEX_COUNT];
            double[] area2 = new double[INDEX_COUNT];

            for (int g = 1; g < INDEX_COUNT; ++g)
            {
                int line = 0;
                int lineR = 0;
                int lineG = 0;
                int lineB = 0;
                double line2 = 0.0;
                for (int b = 1; b < INDEX_COUNT; ++b)
                {
                    int index = getIndex(r, g, b);
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

                    int previousIndex = getIndex(r - 1, g, b);
                    weights[index] = weights[previousIndex] + area[b];
                    momentsR[index] = momentsR[previousIndex] + areaR[b];
                    momentsG[index] = momentsG[previousIndex] + areaG[b];
                    momentsB[index] = momentsB[previousIndex] + areaB[b];
                    moments[index] = moments[previousIndex] + area2[b];
                }
            }
        }
    }

    CreateBoxesResult createBoxes(int maxColorCount)
    {
        cubes = new Box[maxColorCount];
        for (int i = 0; i < maxColorCount; i++)
        {
            cubes[i] = new Box();
        }
        double[] volumeVariance = new double[maxColorCount];
        Box firstBox = cubes[0];
        firstBox.r1 = INDEX_COUNT - 1;
        firstBox.g1 = INDEX_COUNT - 1;
        firstBox.b1 = INDEX_COUNT - 1;

        int generatedColorCount = maxColorCount;
        int next = 0;
        for (int i = 1; i < maxColorCount; i++)
        {
            if (cut(cubes[next], cubes[i]))
            {
                volumeVariance[next] = (cubes[next].vol > 1) ? variance(cubes[next]) : 0.0;
                volumeVariance[i] = (cubes[i].vol > 1) ? variance(cubes[i]) : 0.0;
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

    ICollection<int> createResult(int colorCount)
    {
        var colors = new List<int>();
        for (int i = 0; i < colorCount; ++i)
        {
            Box cube = cubes[i];
            int weight = volume(cube, weights);
            if (weight > 0)
            {
                int r = volume(cube, momentsR) / weight;
                int g = volume(cube, momentsG) / weight;
                int b = volume(cube, momentsB) / weight;
                int color = (255 << 24) | ((r & 0x0ff) << 16) | ((g & 0x0ff) << 8) | (b & 0x0ff);
                colors.Add(color);
            }
        }
        return colors;
    }

    double variance(Box cube)
    {
        int dr = QuantizerWu.volume(cube, momentsR);
        int dg = QuantizerWu.volume(cube, momentsG);
        int db = QuantizerWu.volume(cube, momentsB);
        double xx = moments[getIndex(cube.r1, cube.g1, cube.b1)] -
            moments[getIndex(cube.r1, cube.g1, cube.b0)] -
            moments[getIndex(cube.r1, cube.g0, cube.b1)] +
            moments[getIndex(cube.r1, cube.g0, cube.b0)] -
            moments[getIndex(cube.r0, cube.g1, cube.b1)] +
            moments[getIndex(cube.r0, cube.g1, cube.b0)] +
            moments[getIndex(cube.r0, cube.g0, cube.b1)] -
            moments[getIndex(cube.r0, cube.g0, cube.b0)];

        int hypotenuse = dr * dr + dg * dg + db * db;
        int volume = QuantizerWu.volume(cube, weights);
        return xx - hypotenuse / ((double)volume);
    }

    bool cut(Box one, Box two)
    {
        int wholeR = volume(one, momentsR);
        int wholeG = volume(one, momentsG);
        int wholeB = volume(one, momentsB);
        int wholeW = volume(one, weights);

        MaximizeResult maxRResult = maximize(one, Direction.RED, one.r0 + 1, one.r1, wholeR, wholeG, wholeB, wholeW);
        MaximizeResult maxGResult = maximize(one, Direction.GREEN, one.g0 + 1, one.g1, wholeR, wholeG, wholeB, wholeW);
        MaximizeResult maxBResult = maximize(one, Direction.BLUE, one.b0 + 1, one.b1, wholeR, wholeG, wholeB, wholeW);
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
            cutDirection = Direction.RED;
        }
        else if (maxG >= maxR && maxG >= maxB)
        {
            cutDirection = Direction.GREEN;
        }
        else
        {
            cutDirection = Direction.BLUE;
        }

        two.r1 = one.r1;
        two.g1 = one.g1;
        two.b1 = one.b1;

        switch (cutDirection)
        {
            case Direction.RED:
                one.r1 = maxRResult.cutLocation;
                two.r0 = one.r1;
                two.g0 = one.g0;
                two.b0 = one.b0;
                break;
            case Direction.GREEN:
                one.g1 = maxGResult.cutLocation;
                two.r0 = one.r0;
                two.g0 = one.g1;
                two.b0 = one.b0;
                break;
            case Direction.BLUE:
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

    MaximizeResult maximize(Box cube, Direction direction, int first, int last, int wholeR, int wholeG, int wholeB, int wholeW)
    {
        int bottomR = bottom(cube, direction, momentsR);
        int bottomG = bottom(cube, direction, momentsG);
        int bottomB = bottom(cube, direction, momentsB);
        int bottomW = bottom(cube, direction, weights);

        double max = 0.0;
        int cut = -1;

        int halfR = 0;
        int halfG = 0;
        int halfB = 0;
        int halfW = 0;
        for (int i = first; i < last; i++)
        {
            halfR = bottomR + top(cube, direction, i, momentsR);
            halfG = bottomG + top(cube, direction, i, momentsG);
            halfB = bottomB + top(cube, direction, i, momentsB);
            halfW = bottomW + top(cube, direction, i, weights);
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
            temp += (tempNumerator / tempDenominator);

            if (temp > max)
            {
                max = temp;
                cut = i;
            }
        }
        return new MaximizeResult(cut, max);
    }

    static int volume(Box cube, int[] moment)
    {
        return (moment[getIndex(cube.r1, cube.g1, cube.b1)] -
            moment[getIndex(cube.r1, cube.g1, cube.b0)] -
            moment[getIndex(cube.r1, cube.g0, cube.b1)] +
            moment[getIndex(cube.r1, cube.g0, cube.b0)] -
            moment[getIndex(cube.r0, cube.g1, cube.b1)] +
            moment[getIndex(cube.r0, cube.g1, cube.b0)] +
            moment[getIndex(cube.r0, cube.g0, cube.b1)] -
            moment[getIndex(cube.r0, cube.g0, cube.b0)]);
    }

    static int bottom(Box cube, Direction direction, int[] moment)
    {
        switch (direction)
        {
            case Direction.RED:
                return -moment[getIndex(cube.r0, cube.g1, cube.b1)] +
                    moment[getIndex(cube.r0, cube.g1, cube.b0)] +
                    moment[getIndex(cube.r0, cube.g0, cube.b1)] -
                    moment[getIndex(cube.r0, cube.g0, cube.b0)];
            case Direction.GREEN:
                return -moment[getIndex(cube.r1, cube.g0, cube.b1)] +
                    moment[getIndex(cube.r1, cube.g0, cube.b0)] +
                    moment[getIndex(cube.r0, cube.g0, cube.b1)] -
                    moment[getIndex(cube.r0, cube.g0, cube.b0)];
            case Direction.BLUE:
                return -moment[getIndex(cube.r1, cube.g1, cube.b0)] +
                    moment[getIndex(cube.r1, cube.g0, cube.b0)] +
                    moment[getIndex(cube.r0, cube.g1, cube.b0)] -
                    moment[getIndex(cube.r0, cube.g0, cube.b0)];
        }
        throw new ArgumentException("unexpected direction " + direction);
    }

    static int top(Box cube, Direction direction, int position, int[] moment)
    {
        switch (direction)
        {
            case Direction.RED:
                return (moment[getIndex(position, cube.g1, cube.b1)] -
                    moment[getIndex(position, cube.g1, cube.b0)] -
                    moment[getIndex(position, cube.g0, cube.b1)] +
                    moment[getIndex(position, cube.g0, cube.b0)]);
            case Direction.GREEN:
                return (moment[getIndex(cube.r1, position, cube.b1)] -
                    moment[getIndex(cube.r1, position, cube.b0)] -
                    moment[getIndex(cube.r0, position, cube.b1)] +
                    moment[getIndex(cube.r0, position, cube.b0)]);
            case Direction.BLUE:
                return (moment[getIndex(cube.r1, cube.g1, position)] -
                    moment[getIndex(cube.r1, cube.g0, position)] -
                    moment[getIndex(cube.r0, cube.g1, position)] +
                    moment[getIndex(cube.r0, cube.g0, position)]);
        }
        throw new ArgumentException("unexpected direction " + direction);
    }

    private enum Direction
    {
        RED,
        GREEN,
        BLUE
    }

    private sealed class MaximizeResult
    {
        // < 0 if cut impossible
        public int cutLocation;
        public double maximum;

        public MaximizeResult(int cut, double max)
        {
            this.cutLocation = cut;
            this.maximum = max;
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
