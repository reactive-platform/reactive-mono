using System;
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
                return state.AddCallbackInternal(lazy, Wrapper, callback, null!);

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
            public StateSubscription AddCallback<TComp>(TComp comp, Action<TComp, T> callback, bool lazy = false) where TComp : ILifetimeProvider {
                return state.AddCallbackInternal(lazy, Wrapper, comp, callback);

                static void Wrapper(ref RefStateSubscription sub, T val, object arg1, object arg2) {
                    var comp = (TComp)arg1;
                    var callback = (Action<TComp, T>)arg2;

                    // Unsubscribe if component is not valid anymore
                    if (!comp.IsAlive) {
                        sub.RemoveCallback();
                        return;
                    }

                    callback(comp, val);
                }
            }

            /// <summary>
            /// Binds a callback to some state capturing a unity object.
            /// </summary>
            /// <param name="comp">A component to capture.</param>
            /// <param name="callback">A callback to bind.</param>
            /// <param name="lazy">Whether the callback should be invoked immediately.</param>
            public StateSubscription AddCallbackUnity<TComp>(TComp comp, Action<TComp, T> callback, bool lazy = false) where TComp : UnityEngine.Object {
                return state.AddCallbackInternal(lazy, Wrapper, comp, callback);

                static void Wrapper(ref RefStateSubscription sub, T val, object arg1, object arg2) {
                    var comp = (TComp)arg1;
                    var callback = (Action<TComp, T>)arg2;

                    // Unsubscribe if component is not valid anymore
                    if (!comp.IsAlive) {
                        sub.RemoveCallback();
                        return;
                    }

                    callback(comp, val);
                }
            }

            private StateSubscription AddCallbackInternal(bool lazy, StateCallback<T> callback, object arg1, object arg2) {
                var sub = state.AddCallback(callback, arg1, arg2);

                if (!lazy) {
                    var refHandle = new RefStateSubscription(true);
                    callback(ref refHandle, state.Value, arg1, arg2);

                    if (!refHandle.GetIsValid()) {
                        sub.RemoveCallback();
                    }
                }

                return sub;
            }
        }

        // Note: methods use duplicated logic rather than wrapping
        // as each wrap costs a heap allocation
        extension<TComp>(TComp comp) where TComp : ILifetimeProvider {
            /// <summary>
            /// Binds a callback to some state.
            /// </summary>
            /// <param name="state">A state to bind to.</param>
            /// <param name="callback">A callback to bind.</param>
            /// <param name="lazy">Whether the callback should be invoked immediately.</param>
            public TComp On<T>(IState<T> state, Action<TComp, T> callback, bool lazy = false) {
                state.AddCallback(comp, callback, lazy);
                return comp;
            }
        }

        #endregion
    }
}