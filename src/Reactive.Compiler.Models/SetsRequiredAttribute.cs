namespace Reactive.Compiler;

/// <summary>
/// Allows to specify which required properties are initialized using this property.
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
///         [SetsRequired(Names = [nameof(Component.Text)])]
///         public string sText {
///             set => comp.Text = value;
///         }
///     } 
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class SetsRequiredAttribute : Attribute {
    public required string[] Names { get; init; }
}