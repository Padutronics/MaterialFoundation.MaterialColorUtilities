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

using MaterialFoundation.MaterialColorUtilities.Dislike;
using MaterialFoundation.MaterialColorUtilities.Scheme;
using System;

namespace MaterialFoundation.MaterialColorUtilities.Dynamiccolor;

/// <summary>Named colors, otherwise known as tokens, or roles, in the Material Design system.</summary>
public sealed class MaterialDynamicColors
{
    /// <summary>Optionally use fidelity on most color schemes.</summary>
    private readonly bool isExtendedFidelity;

    public MaterialDynamicColors()
    {
        this.isExtendedFidelity = false;
    }

    public MaterialDynamicColors(bool isExtendedFidelity)
    {
        this.isExtendedFidelity = isExtendedFidelity;
    }

    private static bool IsMonochrome(DynamicScheme scheme)
    {
        return scheme.Variant == Variant.Monochrome;
    }

    private static double FindDesiredChromaByTone(double hue, double chroma, double tone, bool byDecreasingTone)
    {
        double answer = tone;

        Hct.Hct closestToChroma = Hct.Hct.From(hue, chroma, tone);
        if (closestToChroma.Chroma < chroma)
        {
            double chromaPeak = closestToChroma.Chroma;
            while (closestToChroma.Chroma < chroma)
            {
                answer += byDecreasingTone ? -1.0 : 1.0;
                Hct.Hct potentialSolution = Hct.Hct.From(hue, chroma, answer);
                if (chromaPeak > potentialSolution.Chroma)
                {
                    break;
                }
                if (Math.Abs(potentialSolution.Chroma - chroma) < 0.4)
                {
                    break;
                }

                double potentialDelta = Math.Abs(potentialSolution.Chroma - chroma);
                double currentDelta = Math.Abs(closestToChroma.Chroma - chroma);
                if (potentialDelta < currentDelta)
                {
                    closestToChroma = potentialSolution;
                }
                chromaPeak = Math.Max(chromaPeak, potentialSolution.Chroma);
            }
        }

        return answer;
    }

    public DynamicColor HighestSurface(DynamicScheme s)
    {
        return s.IsDark ? SurfaceBright() : SurfaceDim();
    }

    public DynamicColor PrimaryPaletteKeyColor()
    {
        return DynamicColor.FromPalette(
            name: "primary_palette_key_color",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => s.PrimaryPalette.KeyColor.Tone
        );
    }

    public DynamicColor SecondaryPaletteKeyColor()
    {
        return DynamicColor.FromPalette(
            name: "secondary_palette_key_color",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => s.SecondaryPalette.KeyColor.Tone
        );
    }

    public DynamicColor TertiaryPaletteKeyColor()
    {
        return DynamicColor.FromPalette(
            name: "tertiary_palette_key_color",
            palette: (s) => s.TertiaryPalette,
            tone: (s) => s.TertiaryPalette.KeyColor.Tone
        );
    }

    public DynamicColor NeutralPaletteKeyColor()
    {
        return DynamicColor.FromPalette(
            name: "neutral_palette_key_color",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.NeutralPalette.KeyColor.Tone
        );
    }

    public DynamicColor NeutralVariantPaletteKeyColor()
    {
        return DynamicColor.FromPalette(
            name: "neutral_variant_palette_key_color",
            palette: (s) => s.NeutralVariantPalette,
            tone: (s) => s.NeutralVariantPalette.KeyColor.Tone
        );
    }

    public DynamicColor Background()
    {
        return new DynamicColor(
            name: "background",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 6.0 : 98.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor OnBackground()
    {
        return new DynamicColor(
            name: "on_background",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 90.0 : 10.0,
            isBackground: false,
            background: (s) => Background(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 3.0, 4.5, 7.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor Surface()
    {
        return new DynamicColor(
            name: "surface",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 6.0 : 98.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceDim()
    {
        return new DynamicColor(
            name: "surface_dim",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 6.0 : new ContrastCurve(87.0, 87.0, 80.0, 75.0).Get(s.ContrastLevel),
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceBright()
    {
        return new DynamicColor(
            name: "surface_bright",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? new ContrastCurve(24.0, 24.0, 29.0, 34.0).Get(s.ContrastLevel) : 98.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceContainerLowest()
    {
        return new DynamicColor(
            name: "surface_container_lowest",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? new ContrastCurve(4.0, 4.0, 2.0, 0.0).Get(s.ContrastLevel) : 100.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceContainerLow()
    {
        return new DynamicColor(
            name: "surface_container_low",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark
                ? new ContrastCurve(10.0, 10.0, 11.0, 12.0).Get(s.ContrastLevel)
                : new ContrastCurve(96.0, 96.0, 96.0, 95.0).Get(s.ContrastLevel),
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceContainer()
    {
        return new DynamicColor(
            name: "surface_container",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark
                ? new ContrastCurve(12.0, 12.0, 16.0, 20.0).Get(s.ContrastLevel)
                : new ContrastCurve(94.0, 94.0, 92.0, 90.0).Get(s.ContrastLevel),
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceContainerHigh()
    {
        return new DynamicColor(
            name: "surface_container_high",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark
                ? new ContrastCurve(17.0, 17.0, 21.0, 25.0).Get(s.ContrastLevel)
                : new ContrastCurve(92.0, 92.0, 88.0, 85.0).Get(s.ContrastLevel),
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceContainerHighest()
    {
        return new DynamicColor(
            name: "surface_container_highest",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark
                ? new ContrastCurve(22.0, 22.0, 26.0, 30.0).Get(s.ContrastLevel)
                : new ContrastCurve(90.0, 90.0, 84.0, 80.0).Get(s.ContrastLevel),
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor OnSurface()
    {
        return new DynamicColor(
            name: "on_surface",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 90.0 : 10.0,
            isBackground: false,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceVariant()
    {
        return new DynamicColor(
            name: "surface_variant",
            palette: (s) => s.NeutralVariantPalette,
            tone: (s) => s.IsDark ? 30.0 : 90.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor OnSurfaceVariant()
    {
        return new DynamicColor(
            name: "on_surface_variant",
            palette: (s) => s.NeutralVariantPalette,
            tone: (s) => s.IsDark ? 80.0 : 30.0,
            isBackground: false,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 11.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor InverseSurface()
    {
        return new DynamicColor(
            name: "inverse_surface",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 90.0 : 20.0,
            isBackground: false,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor InverseOnSurface()
    {
        return new DynamicColor(
            name: "inverse_on_surface",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 20.0 : 95.0,
            isBackground: false,
            background: (s) => InverseSurface(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor Outline()
    {
        return new DynamicColor(
            name: "outline",
            palette: (s) => s.NeutralVariantPalette,
            tone: (s) => s.IsDark ? 60.0 : 50.0,
            isBackground: false,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.5, 3.0, 4.5, 7.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor OutlineVariant()
    {
        return new DynamicColor(
            name: "outline_variant",
            palette: (s) => s.NeutralVariantPalette,
            tone: (s) => s.IsDark ? 30.0 : 80.0,
            isBackground: false,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: null
        );
    }

    public DynamicColor Shadow()
    {
        return new DynamicColor(
            name: "shadow",
            palette: (s) => s.NeutralPalette,
            tone: (s) => 0.0,
            isBackground: false,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor Scrim()
    {
        return new DynamicColor(
            name: "scrim",
            palette: (s) => s.NeutralPalette,
            tone: (s) => 0.0,
            isBackground: false,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor SurfaceTint()
    {
        return new DynamicColor(
            name: "surface_tint",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null
        );
    }

    public DynamicColor Primary()
    {
        return new DynamicColor(
            name: "primary",
            palette: (s) => s.PrimaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 100.0 : 0.0;
                }
                return s.IsDark ? 80.0 : 40.0;
            },
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 7.0),
            toneDeltaPair: (s) => new ToneDeltaPair(PrimaryContainer(), Primary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnPrimary()
    {
        return new DynamicColor(
            name: "on_primary",
            palette: (s) => s.PrimaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 10.0 : 90.0;
                }
                return s.IsDark ? 20.0 : 100.0;
            },
            isBackground: false,
            background: (s) => Primary(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor PrimaryContainer()
    {
        return new DynamicColor(
            name: "primary_container",
            palette: (s) => s.PrimaryPalette,
            tone: (s) =>
            {
                if (IsFidelity(s))
                {
                    return s.SourceColorHct.Tone;
                }
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 85.0 : 25.0;
                }
                return s.IsDark ? 30.0 : 90.0;
            },
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(PrimaryContainer(), Primary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnPrimaryContainer()
    {
        return new DynamicColor(
            name: "on_primary_container",
            palette: (s) => s.PrimaryPalette,
            tone: (s) =>
            {
                if (IsFidelity(s))
                {
                    return DynamicColor.ForegroundTone(PrimaryContainer().tone(s), 4.5);
                }
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 0.0 : 100.0;
                }
                return s.IsDark ? 90.0 : 10.0;
            },
            isBackground: false,
            background: (s) => PrimaryContainer(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor InversePrimary()
    {
        return new DynamicColor(
            name: "inverse_primary",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => s.IsDark ? 40.0 : 80.0,
            isBackground: false,
            background: (s) => InverseSurface(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 7.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor Secondary()
    {
        return new DynamicColor(
            name: "secondary",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 7.0),
            toneDeltaPair: (s) => new ToneDeltaPair(SecondaryContainer(), Secondary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnSecondary()
    {
        return new DynamicColor(
            name: "on_secondary",
            palette: (s) => s.SecondaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 10.0 : 100.0;
                }
                else
                {
                    return s.IsDark ? 20.0 : 100.0;
                }
            },
            isBackground: false,
            background: (s) => Secondary(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor SecondaryContainer()
    {
        return new DynamicColor(
            name: "secondary_container",
            palette: (s) => s.SecondaryPalette,
            tone: (s) =>
            {
                double initialTone = s.IsDark ? 30.0 : 90.0;
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 30.0 : 85.0;
                }
                if (!IsFidelity(s))
                {
                    return initialTone;
                }
                return FindDesiredChromaByTone(s.SecondaryPalette.Hue, s.SecondaryPalette.Chroma, initialTone, !s.IsDark);
            },
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(SecondaryContainer(), Secondary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnSecondaryContainer()
    {
        return new DynamicColor(
            name: "on_secondary_container",
            palette: (s) => s.SecondaryPalette,
            tone: (s) =>
            {
                if (!IsFidelity(s))
                {
                    return s.IsDark ? 90.0 : 10.0;
                }
                return DynamicColor.ForegroundTone(SecondaryContainer().tone(s), 4.5);
            },
            isBackground: false,
            background: (s) => SecondaryContainer(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor Tertiary()
    {
        return new DynamicColor(
            name: "tertiary",
            palette: (s) => s.TertiaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 90.0 : 25.0;
                }
                return s.IsDark ? 80.0 : 40.0;
            },
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 7.0),
            toneDeltaPair: (s) => new ToneDeltaPair(TertiaryContainer(), Tertiary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnTertiary()
    {
        return new DynamicColor(
            name: "on_tertiary",
            palette: (s) => s.TertiaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 10.0 : 90.0;
                }
                return s.IsDark ? 20.0 : 100.0;
            },
            isBackground: false,
            background: (s) => Tertiary(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor TertiaryContainer()
    {
        return new DynamicColor(
            name: "tertiary_container",
            palette: (s) => s.TertiaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 60.0 : 49.0;
                }
                if (!IsFidelity(s))
                {
                    return s.IsDark ? 30.0 : 90.0;
                }
                Hct.Hct proposedHct = s.TertiaryPalette.GetHct(s.SourceColorHct.Tone);
                return DislikeAnalyzer.FixIfDisliked(proposedHct).Tone;
            },
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(TertiaryContainer(), Tertiary(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnTertiaryContainer()
    {
        return new DynamicColor(
            name: "on_tertiary_container",
            palette: (s) => s.TertiaryPalette,
            tone: (s) =>
            {
                if (IsMonochrome(s))
                {
                    return s.IsDark ? 0.0 : 100.0;
                }
                if (!IsFidelity(s))
                {
                    return s.IsDark ? 90.0 : 10.0;
                }
                return DynamicColor.ForegroundTone(TertiaryContainer().tone(s), 4.5);
            },
            isBackground: false,
            background: (s) => TertiaryContainer(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor Error()
    {
        return new DynamicColor(
            name: "error",
            palette: (s) => s.ErrorPalette,
            tone: (s) => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 7.0),
            toneDeltaPair: (s) => new ToneDeltaPair(ErrorContainer(), Error(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnError()
    {
        return new DynamicColor(
            name: "on_error",
            palette: (s) => s.ErrorPalette,
            tone: (s) => s.IsDark ? 20.0 : 100.0,
            isBackground: false,
            background: (s) => Error(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor ErrorContainer()
    {
        return new DynamicColor(
            name: "error_container",
            palette: (s) => s.ErrorPalette,
            tone: (s) => s.IsDark ? 30.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(ErrorContainer(), Error(), 10.0, TonePolarity.Nearer, false)
        );
    }

    public DynamicColor OnErrorContainer()
    {
        return new DynamicColor(
            name: "on_error_container",
            palette: (s) => s.ErrorPalette,
            tone: (s) => s.IsDark ? 90.0 : 10.0,
            isBackground: false,
            background: (s) => ErrorContainer(),
            secondBackground: null,
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor PrimaryFixed()
    {
        return new DynamicColor(
            name: "primary_fixed",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(PrimaryFixed(), PrimaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor PrimaryFixedDim()
    {
        return new DynamicColor(
            name: "primary_fixed_dim",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(PrimaryFixed(), PrimaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor OnPrimaryFixed()
    {
        return new DynamicColor(
            name: "on_primary_fixed",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => IsMonochrome(s) ? 100.0 : 10.0,
            isBackground: false,
            background: (s) => PrimaryFixedDim(),
            secondBackground: (s) => PrimaryFixed(),
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor OnPrimaryFixedVariant()
    {
        return new DynamicColor(
            name: "on_primary_fixed_variant",
            palette: (s) => s.PrimaryPalette,
            tone: (s) => IsMonochrome(s) ? 90.0 : 30.0,
            isBackground: false,
            background: (s) => PrimaryFixedDim(),
            secondBackground: (s) => PrimaryFixed(),
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 11.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor SecondaryFixed()
    {
        return new DynamicColor(
            name: "secondary_fixed",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => IsMonochrome(s) ? 80.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(SecondaryFixed(), SecondaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor SecondaryFixedDim()
    {
        return new DynamicColor(
            name: "secondary_fixed_dim",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => IsMonochrome(s) ? 70.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(SecondaryFixed(), SecondaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor OnSecondaryFixed()
    {
        return new DynamicColor(
            name: "on_secondary_fixed",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => 10.0,
            isBackground: false,
            background: (s) => SecondaryFixedDim(),
            secondBackground: (s) => SecondaryFixed(),
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor OnSecondaryFixedVariant()
    {
        return new DynamicColor(
            name: "on_secondary_fixed_variant",
            palette: (s) => s.SecondaryPalette,
            tone: (s) => IsMonochrome(s) ? 25.0 : 30.0,
            isBackground: false,
            background: (s) => SecondaryFixedDim(),
            secondBackground: (s) => SecondaryFixed(),
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 11.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor TertiaryFixed()
    {
        return new DynamicColor(
            name: "tertiary_fixed",
            palette: (s) => s.TertiaryPalette,
            tone: (s) => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(TertiaryFixed(), TertiaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor TertiaryFixedDim()
    {
        return new DynamicColor(
            name: "tertiary_fixed_dim",
            palette: (s) => s.TertiaryPalette,
            tone: (s) => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            secondBackground: null,
            contrastCurve: new ContrastCurve(1.0, 1.0, 3.0, 4.5),
            toneDeltaPair: (s) => new ToneDeltaPair(TertiaryFixed(), TertiaryFixedDim(), 10.0, TonePolarity.Lighter, true)
        );
    }

    public DynamicColor OnTertiaryFixed()
    {
        return new DynamicColor(
            name: "on_tertiary_fixed",
            palette: (s) => s.TertiaryPalette,
            tone: (s) => IsMonochrome(s) ? 100.0 : 10.0,
            isBackground: false,
            background: (s) => TertiaryFixedDim(),
            secondBackground: (s) => TertiaryFixed(),
            contrastCurve: new ContrastCurve(4.5, 7.0, 11.0, 21.0),
            toneDeltaPair: null
        );
    }

    public DynamicColor OnTertiaryFixedVariant()
    {
        return new DynamicColor(
            name: "on_tertiary_fixed_variant",
            palette: (s) => s.TertiaryPalette,
            tone: (s) => IsMonochrome(s) ? 90.0 : 30.0,
            isBackground: false,
            background: (s) => TertiaryFixedDim(),
            secondBackground: (s) => TertiaryFixed(),
            contrastCurve: new ContrastCurve(3.0, 4.5, 7.0, 11.0),
            toneDeltaPair: null
        );
    }

    /// <summary>These colors were present in Android framework before Android U, and used by MDC controls. They
    /// should be avoided, if possible. It's unclear if they're used on multiple backgrounds, and if
    /// they are, they can't be adjusted for contrast.* For now, they will be set with no background,
    /// and those won't adjust for contrast, avoiding issues.
    ///
    /// <para>* For example, if the same color is on a white background _and_ black background, there's no
    /// way to increase contrast with either without losing contrast with the other.</para></summary>
    /// <remarks>colorControlActivated documented as colorAccent in M3 & GM3.
    /// colorAccent documented as colorSecondary in M3 and colorPrimary in GM3.
    /// Android used Material's Container as Primary/Secondary/Tertiary at launch.
    /// Therefore, this is a duplicated version of Primary Container.</remarks>
    public DynamicColor ControlActivated()
    {
        return DynamicColor.FromPalette("control_activated", (s) => s.PrimaryPalette, (s) => s.IsDark ? 30.0 : 90.0);
    }

    /// <remarks>colorControlNormal documented as textColorSecondary in M3 & GM3.
    /// In Material, textColorSecondary points to onSurfaceVariant in the non-disabled state,
    /// which is Neutral Variant T30/80 in light/dark.</remarks>
    public DynamicColor ControlNormal()
    {
        return DynamicColor.FromPalette("control_normal", (s) => s.NeutralVariantPalette, (s) => s.IsDark ? 80.0 : 30.0);
    }

    /// <remarks>colorControlHighlight documented, in both M3 & GM3:
    /// Light mode: #1f000000 dark mode: #33ffffff.
    /// These are black and white with some alpha.
    /// 1F hex = 31 decimal; 31 / 255 = 12% alpha.
    /// 33 hex = 51 decimal; 51 / 255 = 20% alpha.
    /// DynamicColors do not support alpha currently, and _may_ not need it for this use case,
    /// depending on how MDC resolved alpha for the other cases.
    /// Returning black in dark mode, white in light mode.</remarks>
    public DynamicColor ControlHighlight()
    {
        return new DynamicColor(
            name: "control_highlight",
            palette: (s) => s.NeutralPalette,
            tone: (s) => s.IsDark ? 100.0 : 0.0,
            isBackground: false,
            background: null,
            secondBackground: null,
            contrastCurve: null,
            toneDeltaPair: null,
            opacity: s => s.IsDark ? 0.20 : 0.12
        );
    }

    /// <remarks>textColorPrimaryInverse documented, in both M3 & GM3, documented as N10/N90.</remarks>
    public DynamicColor TextPrimaryInverse()
    {
        return DynamicColor.FromPalette("text_primary_inverse", (s) => s.NeutralPalette, (s) => s.IsDark ? 10.0 : 90.0);
    }

    /// <remarks>textColorSecondaryInverse and textColorTertiaryInverse both documented, in both M3 & GM3, as
    /// NV30/NV80</remarks>
    public DynamicColor TextSecondaryAndTertiaryInverse()
    {
        return DynamicColor.FromPalette(
            "text_secondary_and_tertiary_inverse",
            (s) => s.NeutralVariantPalette,
            (s) => s.IsDark ? 30.0 : 80.0
        );
    }

    /// <remarks>textColorPrimaryInverseDisableOnly documented, in both M3 & GM3, as N10/N90</remarks>
    public DynamicColor TextPrimaryInverseDisableOnly()
    {
        return DynamicColor.FromPalette(
            "text_primary_inverse_disable_only",
            (s) => s.NeutralPalette,
            (s) => s.IsDark ? 10.0 : 90.0
        );
    }

    /// <remarks>textColorSecondaryInverse and textColorTertiaryInverse in disabled state both documented,
    /// in both M3 & GM3, as N10/N90</remarks>
    public DynamicColor TextSecondaryAndTertiaryInverseDisabled()
    {
        return DynamicColor.FromPalette(
            "text_secondary_and_tertiary_inverse_disabled",
            (s) => s.NeutralPalette,
            (s) => s.IsDark ? 10.0 : 90.0
        );
    }

    /// <remarks>textColorHintInverse documented, in both M3 & GM3, as N10/N90</remarks>
    public DynamicColor TextHintInverse()
    {
        return DynamicColor.FromPalette("text_hint_inverse", (s) => s.NeutralPalette, (s) => s.IsDark ? 10.0 : 90.0);
    }

    private bool IsFidelity(DynamicScheme scheme)
    {
        if (isExtendedFidelity && scheme.Variant != Variant.Monochrome && scheme.Variant != Variant.Neutral)
        {
            return true;
        }
        return scheme.Variant == Variant.Fidelity || scheme.Variant == Variant.Content;
    }
}
