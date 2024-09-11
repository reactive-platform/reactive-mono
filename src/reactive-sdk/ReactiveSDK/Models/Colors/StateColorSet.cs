using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class StateColorSet : IColorSet {
        public ICollection<KeyValuePair<GraphicState, Color>> States => StatesDict;
        public Dictionary<GraphicState, Color> StatesDict { get; } = new();

        public event Action? SetUpdatedEvent;

        public Color GetColor(GraphicState state) {
            return StatesDict.TryGetValue(state, out var result) ? result : Color.clear;
        }

        public void SetStateColor(GraphicState graphicState, Color color) {
            StatesDict[graphicState] = color;
            SetUpdatedEvent?.Invoke();
        }

        public void ApplyColorMod(GraphicState baseState, params (GraphicState, Func<Color, Color>)[] mods) {
            if (!StatesDict.TryGetValue(baseState, out var baseColor)) return;

            foreach (var (state, modFunc) in mods) {
                if (StatesDict.ContainsKey(state)) {
                    StatesDict[state] = modFunc(StatesDict[state]);
                } else {
                    StatesDict[state] = modFunc(baseColor);
                }
            }

            SetUpdatedEvent?.Invoke();
        }
    }
}