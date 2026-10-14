using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

internal interface ILayoutScheduled {
    void RunScheduledLayout();
}

/// <summary>
/// The layout layer of <see cref="ReactiveFrame"/>: components that asked for their layout to be calculated again
/// (ScheduleLayoutRecalculation) are calculated here, after the states they show were applied. Each layout root is
/// calculated once per run however many of its components asked.
/// </summary>
[PublicAPI]
public sealed class LayoutLayer : IReactiveLayer {
    private ILayoutScheduled?[] _queue = new ILayoutScheduled?[64];
    private int _count;
    private int _lastRun;

    internal LayoutLayer() { }

    public bool HasWork => _count > 0;

    /// <summary>
    /// The run in progress, 0 outside runs. A layout root calculated in this run is up to date with every request
    /// made before the run started; requests made during it go to the next run.
    /// </summary>
    public int CurrentRun { get; private set; }

    internal void Schedule(ILayoutScheduled item) {
        if (_count == _queue.Length) {
            Array.Resize(ref _queue, _queue.Length * 2);
        }

        _queue[_count++] = item;
    }

    public void Run() {
        var count = _count;
        // Never 0, which means "outside a run"
        _lastRun = _lastRun == int.MaxValue ? 1 : _lastRun + 1;
        CurrentRun = _lastRun;

        try {
            for (var i = 0; i < count; i++) {
                var item = _queue[i]!;
                _queue[i] = null;

                try {
                    item.RunScheduledLayout();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        } finally {
            // Requests made during the run move to the front for the next one
            var left = _count - count;
            Array.Copy(_queue, count, _queue, 0, left);
            Array.Clear(_queue, left, count);
            _count = left;
            CurrentRun = 0;
        }
    }
}
