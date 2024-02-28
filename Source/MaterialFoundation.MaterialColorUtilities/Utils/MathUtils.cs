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

namespace MaterialFoundation.MaterialColorUtilities.Utils;

/// <summary>Utility methods for mathematical operations.</summary>
public static class MathUtils
{
    /// <summary>The signum function.</summary>
    /// <returns>1 if num > 0, -1 if num < 0, and 0 if num = 0</returns>
    public static int signum(double num)
    {
        if (num < 0)
        {
            return -1;
        }
        else if (num == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>The linear interpolation function.</summary>
    /// <returns>start if amount = 0 and stop if amount = 1</returns>
    public static double lerp(double start, double stop, double amount)
    {
        return (1.0 - amount) * start + amount * stop;
    }

    /// <summary>Clamps an integer between two integers.</summary>
    /// <returns>input when min <= input <= max, and either min or max otherwise.</returns>
    public static int clampInt(int min, int max, int input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
    }

    /// <summary>Clamps an integer between two floating-point numbers.</summary>
    /// <returns>input when min <= input <= max, and either min or max otherwise.</returns>
    public static double clampDouble(double min, double max, double input)
    {
        if (input < min)
        {
            return min;
        }
        else if (input > max)
        {
            return max;
        }

        return input;
    }

    /// <summary>Sanitizes a degree measure as an integer.</summary>
    /// <returns>a degree measure between 0 (inclusive) and 360 (exclusive).</returns>
    public static int sanitizeDegreesInt(int degrees)
    {
        degrees = degrees % 360;
        if (degrees < 0)
        {
            degrees = degrees + 360;
        }
        return degrees;
    }

    /// <summary>Sanitizes a degree measure as a floating-point number.</summary>
    /// <returns>a degree measure between 0.0 (inclusive) and 360.0 (exclusive).</returns>
    public static double sanitizeDegreesDouble(double degrees)
    {
        degrees = degrees % 360.0;
        if (degrees < 0)
        {
            degrees = degrees + 360.0;
        }
        return degrees;
    }

    /// <summary>Sign of direction change needed to travel from one angle to another.
    ///
    /// <para>For angles that are 180 degrees apart from each other, both directions have the same travel
    /// distance, so either direction is shortest. The value 1.0 is returned in this case.</para></summary>
    /// <param name="from">The angle travel starts from, in degrees.</param>
    /// <param name="to">The angle travel ends at, in degrees.</param>
    /// <returns>-1 if decreasing from leads to the shortest travel distance, 1 if increasing from leads
    /// to the shortest travel distance.</returns>
    public static double rotationDirection(double from, double to)
    {
        double increasingDifference = sanitizeDegreesDouble(to - from);
        return increasingDifference <= 180.0 ? 1.0 : -1.0;
    }

    /// <summary>Distance of two points on a circle, represented using degrees.</summary>
    public static double differenceDegrees(double a, double b)
    {
        return 180.0 - Math.Abs(Math.Abs(a - b) - 180.0);
    }

    /// <summary>Multiplies a 1x3 row vector with a 3x3 matrix.</summary>
    public static double[] matrixMultiply(double[] row, double[][] matrix)
    {
        double a = row[0] * matrix[0][0] + row[1] * matrix[0][1] + row[2] * matrix[0][2];
        double b = row[0] * matrix[1][0] + row[1] * matrix[1][1] + row[2] * matrix[1][2];
        double c = row[0] * matrix[2][0] + row[1] * matrix[2][1] + row[2] * matrix[2][2];
        return new double[] { a, b, c };
    }

    public static double toDegrees(double radians)
    {
        return radians * (180 / Math.PI);
    }

    public static double toRadians(double degrees)
    {
        return (Math.PI / 180) * degrees;
    }
}
