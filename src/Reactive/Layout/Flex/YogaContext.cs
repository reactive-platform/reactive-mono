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
        _yogaNode?.Dispose();
    }

    private YogaNode? _yogaNode;
}