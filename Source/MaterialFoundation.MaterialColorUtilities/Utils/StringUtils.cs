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

namespace MaterialFoundation.MaterialColorUtilities.Utils;

/// <summary>Utility methods for string representations of colors.</summary>
public static class StringUtils
{
    /// <summary>Hex string representing color, ex. #ff0000 for red.</summary>
    /// <param name="argb">ARGB representation of a color.</param>
    public static string HexFromArgb(int argb)
    {
        int red = ColorUtils.RedFromArgb(argb);
        int blue = ColorUtils.BlueFromArgb(argb);
        int green = ColorUtils.GreenFromArgb(argb);
        return $"{red:x2}{green:x2}{blue:x2}";
    }
}
