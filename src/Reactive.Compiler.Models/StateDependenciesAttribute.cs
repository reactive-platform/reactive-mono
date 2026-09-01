namespace Reactive.Compiler;

/// <summary>
/// Marks parameter as a state dependency list.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class StateDependenciesAttribute : Attribute;