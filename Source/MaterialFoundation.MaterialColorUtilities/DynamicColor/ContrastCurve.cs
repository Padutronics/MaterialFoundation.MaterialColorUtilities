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

namespace MaterialFoundation.MaterialColorUtilities.DynamicColor;

/// <summary>A class containing a value that changes with the contrast level.
///
/// <para>Usually represents the contrast requirements for a dynamic color on its background. The four
/// values correspond to values for contrast levels -1.0, 0.0, 0.5, and 1.0, respectively.</para></summary>
public sealed class ContrastCurve
{
    /// <summary>Value for contrast level -1.0</summary>
    private readonly double low;
    /// <summary>Value for contrast level 0.0</summary>
    private readonly double normal;
    /// <summary>Value for contrast level 0.5</summary>
    private readonly double medium;
    /// <summary>Value for contrast level 1.0</summary>
    private readonly double high;

    /// <summary>Creates a `ContrastCurve` object.</summary>
    /// <param name="low">Value for contrast level -1.0</param>
    /// <param name="normal">Value for contrast level 0.0</param>
    /// <param name="medium">Value for contrast level 0.5</param>
    /// <param name="high">Value for contrast level 1.0</param>
    public ContrastCurve(double low, double normal, double medium, double high)
    {
        this.low = low;
        this.normal = normal;
        this.medium = medium;
        this.high = high;
    }

    /// <summary>Returns the value at a given contrast level.</summary>
    /// <param name="contrastLevel">The contrast level. 0.0 is the default (normal); -1.0 is the lowest; 1.0
    /// is the highest.</param>
    /// <returns>The value. For contrast ratios, a number between 1.0 and 21.0.</returns>
    public double Get(double contrastLevel)
    {
        if (contrastLevel <= -1.0)
        {
            return low;
        }
        else if (contrastLevel < 0.0)
        {
            return MathUtils.Lerp(low, normal, (contrastLevel - -1) / 1);
        }
        else if (contrastLevel < 0.5)
        {
            return MathUtils.Lerp(normal, medium, (contrastLevel - 0) / 0.5);
        }
        else if (contrastLevel < 1.0)
        {
            return MathUtils.Lerp(medium, high, (contrastLevel - 0.5) / 0.5);
        }
        else
        {
            return high;
        }
    }
}
