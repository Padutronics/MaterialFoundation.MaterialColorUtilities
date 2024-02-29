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

namespace MaterialFoundation.MaterialColorUtilities.DynamicColor;

/// <summary>Documents a constraint between two DynamicColors, in which their tones must have a certain
/// distance from each other.
///
/// <para>Prefer a DynamicColor with a background, this is for special cases when designers want tonal
/// distance, literally contrast, between two colors that don't have a background / foreground
/// relationship or a contrast guarantee.</para></summary>
public sealed class ToneDeltaPair
{
    /// <summary>Documents a constraint in tone distance between two DynamicColors.
    ///
    /// <para>The polarity is an adjective that describes "A", compared to "B".</para>
    ///
    /// <para>For instance, ToneDeltaPair(A, B, 15, 'darker', stayTogether) states that A's tone should be
    /// at least 15 darker than B's.</para>
    ///
    /// <para>'nearer' and 'farther' describes closeness to the surface roles. For instance,
    /// ToneDeltaPair(A, B, 10, 'nearer', stayTogether) states that A should be 10 lighter than B in
    /// light mode, and 10 darker than B in dark mode.</para></summary>
    /// <param name="roleA">The first role in a pair.</param>
    /// <param name="roleB">The second role in a pair.</param>
    /// <param name="delta">Required difference between tones. Absolute value, negative values have undefined
    /// behavior.</param>
    /// <param name="polarity">The relative relation between tones of roleA and roleB, as described above.</param>
    /// <param name="stayTogether">Whether these two roles should stay on the same side of the "awkward zone"
    /// (T50-59). This is necessary for certain cases where one role has two backgrounds.</param>
    public ToneDeltaPair(DynamicColor roleA, DynamicColor roleB, double delta, TonePolarity polarity, bool stayTogether)
    {
        RoleA = roleA;
        RoleB = roleB;
        Delta = delta;
        Polarity = polarity;
        StayTogether = stayTogether;
    }

    /// <summary>The first role in a pair.</summary>
    public DynamicColor RoleA { get; }

    /// <summary>The second role in a pair.</summary>
    public DynamicColor RoleB { get; }

    /// <summary>Required difference between tones. Absolute value, negative values have undefined behavior.</summary>
    public double Delta { get; }

    /// <summary>The relative relation between tones of roleA and roleB, as described above.</summary>
    public TonePolarity Polarity { get; }

    /// <summary>Whether these two roles should stay on the same side of the "awkward zone" (T50-59). This is
    /// necessary for certain cases where one role has two backgrounds.</summary>
    public bool StayTogether { get; }
}
