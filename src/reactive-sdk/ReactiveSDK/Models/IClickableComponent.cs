using System;

namespace Reactive.Components {
    public interface IClickableComponent {
        event Action? ClickEvent;
    }
}