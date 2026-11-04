using System;
using System.Collections;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Provides general extensions for building composable UIs.
/// </summary>
[PublicAPI]
public static class ComposableExtensions {
    extension<T>(T comp) where T : IReactiveComponent {
        /// <summary>
        /// Calls provided callback immediately.
        /// </summary>
        public Action<T> Do {
            set => value(comp);
        }
    }
}