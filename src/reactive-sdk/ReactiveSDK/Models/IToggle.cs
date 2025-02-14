using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// A toggle abstraction.
/// </summary>
[PublicAPI]
public interface IToggle {
    bool Active { get; }
    
    /// <summary>
    /// Sets the toggle state.
    /// </summary>
    /// <param name="active">Determines the toggle state</param>
    /// <param name="notifyListeners">Determines should event be invoked or not</param>
    /// <param name="force">Determines should the state be changed or not even if it is the same</param>
    void SetActive(bool active, bool notifyListeners = true, bool force = false);
}