using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public partial class ScrollArea : Reactive.Components.Basic.ScrollArea {
#if !COMPILE_EDITOR
    protected override void OnInitialize() {
        Content.AddComponent<VRScrollAdapter>();
        base.OnInitialize();
    }
#endif
}