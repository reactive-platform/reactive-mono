using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Runs Reactive's work for the frame in layers, the lowest first: states (bindings get the values changed during
/// the frame), then layout (with the sizes these values give), then any layer added above. When a layer schedules
/// work into a lower one, as a layout event that changes a state does, the frame goes back to it before continuing.
/// <para>
/// Runs once per frame before canvases render (Canvas.preWillRenderCanvases, before UGUI rebuilds what changed), and
/// when <see cref="Run"/> or <see cref="RunUntil"/> is called.
/// </para>
/// </summary>
[PublicAPI]
public static class ReactiveFrame {
    public const int StatesOrder = 0;
    public const int LayoutOrder = 1000;

    // Passes over the layers in one run: more means layers keep scheduling each other's work.
    // What's left stays scheduled for the next frame
    private const int MaxPasses = 100;

    private static readonly List<(int order, IReactiveLayer layer)> layers = new();
    private static bool _running;

    /// <summary>
    /// The states layer: bindings and other callbacks of the states changed since the last run.
    /// </summary>
    public static IReactiveLayer States { get; } = new StateLayer();

    /// <summary>
    /// The layout layer: components that need their layout calculated again.
    /// </summary>
    public static LayoutLayer Layout { get; } = new();

    static ReactiveFrame() {
        AddLayer(States, StatesOrder);
        AddLayer(Layout, LayoutOrder);
        Canvas.preWillRenderCanvases += Run;
    }

    // Lets StateScheduler make sure the frame runs
    internal static void Initialize() { }

    /// <summary>
    /// Adds a layer. Layers with a lower order run first; one with the same order as another runs after it.
    /// </summary>
    public static void AddLayer(IReactiveLayer layer, int order) {
        var index = layers.FindIndex(x => x.order > order);
        layers.Insert(index < 0 ? layers.Count : index, (order, layer));
    }

    public static bool RemoveLayer(IReactiveLayer layer) {
        return layers.RemoveAll(x => x.layer == layer) > 0;
    }

    /// <summary>
    /// Runs every layer with work, until none has any. Does nothing when called from a layer.
    /// </summary>
    public static void Run() {
        RunUntil(int.MaxValue);
    }

    /// <summary>
    /// Runs the layers up to this order (included), until none of them has work: RunUntil(StatesOrder) applies
    /// states without calculating layouts. Does nothing when called from a layer.
    /// </summary>
    public static void RunUntil(int order) {
        if (_running) {
            return;
        }

        _running = true;

        try {
            for (var pass = 0; ; pass++) {
                var layer = FindLayerWithWork(order);
                if (layer == null) {
                    break;
                }

                if (pass == MaxPasses) {
                    Debug.LogError($"Reactive: layers kept scheduling each other's work for {MaxPasses} passes, left for the next frame");
                    break;
                }

                try {
                    layer.Run();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        } finally {
            _running = false;
        }
    }

    private static IReactiveLayer? FindLayerWithWork(int order) {
        for (var i = 0; i < layers.Count; i++) {
            var (layerOrder, layer) = layers[i];

            if (layerOrder > order) {
                break;
            }

            if (layer.HasWork) {
                return layer;
            }
        }

        return null;
    }

    private sealed class StateLayer : IReactiveLayer {
        public bool HasWork => StateScheduler.HasPending;

        public void Run() {
            StateScheduler.Flush();
        }
    }
}
