using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Leaves the application to a callback.
/// </summary>
[PublicAPI]
public sealed class CallbackLayoutApplicator : LayoutApplicator {
    public delegate bool ApplyCallback(LayoutDimensions current, LayoutDimensions target, out LayoutDimensions dimensions);

    public CallbackLayoutApplicator(ApplyCallback callback) {
        _callback = callback;
    }

    private readonly ApplyCallback _callback;

    public override bool Apply(LayoutDimensions current, LayoutDimensions target, out LayoutDimensions dimensions) {
        return _callback(current, target, out dimensions);
    }
}