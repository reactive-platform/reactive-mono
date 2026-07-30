using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Represents an object that has a lifetime. Used by the compiler to determine
/// whether a state binding remains valid. By default, you can use state generator on
/// unity objects and classes that implement IReactiveComponent. To use it on your own
/// classes you must implement this interface.
/// </summary>
[PublicAPI]
public interface ILifetimeProvider {
    bool IsAlive { get; }
}

[PublicAPI]
public static class LifetimeExtensions {
    extension(UnityEngine.Object obj) {
        // Allows the compiler to blindly do IsAlive calls
        public bool IsAlive => obj != null;
    }
}