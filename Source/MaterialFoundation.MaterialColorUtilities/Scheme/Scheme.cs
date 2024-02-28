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

using MaterialFoundation.MaterialColorUtilities.Palettes;

namespace MaterialFoundation.MaterialColorUtilities.Scheme;

/// <summary>Represents a Material color scheme, a mapping of color roles to colors.</summary>
public class Scheme
{
    private int primary;
    private int onPrimary;
    private int primaryContainer;
    private int onPrimaryContainer;
    private int secondary;
    private int onSecondary;
    private int secondaryContainer;
    private int onSecondaryContainer;
    private int tertiary;
    private int onTertiary;
    private int tertiaryContainer;
    private int onTertiaryContainer;
    private int error;
    private int onError;
    private int errorContainer;
    private int onErrorContainer;
    private int background;
    private int onBackground;
    private int surface;
    private int onSurface;
    private int surfaceVariant;
    private int onSurfaceVariant;
    private int outline;
    private int outlineVariant;
    private int shadow;
    private int scrim;
    private int inverseSurface;
    private int inverseOnSurface;
    private int inversePrimary;

    public Scheme()
    {
    }

    public Scheme(int primary, int onPrimary, int primaryContainer, int onPrimaryContainer, int secondary, int onSecondary, int secondaryContainer, int onSecondaryContainer, int tertiary, int onTertiary, int tertiaryContainer, int onTertiaryContainer, int error, int onError, int errorContainer, int onErrorContainer, int background, int onBackground, int surface, int onSurface, int surfaceVariant, int onSurfaceVariant, int outline, int outlineVariant, int shadow, int scrim, int inverseSurface, int inverseOnSurface, int inversePrimary)
    {
        this.primary = primary;
        this.onPrimary = onPrimary;
        this.primaryContainer = primaryContainer;
        this.onPrimaryContainer = onPrimaryContainer;
        this.secondary = secondary;
        this.onSecondary = onSecondary;
        this.secondaryContainer = secondaryContainer;
        this.onSecondaryContainer = onSecondaryContainer;
        this.tertiary = tertiary;
        this.onTertiary = onTertiary;
        this.tertiaryContainer = tertiaryContainer;
        this.onTertiaryContainer = onTertiaryContainer;
        this.error = error;
        this.onError = onError;
        this.errorContainer = errorContainer;
        this.onErrorContainer = onErrorContainer;
        this.background = background;
        this.onBackground = onBackground;
        this.surface = surface;
        this.onSurface = onSurface;
        this.surfaceVariant = surfaceVariant;
        this.onSurfaceVariant = onSurfaceVariant;
        this.outline = outline;
        this.outlineVariant = outlineVariant;
        this.shadow = shadow;
        this.scrim = scrim;
        this.inverseSurface = inverseSurface;
        this.inverseOnSurface = inverseOnSurface;
        this.inversePrimary = inversePrimary;
    }

    /// <summary>Creates a light theme Scheme from a source color in ARGB, i.e. a hex code.</summary>
    public static Scheme Light(int argb)
    {
        return LightFromCorePalette(CorePalette.Of(argb));
    }

    /// <summary>Creates a dark theme Scheme from a source color in ARGB, i.e. a hex code.</summary>
    public static Scheme Dark(int argb)
    {
        return DarkFromCorePalette(CorePalette.Of(argb));
    }

    /// <summary>Creates a light theme content-based Scheme from a source color in ARGB, i.e. a hex code.</summary>
    public static Scheme LightContent(int argb)
    {
        return LightFromCorePalette(CorePalette.ContentOf(argb));
    }

    /// <summary>Creates a dark theme content-based Scheme from a source color in ARGB, i.e. a hex code.</summary>
    public static Scheme DarkContent(int argb)
    {
        return DarkFromCorePalette(CorePalette.ContentOf(argb));
    }

    private static Scheme LightFromCorePalette(CorePalette core)
    {
        return new Scheme()
            .WithPrimary(core.a1.Tone(40))
            .WithOnPrimary(core.a1.Tone(100))
            .WithPrimaryContainer(core.a1.Tone(90))
            .WithOnPrimaryContainer(core.a1.Tone(10))
            .WithSecondary(core.a2.Tone(40))
            .WithOnSecondary(core.a2.Tone(100))
            .WithSecondaryContainer(core.a2.Tone(90))
            .WithOnSecondaryContainer(core.a2.Tone(10))
            .WithTertiary(core.a3.Tone(40))
            .WithOnTertiary(core.a3.Tone(100))
            .WithTertiaryContainer(core.a3.Tone(90))
            .WithOnTertiaryContainer(core.a3.Tone(10))
            .WithError(core.error.Tone(40))
            .WithOnError(core.error.Tone(100))
            .WithErrorContainer(core.error.Tone(90))
            .WithOnErrorContainer(core.error.Tone(10))
            .WithBackground(core.n1.Tone(99))
            .WithOnBackground(core.n1.Tone(10))
            .WithSurface(core.n1.Tone(99))
            .WithOnSurface(core.n1.Tone(10))
            .WithSurfaceVariant(core.n2.Tone(90))
            .WithOnSurfaceVariant(core.n2.Tone(30))
            .WithOutline(core.n2.Tone(50))
            .WithOutlineVariant(core.n2.Tone(80))
            .WithShadow(core.n1.Tone(0))
            .WithScrim(core.n1.Tone(0))
            .WithInverseSurface(core.n1.Tone(20))
            .WithInverseOnSurface(core.n1.Tone(95))
            .WithInversePrimary(core.a1.Tone(80));
    }

    private static Scheme DarkFromCorePalette(CorePalette core)
    {
        return new Scheme()
            .WithPrimary(core.a1.Tone(80))
            .WithOnPrimary(core.a1.Tone(20))
            .WithPrimaryContainer(core.a1.Tone(30))
            .WithOnPrimaryContainer(core.a1.Tone(90))
            .WithSecondary(core.a2.Tone(80))
            .WithOnSecondary(core.a2.Tone(20))
            .WithSecondaryContainer(core.a2.Tone(30))
            .WithOnSecondaryContainer(core.a2.Tone(90))
            .WithTertiary(core.a3.Tone(80))
            .WithOnTertiary(core.a3.Tone(20))
            .WithTertiaryContainer(core.a3.Tone(30))
            .WithOnTertiaryContainer(core.a3.Tone(90))
            .WithError(core.error.Tone(80))
            .WithOnError(core.error.Tone(20))
            .WithErrorContainer(core.error.Tone(30))
            .WithOnErrorContainer(core.error.Tone(80))
            .WithBackground(core.n1.Tone(10))
            .WithOnBackground(core.n1.Tone(90))
            .WithSurface(core.n1.Tone(10))
            .WithOnSurface(core.n1.Tone(90))
            .WithSurfaceVariant(core.n2.Tone(30))
            .WithOnSurfaceVariant(core.n2.Tone(80))
            .WithOutline(core.n2.Tone(60))
            .WithOutlineVariant(core.n2.Tone(30))
            .WithShadow(core.n1.Tone(0))
            .WithScrim(core.n1.Tone(0))
            .WithInverseSurface(core.n1.Tone(90))
            .WithInverseOnSurface(core.n1.Tone(20))
            .WithInversePrimary(core.a1.Tone(40));
    }

    public int GetPrimary()
    {
        return primary;
    }

    public void SetPrimary(int primary)
    {
        this.primary = primary;
    }

    public Scheme WithPrimary(int primary)
    {
        this.primary = primary;
        return this;
    }

    public int GetOnPrimary()
    {
        return onPrimary;
    }

    public void SetOnPrimary(int onPrimary)
    {
        this.onPrimary = onPrimary;
    }

    public Scheme WithOnPrimary(int onPrimary)
    {
        this.onPrimary = onPrimary;
        return this;
    }

    public int GetPrimaryContainer()
    {
        return primaryContainer;
    }

    public void SetPrimaryContainer(int primaryContainer)
    {
        this.primaryContainer = primaryContainer;
    }

    public Scheme WithPrimaryContainer(int primaryContainer)
    {
        this.primaryContainer = primaryContainer;
        return this;
    }

    public int GetOnPrimaryContainer()
    {
        return onPrimaryContainer;
    }

    public void SetOnPrimaryContainer(int onPrimaryContainer)
    {
        this.onPrimaryContainer = onPrimaryContainer;
    }

    public Scheme WithOnPrimaryContainer(int onPrimaryContainer)
    {
        this.onPrimaryContainer = onPrimaryContainer;
        return this;
    }

    public int GetSecondary()
    {
        return secondary;
    }

    public void SetSecondary(int secondary)
    {
        this.secondary = secondary;
    }

    public Scheme WithSecondary(int secondary)
    {
        this.secondary = secondary;
        return this;
    }

    public int GetOnSecondary()
    {
        return onSecondary;
    }

    public void SetOnSecondary(int onSecondary)
    {
        this.onSecondary = onSecondary;
    }

    public Scheme WithOnSecondary(int onSecondary)
    {
        this.onSecondary = onSecondary;
        return this;
    }

    public int GetSecondaryContainer()
    {
        return secondaryContainer;
    }

    public void SetSecondaryContainer(int secondaryContainer)
    {
        this.secondaryContainer = secondaryContainer;
    }

    public Scheme WithSecondaryContainer(int secondaryContainer)
    {
        this.secondaryContainer = secondaryContainer;
        return this;
    }

    public int GetOnSecondaryContainer()
    {
        return onSecondaryContainer;
    }

    public void SetOnSecondaryContainer(int onSecondaryContainer)
    {
        this.onSecondaryContainer = onSecondaryContainer;
    }

    public Scheme WithOnSecondaryContainer(int onSecondaryContainer)
    {
        this.onSecondaryContainer = onSecondaryContainer;
        return this;
    }

    public int GetTertiary()
    {
        return tertiary;
    }

    public void SetTertiary(int tertiary)
    {
        this.tertiary = tertiary;
    }

    public Scheme WithTertiary(int tertiary)
    {
        this.tertiary = tertiary;
        return this;
    }

    public int GetOnTertiary()
    {
        return onTertiary;
    }

    public void SetOnTertiary(int onTertiary)
    {
        this.onTertiary = onTertiary;
    }

    public Scheme WithOnTertiary(int onTertiary)
    {
        this.onTertiary = onTertiary;
        return this;
    }

    public int GetTertiaryContainer()
    {
        return tertiaryContainer;
    }

    public void SetTertiaryContainer(int tertiaryContainer)
    {
        this.tertiaryContainer = tertiaryContainer;
    }

    public Scheme WithTertiaryContainer(int tertiaryContainer)
    {
        this.tertiaryContainer = tertiaryContainer;
        return this;
    }

    public int GetOnTertiaryContainer()
    {
        return onTertiaryContainer;
    }

    public void SetOnTertiaryContainer(int onTertiaryContainer)
    {
        this.onTertiaryContainer = onTertiaryContainer;
    }

    public Scheme WithOnTertiaryContainer(int onTertiaryContainer)
    {
        this.onTertiaryContainer = onTertiaryContainer;
        return this;
    }

    public int GetError()
    {
        return error;
    }

    public void SetError(int error)
    {
        this.error = error;
    }

    public Scheme WithError(int error)
    {
        this.error = error;
        return this;
    }

    public int GetOnError()
    {
        return onError;
    }

    public void SetOnError(int onError)
    {
        this.onError = onError;
    }

    public Scheme WithOnError(int onError)
    {
        this.onError = onError;
        return this;
    }

    public int GetErrorContainer()
    {
        return errorContainer;
    }

    public void SetErrorContainer(int errorContainer)
    {
        this.errorContainer = errorContainer;
    }

    public Scheme WithErrorContainer(int errorContainer)
    {
        this.errorContainer = errorContainer;
        return this;
    }

    public int GetOnErrorContainer()
    {
        return onErrorContainer;
    }

    public void SetOnErrorContainer(int onErrorContainer)
    {
        this.onErrorContainer = onErrorContainer;
    }

    public Scheme WithOnErrorContainer(int onErrorContainer)
    {
        this.onErrorContainer = onErrorContainer;
        return this;
    }

    public int GetBackground()
    {
        return background;
    }

    public void SetBackground(int background)
    {
        this.background = background;
    }

    public Scheme WithBackground(int background)
    {
        this.background = background;
        return this;
    }

    public int GetOnBackground()
    {
        return onBackground;
    }

    public void SetOnBackground(int onBackground)
    {
        this.onBackground = onBackground;
    }

    public Scheme WithOnBackground(int onBackground)
    {
        this.onBackground = onBackground;
        return this;
    }

    public int GetSurface()
    {
        return surface;
    }

    public void SetSurface(int surface)
    {
        this.surface = surface;
    }

    public Scheme WithSurface(int surface)
    {
        this.surface = surface;
        return this;
    }

    public int GetOnSurface()
    {
        return onSurface;
    }

    public void SetOnSurface(int onSurface)
    {
        this.onSurface = onSurface;
    }

    public Scheme WithOnSurface(int onSurface)
    {
        this.onSurface = onSurface;
        return this;
    }

    public int GetSurfaceVariant()
    {
        return surfaceVariant;
    }

    public void SetSurfaceVariant(int surfaceVariant)
    {
        this.surfaceVariant = surfaceVariant;
    }

    public Scheme WithSurfaceVariant(int surfaceVariant)
    {
        this.surfaceVariant = surfaceVariant;
        return this;
    }

    public int GetOnSurfaceVariant()
    {
        return onSurfaceVariant;
    }

    public void SetOnSurfaceVariant(int onSurfaceVariant)
    {
        this.onSurfaceVariant = onSurfaceVariant;
    }

    public Scheme WithOnSurfaceVariant(int onSurfaceVariant)
    {
        this.onSurfaceVariant = onSurfaceVariant;
        return this;
    }

    public int GetOutline()
    {
        return outline;
    }

    public void SetOutline(int outline)
    {
        this.outline = outline;
    }

    public Scheme WithOutline(int outline)
    {
        this.outline = outline;
        return this;
    }

    public int GetOutlineVariant()
    {
        return outlineVariant;
    }

    public void SetOutlineVariant(int outlineVariant)
    {
        this.outlineVariant = outlineVariant;
    }

    public Scheme WithOutlineVariant(int outlineVariant)
    {
        this.outlineVariant = outlineVariant;
        return this;
    }

    public int GetShadow()
    {
        return shadow;
    }

    public void SetShadow(int shadow)
    {
        this.shadow = shadow;
    }

    public Scheme WithShadow(int shadow)
    {
        this.shadow = shadow;
        return this;
    }

    public int GetScrim()
    {
        return scrim;
    }

    public void SetScrim(int scrim)
    {
        this.scrim = scrim;
    }

    public Scheme WithScrim(int scrim)
    {
        this.scrim = scrim;
        return this;
    }

    public int GetInverseSurface()
    {
        return inverseSurface;
    }

    public void SetInverseSurface(int inverseSurface)
    {
        this.inverseSurface = inverseSurface;
    }

    public Scheme WithInverseSurface(int inverseSurface)
    {
        this.inverseSurface = inverseSurface;
        return this;
    }

    public int GetInverseOnSurface()
    {
        return inverseOnSurface;
    }

    public void SetInverseOnSurface(int inverseOnSurface)
    {
        this.inverseOnSurface = inverseOnSurface;
    }

    public Scheme WithInverseOnSurface(int inverseOnSurface)
    {
        this.inverseOnSurface = inverseOnSurface;
        return this;
    }

    public int GetInversePrimary()
    {
        return inversePrimary;
    }

    public void SetInversePrimary(int inversePrimary)
    {
        this.inversePrimary = inversePrimary;
    }

    public Scheme WithInversePrimary(int inversePrimary)
    {
        this.inversePrimary = inversePrimary;
        return this;
    }

    public override string ToString()
    {
        return "Scheme{" +
            "primary=" +
            primary +
            ", onPrimary=" +
            onPrimary +
            ", primaryContainer=" +
            primaryContainer +
            ", onPrimaryContainer=" +
            onPrimaryContainer +
            ", secondary=" +
            secondary +
            ", onSecondary=" +
            onSecondary +
            ", secondaryContainer=" +
            secondaryContainer +
            ", onSecondaryContainer=" +
            onSecondaryContainer +
            ", tertiary=" +
            tertiary +
            ", onTertiary=" +
            onTertiary +
            ", tertiaryContainer=" +
            tertiaryContainer +
            ", onTertiaryContainer=" +
            onTertiaryContainer +
            ", error=" +
            error +
            ", onError=" +
            onError +
            ", errorContainer=" +
            errorContainer +
            ", onErrorContainer=" +
            onErrorContainer +
            ", background=" +
            background +
            ", onBackground=" +
            onBackground +
            ", surface=" +
            surface +
            ", onSurface=" +
            onSurface +
            ", surfaceVariant=" +
            surfaceVariant +
            ", onSurfaceVariant=" +
            onSurfaceVariant +
            ", outline=" +
            outline +
            ", outlineVariant=" +
            outlineVariant +
            ", shadow=" +
            shadow +
            ", scrim=" +
            scrim +
            ", inverseSurface=" +
            inverseSurface +
            ", inverseOnSurface=" +
            inverseOnSurface +
            ", inversePrimary=" +
            inversePrimary +
            '}';
    }

    public override bool Equals(object? @object)
    {
        if (this == @object)
        {
            return true;
        }
        if (@object is not Scheme)
        {
            return false;
        }

        Scheme scheme = (Scheme)@object;

        if (primary != scheme.primary)
        {
            return false;
        }
        if (onPrimary != scheme.onPrimary)
        {
            return false;
        }
        if (primaryContainer != scheme.primaryContainer)
        {
            return false;
        }
        if (onPrimaryContainer != scheme.onPrimaryContainer)
        {
            return false;
        }
        if (secondary != scheme.secondary)
        {
            return false;
        }
        if (onSecondary != scheme.onSecondary)
        {
            return false;
        }
        if (secondaryContainer != scheme.secondaryContainer)
        {
            return false;
        }
        if (onSecondaryContainer != scheme.onSecondaryContainer)
        {
            return false;
        }
        if (tertiary != scheme.tertiary)
        {
            return false;
        }
        if (onTertiary != scheme.onTertiary)
        {
            return false;
        }
        if (tertiaryContainer != scheme.tertiaryContainer)
        {
            return false;
        }
        if (onTertiaryContainer != scheme.onTertiaryContainer)
        {
            return false;
        }
        if (error != scheme.error)
        {
            return false;
        }
        if (onError != scheme.onError)
        {
            return false;
        }
        if (errorContainer != scheme.errorContainer)
        {
            return false;
        }
        if (onErrorContainer != scheme.onErrorContainer)
        {
            return false;
        }
        if (background != scheme.background)
        {
            return false;
        }
        if (onBackground != scheme.onBackground)
        {
            return false;
        }
        if (surface != scheme.surface)
        {
            return false;
        }
        if (onSurface != scheme.onSurface)
        {
            return false;
        }
        if (surfaceVariant != scheme.surfaceVariant)
        {
            return false;
        }
        if (onSurfaceVariant != scheme.onSurfaceVariant)
        {
            return false;
        }
        if (outline != scheme.outline)
        {
            return false;
        }
        if (outlineVariant != scheme.outlineVariant)
        {
            return false;
        }
        if (shadow != scheme.shadow)
        {
            return false;
        }
        if (scrim != scheme.scrim)
        {
            return false;
        }
        if (inverseSurface != scheme.inverseSurface)
        {
            return false;
        }
        if (inverseOnSurface != scheme.inverseOnSurface)
        {
            return false;
        }
        if (inversePrimary != scheme.inversePrimary)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = base.GetHashCode();
        result = 31 * result + primary;
        result = 31 * result + onPrimary;
        result = 31 * result + primaryContainer;
        result = 31 * result + onPrimaryContainer;
        result = 31 * result + secondary;
        result = 31 * result + onSecondary;
        result = 31 * result + secondaryContainer;
        result = 31 * result + onSecondaryContainer;
        result = 31 * result + tertiary;
        result = 31 * result + onTertiary;
        result = 31 * result + tertiaryContainer;
        result = 31 * result + onTertiaryContainer;
        result = 31 * result + error;
        result = 31 * result + onError;
        result = 31 * result + errorContainer;
        result = 31 * result + onErrorContainer;
        result = 31 * result + background;
        result = 31 * result + onBackground;
        result = 31 * result + surface;
        result = 31 * result + onSurface;
        result = 31 * result + surfaceVariant;
        result = 31 * result + onSurfaceVariant;
        result = 31 * result + outline;
        result = 31 * result + outlineVariant;
        result = 31 * result + shadow;
        result = 31 * result + scrim;
        result = 31 * result + inverseSurface;
        result = 31 * result + inverseOnSurface;
        result = 31 * result + inversePrimary;
        return result;
    }
}
