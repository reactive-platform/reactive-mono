using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reactive {
    [PublicAPI]
    public static class StateExtensions {
        #region Queries

        /// <param name="state">A state to wrap.</param>
        /// <typeparam name="T">An initial type of the state.</typeparam>
        extension<T>(IState<T> state) {
            /// <summary>
            /// Maps a state to another type.
            /// </summary>
            /// <param name="predicate">A mapping predicate.</param>
            /// <typeparam name="TMap">A type of the mapped state.</typeparam>
            /// <returns>A wrapper over the original state instance.</returns>
            public MappedState<T, TMap> Map<TMap>(Func<T, TMap> predicate) {
                return new(state, predicate);
            }

            /// <summary>
            /// Filters state updates by some condition.
            /// </summary>
            /// <param name="predicate">A filtering predicate.</param>
            /// <returns>A wrapper over the original state instance.</returns>
            public BranchedState<T> Where(Func<T, bool> predicate) {
                return new(state, predicate);
            }
        }

        #endregion

        #region On

        extension<T>(IState<T> state) {
            /// <summary>
            /// Binds a callback to some state.
            /// </summary>
            /// <param name="callback">A callback to bind.</param>
            /// <param name="lazy">Whether the callback should be invoked immediately.</param>
            public StateSubscription AddCallback(Action<T> callback, bool lazy = false) {
                var sub = state.AddCallback(Wrapper, callback, null!);

                if (!lazy) {
                    var refHandle = new RefStateSubscription(true);
                    Wrapper(ref refHandle, state.Value, callback, null!);

                    if (!refHandle.GetIsValid()) {
                        state.RemoveCallback(sub);
                    }
                }

                return sub;

                static void Wrapper(ref RefStateSubscription sub, T val, object arg1, object arg2) {
                    ((Action<T>)arg1)(val);
                }
            }

            /// <summary>
            /// Binds a callback to some state capturing an IReactiveComponent.
            /// </summary>
            /// <param name="comp">A component to capture.</param>
            /// <param name="callback">A callback to bind.</param>
            /// <param name="lazy">Whether the callback should be invoked immediately.</param>
            public StateSubscription AddCallback<TComp>(TComp comp, Action<TComp, T> callback, bool lazy = false) where TComp : IReactiveComponent {
                var sub = state.AddCallback(Wrapper, comp, callback);

                if (!lazy) {
                    var refHandle = new RefStateSubscription(true);
                    Wrapper(ref refHandle, state.Value, comp, callback);

                    if (!refHandle.GetIsValid()) {
                        state.RemoveCallback(sub);
                    }
                }

                return sub;

                static void Wrapper(ref RefStateSubscription sub, T val, object arg1, object arg2) {
                    var comp = (TComp)arg1;
                    var callback = (Action<TComp, T>)arg2;

                    // Return if component is not valid yet
                    if (!comp.IsInitialized) {
                        return;
                    }

                    // Unsubscribe if component is not valid anymore
                    if (comp.IsDestroyed) {
                        sub.RemoveCallback();
                        return;
                    }

                    callback(comp, val);
                }
            }
        }

        // Note: methods use duplicated logic rather than wrapping
        // as each wrap costs a heap allocation
        extension<T>(T comp) where T : IReactiveComponent {
            /// <summary>
            /// Binds a callback to some state.
            /// </summary>
            /// <param name="state">A state to bind to.</param>
            /// <param name="callback">A callback to bind.</param>
            /// <param name="lazy">Whether the callback should be invoked immediately.</param>
            public T On<TValue>(IState<TValue> state, Action<T, TValue> callback, bool lazy = false) {
                state.AddCallback(comp, callback, lazy);
                return comp;
            }
        }

        #endregion
    }
}