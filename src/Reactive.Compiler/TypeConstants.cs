namespace Reactive.Compiler;

/// <summary>
/// Full names of types the compiler recognizes but can't reference directly, since analyzers can't depend
/// on Reactive or Unity. Types linked into the compiler, like the attributes, should use <c>typeof</c> instead.
/// </summary>
internal static class TypeConstants {
    public const string ReactiveNamespace = "Reactive";

    public const string ReactiveComponent = $"{ReactiveNamespace}.ReactiveComponent";
    public const string LifetimeProvider = $"{ReactiveNamespace}.ILifetimeProvider";

    public const string StateName = "IState";
    public const string State = $"{ReactiveNamespace}.{StateName}";
    public const string StateBinder = $"{ReactiveNamespace}.Compiler.StateBinder";

    public const string UnityObject = "UnityEngine.Object";
}
