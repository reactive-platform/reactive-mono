namespace Reactive.Compiler;

/// <summary>
/// Marks a delegate parameter whose lambdas must not capture states.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class NoStateCaptureAttribute : Attribute;
