using System;

namespace Reactive.Components;

public interface IStatedComponent {
    event Action<bool>? StateChangedEvent;
}