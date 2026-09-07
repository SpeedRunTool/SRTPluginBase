namespace SRTPluginBase.Abstractions;

/// <summary>
/// Presentation hints for a settings property.
/// </summary>
/// <remarks>
/// These cover only what <c>System.ComponentModel.DataAnnotations</c> does not. Use the standard
/// attributes for anything they already express - <c>[Display]</c> for the label and description,
/// <c>[Range]</c> to get a slider, <c>[Required]</c> and <c>[RegularExpression]</c> for validation.
/// Validation is enforced in the runner, which is authoritative; the host re-checks the same rules
/// only to give immediate feedback while typing.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtSettingAttribute : Attribute
{
    /// <summary>Group heading to file this setting under. Groups render as expanders.</summary>
    public string? Group { get; init; }

    /// <summary>Sort order within the group. Ties fall back to declaration order.</summary>
    public int Order { get; init; }

    /// <summary>Hide behind "Show advanced". Use for settings that can break things.</summary>
    public bool Advanced { get; init; }

    /// <summary>Longer explanation, shown as help text under the control.</summary>
    public string? HelpText { get; init; }
}

/// <summary>Excludes a property from the generated form entirely.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtHiddenAttribute : Attribute;

/// <summary>Renders a colour picker. Applies to a hex string or a packed ARGB integer.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtColorAttribute : Attribute;

/// <summary>Renders a font picker over the fonts installed on the machine.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtFontAttribute : Attribute;

/// <summary>Renders a key-capture button that records a shortcut.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtHotkeyAttribute : Attribute;

/// <summary>Renders a text box with a Browse button.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SrtPathAttribute : Attribute
{
    /// <summary>Pick a directory rather than a file.</summary>
    public bool Directory { get; init; }

    /// <summary>File filter, e.g. <c>"PNG images|*.png"</c>. Ignored when <see cref="Directory"/> is set.</summary>
    public string? Filter { get; init; }
}

/// <summary>
/// Makes a setting conditional on another one, so a form can hide options that do not currently
/// apply instead of letting the user set something with no effect.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class SrtDependsOnAttribute(string propertyName, object? value) : Attribute
{
    /// <summary>The sibling property to test.</summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>The value it must equal for this setting to apply.</summary>
    public object? Value { get; } = value;

    /// <summary>
    /// Hide the setting when the condition is unmet rather than disabling it. Disabling is usually
    /// kinder - it keeps the option discoverable - so this defaults to false.
    /// </summary>
    public bool HideWhenUnmet { get; init; }
}
