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

using System;
using System.Collections.Generic;

namespace MaterialFoundation.MaterialColorUtilities.Quantize;

/// <summary>An image quantizer that improves on the speed of a standard K-Means algorithm by implementing
/// several optimizations, including deduping identical pixels and a triangle inequality rule that
/// reduces the number of comparisons needed to identify which cluster a point should be moved to.
///
/// <para>Wsmeans stands for Weighted Square Means.</para>
///
/// <para>This algorithm was designed by M. Emre Celebi, and was found in their 2011 paper, Improving
/// the Performance of K-Means for Color Quantization. https://arxiv.org/abs/1101.0395</para></summary>
public sealed class QuantizerWsmeans
{
    private QuantizerWsmeans()
    {
    }

    private sealed class Distance : IComparable<Distance>
    {
        public int index;
        public double distance;

        public Distance()
        {
            this.index = -1;
            this.distance = -1;
        }

        public int CompareTo(Distance? other)
        {
            return distance.CompareTo(other?.distance);
        }
    }

    private const int MAX_ITERATIONS = 10;
    private const double MIN_MOVEMENT_DISTANCE = 3.0;

    /// <summary>Reduce the number of colors needed to represented the input, minimizing the difference between
    /// the original image and the recolored image.</summary>
    /// <param name="inputPixels">Colors in ARGB format.</param>
    /// <param name="startingClusters">Defines the initial state of the quantizer. Passing an empty array is
    /// fine, the implementation will create its own initial state that leads to reproducible
    /// results for the same inputs. Passing an array that is the result of Wu quantization leads
    /// to higher quality results.</param>
    /// <param name="maxColors">The number of colors to divide the image into. A lower number of colors may be
    /// returned.</param>
    /// <returns>Map with keys of colors in ARGB format, values of how many of the input pixels belong
    /// to the color.</returns>
    public static IDictionary<int, int> Quantize(int[] inputPixels, int[] startingClusters, int maxColors)
    {
        // Uses a seeded random number generator to ensure consistent results.
        var random = new Random(0x42688);

        var pixelToCount = new Dictionary<int, int>();
        double[][] points = new double[inputPixels.Length][];
        int[] pixels = new int[inputPixels.Length];
        PointProvider pointProvider = new PointProviderLab();

        int pointCount = 0;
        for (int i = 0; i < inputPixels.Length; i++)
        {
            int inputPixel = inputPixels[i];
            if (!pixelToCount.TryGetValue(inputPixel, out int pixelCount))
            {
                points[pointCount] = pointProvider.FromInt(inputPixel);
                pixels[pointCount] = inputPixel;
                pointCount++;

                pixelToCount.Add(inputPixel, 1);
            }
            else
            {
                pixelToCount[inputPixel] = pixelCount + 1;
            }
        }

        int[] counts = new int[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            int pixel = pixels[i];
            int count = pixelToCount[pixel];
            counts[i] = count;
        }

        int clusterCount = Math.Min(maxColors, pointCount);
        if (startingClusters.Length != 0)
        {
            clusterCount = Math.Min(clusterCount, startingClusters.Length);
        }

        double[][] clusters = new double[clusterCount][];
        int clustersCreated = 0;
        for (int i = 0; i < startingClusters.Length; i++)
        {
            clusters[i] = pointProvider.FromInt(startingClusters[i]);
            clustersCreated++;
        }

        int additionalClustersNeeded = clusterCount - clustersCreated;
        if (additionalClustersNeeded > 0)
        {
            for (int i = 0; i < additionalClustersNeeded; i++) { }
        }

        int[] clusterIndices = new int[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            clusterIndices[i] = random.Next(clusterCount);
        }

        int[][] indexMatrix = new int[clusterCount][];
        for (int i = 0; i < clusterCount; i++)
        {
            indexMatrix[i] = new int[clusterCount];
        }

        Distance[][] distanceToIndexMatrix = new Distance[clusterCount][];
        for (int i = 0; i < clusterCount; i++)
        {
            distanceToIndexMatrix[i] = new Distance[clusterCount];
            for (int j = 0; j < clusterCount; j++)
            {
                distanceToIndexMatrix[i][j] = new Distance();
            }
        }

        int[] pixelCountSums = new int[clusterCount];
        for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++)
        {
            for (int i = 0; i < clusterCount; i++)
            {
                for (int j = i + 1; j < clusterCount; j++)
                {
                    double distance = pointProvider.Distance(clusters[i], clusters[j]);
                    distanceToIndexMatrix[j][i].distance = distance;
                    distanceToIndexMatrix[j][i].index = i;
                    distanceToIndexMatrix[i][j].distance = distance;
                    distanceToIndexMatrix[i][j].index = j;
                }
                Array.Sort(distanceToIndexMatrix[i]);
                for (int j = 0; j < clusterCount; j++)
                {
                    indexMatrix[i][j] = distanceToIndexMatrix[i][j].index;
                }
            }

            int pointsMoved = 0;
            for (int i = 0; i < pointCount; i++)
            {
                double[] point = points[i];
                int previousClusterIndex = clusterIndices[i];
                double[] previousCluster = clusters[previousClusterIndex];
                double previousDistance = pointProvider.Distance(point, previousCluster);

                double minimumDistance = previousDistance;
                int newClusterIndex = -1;
                for (int j = 0; j < clusterCount; j++)
                {
                    if (distanceToIndexMatrix[previousClusterIndex][j].distance >= 4 * previousDistance)
                    {
                        continue;
                    }
                    double distance = pointProvider.Distance(point, clusters[j]);
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        newClusterIndex = j;
                    }
                }
                if (newClusterIndex != -1)
                {
                    double distanceChange = Math.Abs(Math.Sqrt(minimumDistance) - Math.Sqrt(previousDistance));
                    if (distanceChange > MIN_MOVEMENT_DISTANCE)
                    {
                        pointsMoved++;
                        clusterIndices[i] = newClusterIndex;
                    }
                }
            }

            if (pointsMoved == 0 && iteration != 0)
            {
                break;
            }

            double[] componentASums = new double[clusterCount];
            double[] componentBSums = new double[clusterCount];
            double[] componentCSums = new double[clusterCount];
            Array.Fill(pixelCountSums, 0);
            for (int i = 0; i < pointCount; i++)
            {
                int clusterIndex = clusterIndices[i];
                double[] point = points[i];
                int count = counts[i];
                pixelCountSums[clusterIndex] += count;
                componentASums[clusterIndex] += (point[0] * count);
                componentBSums[clusterIndex] += (point[1] * count);
                componentCSums[clusterIndex] += (point[2] * count);
            }

            for (int i = 0; i < clusterCount; i++)
            {
                int count = pixelCountSums[i];
                if (count == 0)
                {
                    clusters[i] = new double[] { 0.0, 0.0, 0.0 };
                    continue;
                }
                double a = componentASums[i] / count;
                double b = componentBSums[i] / count;
                double c = componentCSums[i] / count;
                clusters[i][0] = a;
                clusters[i][1] = b;
                clusters[i][2] = c;
            }
        }

        var argbToPopulation = new Dictionary<int, int>();
        for (int i = 0; i < clusterCount; i++)
        {
            int count = pixelCountSums[i];
            if (count == 0)
            {
                continue;
            }

            int possibleNewCluster = pointProvider.ToInt(clusters[i]);
            if (argbToPopulation.ContainsKey(possibleNewCluster))
            {
                continue;
            }

            argbToPopulation.Add(possibleNewCluster, count);
        }

        return argbToPopulation;
    }
}
