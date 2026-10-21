using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Runs the work scheduled during the frame in layers: <see cref="State"/> first, so states are evaluated once
/// however many times their dependencies changed, then <see cref="Layout"/> with the sizes they give. If layout
/// changes states, both run again. Runs once per frame before canvases render, after every LateUpdate, and when
/// <see cref="Run"/> is called.
/// </summary>
[PublicAPI]
public static class ReactiveScheduler {
    // Runs of both layers in a frame; more means they keep scheduling each other's work
    private const int MaxPasses = 100;

    public static readonly StateLayer State = new();
    public static readonly LayoutLayer Layout = new();

    private static bool _running;

    static ReactiveScheduler() {
        Canvas.preWillRenderCanvases += Run;
    }

    /// <summary>
    /// Runs both layers until neither has work. Does nothing when called while a layer runs.
    /// </summary>
    public static void Run() {
        if (_running || State.IsRunning || Layout.IsRunning) {
            return;
        }

        _running = true;

        try {
            for (var pass = 0; State.HasWork || Layout.HasWork; pass++) {
                if (pass == MaxPasses) {
                    Debug.LogError($"Reactive: states and layout kept scheduling each other for {MaxPasses} passes, the rest is left for the next frame");
                    break;
                }

                State.Run();
                Layout.Run();
            }
        }
        finally {
            _running = false;
        }
    }

    [PublicAPI]
    public sealed class StateLayer {
        // More flushes in a run means states keep changing each other
        private const int MaxFlushes = 100_000;

        private readonly Queue<IState> _queue = new();

        internal StateLayer() { }

        public bool HasWork => _queue.Count > 0;

        internal bool IsRunning { get; private set; }

        public void Schedule(IState state) {
            _queue.Enqueue(state);
        }

        /// <summary>
        /// Flushes the scheduled states, and the states their callbacks change.
        /// </summary>
        public void Run() {
            if (IsRunning) {
                return;
            }

            IsRunning = true;

            try {
                for (var i = 0; _queue.Count > 0; i++) {
                    if (i == MaxFlushes) {
                        Debug.LogError($"Reactive: states kept changing each other for {MaxFlushes} flushes, the rest is left for the next run");
                        break;
                    }

                    try {
                        _queue.Dequeue().Flush();
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
            finally {
                IsRunning = false;
            }
        }
    }

    [PublicAPI]
    public sealed class LayoutLayer {
        private readonly Queue<ILayoutNode> _queue = new();
        private readonly List<ILayoutNode> _roots = new();
        private int _runs;

        internal LayoutLayer() { }

        public bool HasWork => _queue.Count > 0;

        internal bool IsRunning => CurrentRun != 0;

        /// <summary>
        /// The run in progress, 0 outside runs. A layout root calculated in this run is up to date with the
        /// requests made before it started; requests made during it are calculated in the next one.
        /// </summary>
        public int CurrentRun { get; private set; }

        private int NextRun => _runs == int.MaxValue ? 1 : _runs + 1;

        public void Schedule(ILayoutNode node) {
            ref var state = ref node.LayoutState;
            var run = NextRun;

            if (state.QueuedFor == run) {
                return;
            }

            state.QueuedFor = run;
            _queue.Enqueue(node);
        }

        public void Run() {
            if (IsRunning || _queue.Count == 0) {
                return;
            }

            _runs = NextRun;
            CurrentRun = _runs;

            try {
                for (var count = _queue.Count; count > 0; count--) {
                    var node = _queue.Dequeue();

                    // An item outside the layout is scheduled again when it's enabled
                    if (node is ILayoutItem { WithinLayout: false }) {
                        continue;
                    }

                    node.LayoutState.QueuedFor = 0;

                    try {
                        CollectRoot(node);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }

                for (var i = 0; i < _roots.Count; i++) {
                    try {
                        _roots[i].RecalculateLayout();
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
            finally {
                _roots.Clear();
                CurrentRun = 0;
            }
        }

        private void CollectRoot(ILayoutNode node) {
            var run = CurrentRun;

            while (node is ILayoutItem item) {
                ref var state = ref item.LayoutState;

                if (state.PropagatedFor == run) {
                    return;
                }

                state.PropagatedFor = run;

                if (!LayoutTool.IsControlledByParent(item)) {
                    break;
                }

                node = item.LayoutDriver!;
            }

            ref var rootState = ref node.LayoutState;
            if (rootState.RootFor == run) {
                return;
            }

            rootState.RootFor = run;
            _roots.Add(node);
        }
    }
}