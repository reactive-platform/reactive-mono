using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

internal interface IScheduledState {
    void Deliver();
    void Cancel();
}

/// <summary>
/// Runs state callbacks: the states layer of <see cref="ReactiveFrame"/>. When a state changes, the states derived
/// from it learn at once but only forget their value; the callbacks run later, once, with the latest value. A state
/// derived from 3 others that all change is evaluated once, with all 3 new values, and calls back once.
/// <para>
/// Values are always current: reading a state's Value evaluates it if needed. Only callbacks wait, until the frame
/// runs its layers (before canvases render), or <see cref="Flush"/>.
/// </para>
/// </summary>
[PublicAPI]
public static class StateScheduler {
    // A flush delivering more than this is a cycle: states setting each other forever
    private const int MaxDeliveries = 100_000;

    private static IScheduledState?[] _queue = new IScheduledState?[64];
    private static int _count;
    private static int _depth;
    private static bool _flushing;
    private static bool _deferred = true;

    static StateScheduler() {
        ReactiveFrame.Initialize();
    }

    /// <summary>
    /// When true (the default), callbacks run at the next flush. When false, before the change returns:
    /// after the states derived from it have learnt about it, so they're still evaluated once per change.
    /// </summary>
    public static bool Deferred {
        get => _deferred;
        set {
            _deferred = value;

            if (!value) {
                Flush();
            }
        }
    }

    /// <summary>
    /// Whether some callbacks wait for a flush.
    /// </summary>
    public static bool HasPending => _count > 0;

    /// <summary>
    /// Holds callbacks until the scope is disposed, when <see cref="Deferred"/> is false
    /// (they always wait when it's true).
    /// </summary>
    public static BatchScope Batch() {
        BeginNotification();
        return new(true);
    }

    [PublicAPI]
    public readonly struct BatchScope : IDisposable {
        private readonly bool _active;

        internal BatchScope(bool active) {
            _active = active;
        }

        public void Dispose() {
            if (_active) {
                EndNotification();
            }
        }
    }

    /// <summary>
    /// Runs the callbacks of the states that changed, including those of states changed by these callbacks.
    /// </summary>
    public static void Flush() {
        if (_flushing || _count == 0) {
            return;
        }

        _flushing = true;
        var i = 0;

        try {
            // Callbacks may schedule more states: they're appended and delivered in this loop
            for (; i < _count; i++) {
                if (i == MaxDeliveries) {
                    Debug.LogError($"Reactive: states kept changing each other while delivering {MaxDeliveries} updates; {_count - i} not delivered");
                    break;
                }

                var state = _queue[i]!;
                _queue[i] = null;

                try {
                    state.Deliver();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        } finally {
            for (; i < _count; i++) {
                _queue[i]!.Cancel();
                _queue[i] = null;
            }

            _count = 0;
            _flushing = false;
        }
    }

    internal static void Schedule(IScheduledState state) {
        if (_count == _queue.Length) {
            Array.Resize(ref _queue, _queue.Length * 2);
        }

        _queue[_count++] = state;
    }

    internal static void BeginNotification() {
        _depth++;
    }

    internal static void EndNotification() {
        _depth--;

        if (_depth == 0 && !_deferred) {
            Flush();
        }
    }
}
