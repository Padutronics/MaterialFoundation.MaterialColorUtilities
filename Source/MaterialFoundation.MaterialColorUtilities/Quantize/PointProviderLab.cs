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

namespace MaterialFoundation.MaterialColorUtilities.Quantize;

/// <summary>Provides conversions needed for K-Means quantization. Converting input to points, and converting
/// the final state of the K-Means algorithm to colors.</summary>
public sealed class PointProviderLab : PointProvider
{
    /// <summary>Convert a color represented in ARGB to a 3-element array of L*a*b* coordinates of the color.</summary>
    public double[] FromInt(int argb)
    {
        double[] lab = ColorUtils.LabFromArgb(argb);
        return new double[] { lab[0], lab[1], lab[2] };
    }

    /// <summary>Convert a 3-element array to a color represented in ARGB.</summary>
    public int ToInt(double[] lab)
    {
        return ColorUtils.ArgbFromLab(lab[0], lab[1], lab[2]);
    }

    /// <summary>Standard CIE 1976 delta E formula also takes the square root, unneeded here. This method is
    /// used by quantization algorithms to compare distance, and the relative ordering is the same,
    /// with or without a square root.
    ///
    /// <para>This relatively minor optimization is helpful because this method is called at least once
    /// for each pixel in an image.</para></summary>
    public double Distance(double[] one, double[] two)
    {
        double dL = (one[0] - two[0]);
        double dA = (one[1] - two[1]);
        double dB = (one[2] - two[2]);
        return (dL * dL + dA * dA + dB * dB);
    }
}
