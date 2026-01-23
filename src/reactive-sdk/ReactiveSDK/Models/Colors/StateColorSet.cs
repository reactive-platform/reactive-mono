using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    /// <summary>
    /// Determines how <see cref="StateColorSet"/> acquires a color for an unrepresented state.
    /// </summary>
    [PublicAPI]
    public enum StateResolutionMode {
        /// <summary>
        /// Returns the default color if not present.
        /// </summary>
        Default,

        /// <summary>
        /// Throws an exception if required state is not present.
        /// </summary>
        Throw,

        /// <summary>
        /// Attempts to find the closest state to the required one.
        /// If fails returns the default color.
        /// </summary>
        Auto
    }

    [PublicAPI]
    public class StateColorSet : IColorSet {
        public ICollection<KeyValuePair<GraphicState, Color>> States => StatesDict;
        public Dictionary<GraphicState, Color> StatesDict { get; } = new();

        public event Action? SetUpdatedEvent;

        public StateResolutionMode ResolutionMode = StateResolutionMode.Auto;
        public Color DefaultColor = Color.clear;

        public Color GetColor(GraphicState state) {
            if (StatesDict.TryGetValue(state, out var result)) {
                return result;
            }
            switch (ResolutionMode) {
                case StateResolutionMode.Default:
                    return DefaultColor;
                case StateResolutionMode.Auto:
                    if (state == 0) return DefaultColor;
                    state--;
                    return GetColor(state);
                case StateResolutionMode.Throw:
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public void SetStateColor(GraphicState graphicState, Color color) {
            StatesDict[graphicState] = color;
            SetUpdatedEvent?.Invoke();
        }
    }
}