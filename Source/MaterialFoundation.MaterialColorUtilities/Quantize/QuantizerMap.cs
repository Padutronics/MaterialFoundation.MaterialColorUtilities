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

using System.Collections.Generic;

namespace MaterialFoundation.MaterialColorUtilities.Quantize;

/// <summary>Creates a dictionary with keys of colors, and values of count of the color</summary>
public sealed class QuantizerMap : IQuantizer
{
    public IDictionary<int, int>? ColorToCount { get; private set; }

    public QuantizerResult Quantize(int[] pixels, int colorCount)
    {
        var pixelByCount = new Dictionary<int, int>();
        foreach (int pixel in pixels)
        {
            int newPixelCount;
            if (pixelByCount.TryGetValue(pixel, out int currentPixelCount))
            {
                newPixelCount = currentPixelCount + 1;
                pixelByCount[pixel] = newPixelCount;
            }
            else
            {
                newPixelCount = 1;
                pixelByCount.Add(pixel, newPixelCount);
            }
        }
        ColorToCount = pixelByCount;
        return new QuantizerResult(pixelByCount);
    }
}
