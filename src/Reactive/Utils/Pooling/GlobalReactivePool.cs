using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactive;

/// <summary>
/// A global pool of components that support hot reinitialization. See <see cref="IPoolableComponent"/>.
/// Despawned components are parked under an inactive root that survives scene changes.
/// </summary>
[PublicAPI]
public static class GlobalReactivePool {
    private static readonly Dictionary<Type, Stack<IPoolableComponent>> reserved = new();
    private static readonly HashSet<IPoolableComponent> spawned = new();

    private static Transform Root {
        get {
            if (!field) {
                var go = new GameObject("GlobalReactivePool");
                go.SetActive(false);
                Object.DontDestroyOnLoad(go);

                field = go.transform;
            }

            return field!;
        }
    }

    /// <summary>
    /// Takes a component from the pool or constructs a new one if there's nothing to reuse.
    /// The component stays parked under the pool root until its user attaches it somewhere.
    /// </summary>
    public static T Spawn<T>() where T : class, IPoolableComponent, new() {
        var comp = TryPop<T>();

        if (comp == null) {
            comp = new T();
            comp.Use(Root);
        }

        spawned.Add(comp);
        comp.OnSpawn();

        return comp;
    }

    /// <summary>
    /// Returns the component to the pool. Destroyed components are dropped.
    /// </summary>
    public static void Despawn(IPoolableComponent comp) {
        if (!spawned.Remove(comp)) {
            throw new InvalidOperationException("Cannot despawn a component that was not spawned by the pool");
        }

        if (!IsAlive(comp)) {
            return;
        }

        comp.OnDespawn();
        comp.Use(Root);

        if (!reserved.TryGetValue(comp.GetType(), out var stack)) {
            stack = new();
            reserved[comp.GetType()] = stack;
        }

        stack.Push(comp);
    }

    /// <summary>
    /// Constructs components in advance, so the first spawns don't have to.
    /// </summary>
    public static void Preload<T>(int count) where T : class, IPoolableComponent, new() {
        var stack = reserved.TryGetValue(typeof(T), out var s) ? s : reserved[typeof(T)] = new();

        for (var i = stack.Count; i < count; i++) {
            var comp = new T();
            comp.Use(Root);
            stack.Push(comp);
        }
    }

    private static T? TryPop<T>() where T : class, IPoolableComponent {
        if (!reserved.TryGetValue(typeof(T), out var stack)) {
            return null;
        }

        while (stack.TryPop(out var comp)) {
            // Could be destroyed along with the hierarchy it was used in
            if (IsAlive(comp)) {
                return (T)comp;
            }
        }

        return null;
    }

    private static bool IsAlive(IPoolableComponent comp) {
        return !comp.IsDestroyed && comp.Content;
    }
}
