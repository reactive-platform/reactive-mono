namespace Reactive.Yoga;

internal class YogaContext {
    public YogaNode YogaNode {
        get {
            _yogaNode ??= YogaNode.New();
            return _yogaNode;
        }
    }

    public YogaLayoutController? Controller { get; set; }

    ~YogaContext() {
        // Finalizers run on the GC thread, so the node is freed later on the main thread
        _yogaNode?.DisposeLater();
    }

    /// <summary>
    /// Drops the measure function, which captures the measured item. The node table is static, so otherwise the node
    /// keeps the item and this context reachable, and the finalizer never runs.
    /// </summary>
    public void Release() {
        if (_yogaNode is { IsInitialized: true }) {
            _yogaNode.SetMeasureFunc(null);
        }
    }

    private YogaNode? _yogaNode;
}