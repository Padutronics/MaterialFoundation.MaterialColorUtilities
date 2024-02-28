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

namespace MaterialFoundation.MaterialColorUtilities.Quantize;

/// <summary>An interface to allow use of different color spaces by quantizers.</summary>
public interface PointProvider
{
    /// <summary>The four components in the color space of an sRGB color.</summary>
    double[] fromInt(int argb);

    /// <summary>The ARGB (i.e. hex code) representation of this color.</summary>
    int toInt(double[] point);

    /// <summary>Squared distance between two colors. Distance is defined by scientific color spaces and
    /// referred to as delta E.</summary>
    double distance(double[] a, double[] b);
}
