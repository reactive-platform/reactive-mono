using System;

namespace Reactive.Yoga;

internal class YogaContext {
    public YogaNode YogaNode {
        get {
            if (_freed) {
                throw new ObjectDisposedException(nameof(YogaContext));
            }

            _yogaNode ??= YogaNode.New();
            return _yogaNode;
        }
    }

    public YogaLayoutController? Controller { get; set; }

    private YogaNode? _yogaNode;
    private int _holders;
    private bool _freed;

    public void Retain() {
        if (_freed) {
            throw new ObjectDisposedException(nameof(YogaContext));
        }

        _holders++;
    }

    public void Release() {
        if (--_holders > 0) {
            return;
        }

        _freed = true;
        Controller = null;

        _yogaNode?.Dispose();
        _yogaNode = null;
    }
}