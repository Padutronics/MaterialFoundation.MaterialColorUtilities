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
    public Scheme()
    {
    }

    public Scheme(int primary, int onPrimary, int primaryContainer, int onPrimaryContainer, int secondary, int onSecondary, int secondaryContainer, int onSecondaryContainer, int tertiary, int onTertiary, int tertiaryContainer, int onTertiaryContainer, int error, int onError, int errorContainer, int onErrorContainer, int background, int onBackground, int surface, int onSurface, int surfaceVariant, int onSurfaceVariant, int outline, int outlineVariant, int shadow, int scrim, int inverseSurface, int inverseOnSurface, int inversePrimary)
    {
        Primary = primary;
        OnPrimary = onPrimary;
        PrimaryContainer = primaryContainer;
        OnPrimaryContainer = onPrimaryContainer;
        Secondary = secondary;
        OnSecondary = onSecondary;
        SecondaryContainer = secondaryContainer;
        OnSecondaryContainer = onSecondaryContainer;
        Tertiary = tertiary;
        OnTertiary = onTertiary;
        TertiaryContainer = tertiaryContainer;
        OnTertiaryContainer = onTertiaryContainer;
        Error = error;
        OnError = onError;
        ErrorContainer = errorContainer;
        OnErrorContainer = onErrorContainer;
        Background = background;
        OnBackground = onBackground;
        Surface = surface;
        OnSurface = onSurface;
        SurfaceVariant = surfaceVariant;
        OnSurfaceVariant = onSurfaceVariant;
        Outline = outline;
        OutlineVariant = outlineVariant;
        Shadow = shadow;
        Scrim = scrim;
        InverseSurface = inverseSurface;
        InverseOnSurface = inverseOnSurface;
        InversePrimary = inversePrimary;
    }

    public int Primary { get; private set; }

    public int OnPrimary { get; private set; }

    public int PrimaryContainer { get; private set; }

    public int OnPrimaryContainer { get; private set; }

    public int Secondary { get; private set; }

    public int OnSecondary { get; private set; }

    public int SecondaryContainer { get; private set; }

    public int OnSecondaryContainer { get; private set; }

    public int Tertiary { get; private set; }

    public int OnTertiary { get; private set; }

    public int TertiaryContainer { get; private set; }

    public int OnTertiaryContainer { get; private set; }

    public int Error { get; private set; }

    public int OnError { get; private set; }

    public int ErrorContainer { get; private set; }

    public int OnErrorContainer { get; private set; }

    public int Background { get; private set; }

    public int OnBackground { get; private set; }

    public int Surface { get; private set; }

    public int OnSurface { get; private set; }

    public int SurfaceVariant { get; private set; }

    public int OnSurfaceVariant { get; private set; }

    public int Outline { get; private set; }

    public int OutlineVariant { get; private set; }

    public int Shadow { get; private set; }

    public int Scrim { get; private set; }

    public int InverseSurface { get; private set; }

    public int InverseOnSurface { get; private set; }

    public int InversePrimary { get; private set; }

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
            .WithPrimary(core.A1.Tone(40))
            .WithOnPrimary(core.A1.Tone(100))
            .WithPrimaryContainer(core.A1.Tone(90))
            .WithOnPrimaryContainer(core.A1.Tone(10))
            .WithSecondary(core.A2.Tone(40))
            .WithOnSecondary(core.A2.Tone(100))
            .WithSecondaryContainer(core.A2.Tone(90))
            .WithOnSecondaryContainer(core.A2.Tone(10))
            .WithTertiary(core.A3.Tone(40))
            .WithOnTertiary(core.A3.Tone(100))
            .WithTertiaryContainer(core.A3.Tone(90))
            .WithOnTertiaryContainer(core.A3.Tone(10))
            .WithError(core.Error.Tone(40))
            .WithOnError(core.Error.Tone(100))
            .WithErrorContainer(core.Error.Tone(90))
            .WithOnErrorContainer(core.Error.Tone(10))
            .WithBackground(core.N1.Tone(99))
            .WithOnBackground(core.N1.Tone(10))
            .WithSurface(core.N1.Tone(99))
            .WithOnSurface(core.N1.Tone(10))
            .WithSurfaceVariant(core.N2.Tone(90))
            .WithOnSurfaceVariant(core.N2.Tone(30))
            .WithOutline(core.N2.Tone(50))
            .WithOutlineVariant(core.N2.Tone(80))
            .WithShadow(core.N1.Tone(0))
            .WithScrim(core.N1.Tone(0))
            .WithInverseSurface(core.N1.Tone(20))
            .WithInverseOnSurface(core.N1.Tone(95))
            .WithInversePrimary(core.A1.Tone(80));
    }

    private static Scheme DarkFromCorePalette(CorePalette core)
    {
        return new Scheme()
            .WithPrimary(core.A1.Tone(80))
            .WithOnPrimary(core.A1.Tone(20))
            .WithPrimaryContainer(core.A1.Tone(30))
            .WithOnPrimaryContainer(core.A1.Tone(90))
            .WithSecondary(core.A2.Tone(80))
            .WithOnSecondary(core.A2.Tone(20))
            .WithSecondaryContainer(core.A2.Tone(30))
            .WithOnSecondaryContainer(core.A2.Tone(90))
            .WithTertiary(core.A3.Tone(80))
            .WithOnTertiary(core.A3.Tone(20))
            .WithTertiaryContainer(core.A3.Tone(30))
            .WithOnTertiaryContainer(core.A3.Tone(90))
            .WithError(core.Error.Tone(80))
            .WithOnError(core.Error.Tone(20))
            .WithErrorContainer(core.Error.Tone(30))
            .WithOnErrorContainer(core.Error.Tone(80))
            .WithBackground(core.N1.Tone(10))
            .WithOnBackground(core.N1.Tone(90))
            .WithSurface(core.N1.Tone(10))
            .WithOnSurface(core.N1.Tone(90))
            .WithSurfaceVariant(core.N2.Tone(30))
            .WithOnSurfaceVariant(core.N2.Tone(80))
            .WithOutline(core.N2.Tone(60))
            .WithOutlineVariant(core.N2.Tone(30))
            .WithShadow(core.N1.Tone(0))
            .WithScrim(core.N1.Tone(0))
            .WithInverseSurface(core.N1.Tone(90))
            .WithInverseOnSurface(core.N1.Tone(20))
            .WithInversePrimary(core.A1.Tone(40));
    }

    public Scheme WithPrimary(int primary)
    {
        Primary = primary;
        return this;
    }

    public Scheme WithOnPrimary(int onPrimary)
    {
        OnPrimary = onPrimary;
        return this;
    }

    public Scheme WithPrimaryContainer(int primaryContainer)
    {
        PrimaryContainer = primaryContainer;
        return this;
    }

    public Scheme WithOnPrimaryContainer(int onPrimaryContainer)
    {
        OnPrimaryContainer = onPrimaryContainer;
        return this;
    }

    public Scheme WithSecondary(int secondary)
    {
        Secondary = secondary;
        return this;
    }

    public Scheme WithOnSecondary(int onSecondary)
    {
        OnSecondary = onSecondary;
        return this;
    }

    public Scheme WithSecondaryContainer(int secondaryContainer)
    {
        SecondaryContainer = secondaryContainer;
        return this;
    }

    public Scheme WithOnSecondaryContainer(int onSecondaryContainer)
    {
        OnSecondaryContainer = onSecondaryContainer;
        return this;
    }

    public Scheme WithTertiary(int tertiary)
    {
        Tertiary = tertiary;
        return this;
    }

    public Scheme WithOnTertiary(int onTertiary)
    {
        OnTertiary = onTertiary;
        return this;
    }

    public Scheme WithTertiaryContainer(int tertiaryContainer)
    {
        TertiaryContainer = tertiaryContainer;
        return this;
    }

    public Scheme WithOnTertiaryContainer(int onTertiaryContainer)
    {
        OnTertiaryContainer = onTertiaryContainer;
        return this;
    }

    public Scheme WithError(int error)
    {
        Error = error;
        return this;
    }

    public Scheme WithOnError(int onError)
    {
        OnError = onError;
        return this;
    }

    public Scheme WithErrorContainer(int errorContainer)
    {
        ErrorContainer = errorContainer;
        return this;
    }

    public Scheme WithOnErrorContainer(int onErrorContainer)
    {
        OnErrorContainer = onErrorContainer;
        return this;
    }

    public Scheme WithBackground(int background)
    {
        Background = background;
        return this;
    }

    public Scheme WithOnBackground(int onBackground)
    {
        OnBackground = onBackground;
        return this;
    }

    public Scheme WithSurface(int surface)
    {
        Surface = surface;
        return this;
    }

    public Scheme WithOnSurface(int onSurface)
    {
        OnSurface = onSurface;
        return this;
    }

    public Scheme WithSurfaceVariant(int surfaceVariant)
    {
        SurfaceVariant = surfaceVariant;
        return this;
    }

    public Scheme WithOnSurfaceVariant(int onSurfaceVariant)
    {
        OnSurfaceVariant = onSurfaceVariant;
        return this;
    }

    public Scheme WithOutline(int outline)
    {
        Outline = outline;
        return this;
    }

    public Scheme WithOutlineVariant(int outlineVariant)
    {
        OutlineVariant = outlineVariant;
        return this;
    }

    public Scheme WithShadow(int shadow)
    {
        Shadow = shadow;
        return this;
    }

    public Scheme WithScrim(int scrim)
    {
        Scrim = scrim;
        return this;
    }

    public Scheme WithInverseSurface(int inverseSurface)
    {
        InverseSurface = inverseSurface;
        return this;
    }

    public Scheme WithInverseOnSurface(int inverseOnSurface)
    {
        InverseOnSurface = inverseOnSurface;
        return this;
    }

    public Scheme WithInversePrimary(int inversePrimary)
    {
        InversePrimary = inversePrimary;
        return this;
    }

    public override string ToString()
    {
        return "Scheme{" +
            "primary=" +
            Primary +
            ", onPrimary=" +
            OnPrimary +
            ", primaryContainer=" +
            PrimaryContainer +
            ", onPrimaryContainer=" +
            OnPrimaryContainer +
            ", secondary=" +
            Secondary +
            ", onSecondary=" +
            OnSecondary +
            ", secondaryContainer=" +
            SecondaryContainer +
            ", onSecondaryContainer=" +
            OnSecondaryContainer +
            ", tertiary=" +
            Tertiary +
            ", onTertiary=" +
            OnTertiary +
            ", tertiaryContainer=" +
            TertiaryContainer +
            ", onTertiaryContainer=" +
            OnTertiaryContainer +
            ", error=" +
            Error +
            ", onError=" +
            OnError +
            ", errorContainer=" +
            ErrorContainer +
            ", onErrorContainer=" +
            OnErrorContainer +
            ", background=" +
            Background +
            ", onBackground=" +
            OnBackground +
            ", surface=" +
            Surface +
            ", onSurface=" +
            OnSurface +
            ", surfaceVariant=" +
            SurfaceVariant +
            ", onSurfaceVariant=" +
            OnSurfaceVariant +
            ", outline=" +
            Outline +
            ", outlineVariant=" +
            OutlineVariant +
            ", shadow=" +
            Shadow +
            ", scrim=" +
            Scrim +
            ", inverseSurface=" +
            InverseSurface +
            ", inverseOnSurface=" +
            InverseOnSurface +
            ", inversePrimary=" +
            InversePrimary +
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

        if (Primary != scheme.Primary)
        {
            return false;
        }
        if (OnPrimary != scheme.OnPrimary)
        {
            return false;
        }
        if (PrimaryContainer != scheme.PrimaryContainer)
        {
            return false;
        }
        if (OnPrimaryContainer != scheme.OnPrimaryContainer)
        {
            return false;
        }
        if (Secondary != scheme.Secondary)
        {
            return false;
        }
        if (OnSecondary != scheme.OnSecondary)
        {
            return false;
        }
        if (SecondaryContainer != scheme.SecondaryContainer)
        {
            return false;
        }
        if (OnSecondaryContainer != scheme.OnSecondaryContainer)
        {
            return false;
        }
        if (Tertiary != scheme.Tertiary)
        {
            return false;
        }
        if (OnTertiary != scheme.OnTertiary)
        {
            return false;
        }
        if (TertiaryContainer != scheme.TertiaryContainer)
        {
            return false;
        }
        if (OnTertiaryContainer != scheme.OnTertiaryContainer)
        {
            return false;
        }
        if (Error != scheme.Error)
        {
            return false;
        }
        if (OnError != scheme.OnError)
        {
            return false;
        }
        if (ErrorContainer != scheme.ErrorContainer)
        {
            return false;
        }
        if (OnErrorContainer != scheme.OnErrorContainer)
        {
            return false;
        }
        if (Background != scheme.Background)
        {
            return false;
        }
        if (OnBackground != scheme.OnBackground)
        {
            return false;
        }
        if (Surface != scheme.Surface)
        {
            return false;
        }
        if (OnSurface != scheme.OnSurface)
        {
            return false;
        }
        if (SurfaceVariant != scheme.SurfaceVariant)
        {
            return false;
        }
        if (OnSurfaceVariant != scheme.OnSurfaceVariant)
        {
            return false;
        }
        if (Outline != scheme.Outline)
        {
            return false;
        }
        if (OutlineVariant != scheme.OutlineVariant)
        {
            return false;
        }
        if (Shadow != scheme.Shadow)
        {
            return false;
        }
        if (Scrim != scheme.Scrim)
        {
            return false;
        }
        if (InverseSurface != scheme.InverseSurface)
        {
            return false;
        }
        if (InverseOnSurface != scheme.InverseOnSurface)
        {
            return false;
        }
        if (InversePrimary != scheme.InversePrimary)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = base.GetHashCode();
        result = 31 * result + Primary;
        result = 31 * result + OnPrimary;
        result = 31 * result + PrimaryContainer;
        result = 31 * result + OnPrimaryContainer;
        result = 31 * result + Secondary;
        result = 31 * result + OnSecondary;
        result = 31 * result + SecondaryContainer;
        result = 31 * result + OnSecondaryContainer;
        result = 31 * result + Tertiary;
        result = 31 * result + OnTertiary;
        result = 31 * result + TertiaryContainer;
        result = 31 * result + OnTertiaryContainer;
        result = 31 * result + Error;
        result = 31 * result + OnError;
        result = 31 * result + ErrorContainer;
        result = 31 * result + OnErrorContainer;
        result = 31 * result + Background;
        result = 31 * result + OnBackground;
        result = 31 * result + Surface;
        result = 31 * result + OnSurface;
        result = 31 * result + SurfaceVariant;
        result = 31 * result + OnSurfaceVariant;
        result = 31 * result + Outline;
        result = 31 * result + OutlineVariant;
        result = 31 * result + Shadow;
        result = 31 * result + Scrim;
        result = 31 * result + InverseSurface;
        result = 31 * result + InverseOnSurface;
        result = 31 * result + InversePrimary;
        return result;
    }
}
