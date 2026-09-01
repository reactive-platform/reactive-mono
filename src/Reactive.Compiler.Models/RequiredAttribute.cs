namespace Reactive.Compiler;

/// <summary>
/// Makes the property required to construct the containing component.
/// Acts as a replacement to the builtin keyword <see langword="required"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute;