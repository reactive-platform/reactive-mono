namespace Reactive.Compiler;

/// <summary>
/// Makes the property required to construct the containing component.
/// Acts as a replacement to the builtin keyword <see langword="required"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute {
    /// <summary>
    /// Specifies the name of a property it shadows if it does.
    /// This is only applicable to extensions meant to assign compiler-required fields.
    /// </summary>
    /// <example>
    /// Here we define a component with a required property 'Text'. Without Required on
    /// the extension property it wouldn't compile because the initial property is not assigned.
    /// <code>
    /// class Component {
    ///     [Required]
    ///     public string Text { get; set; }
    /// }
    /// 
    /// static class Ext {
    ///     extension(Component comp) {
    ///         [Required(ShadowsName = nameof(Component.Text))]
    ///         public string sText {
    ///             set => comp.Text = value;
    ///         }
    ///     } 
    /// }
    /// </code>
    /// </example>
    public string? ShadowsName { get; set; }
}