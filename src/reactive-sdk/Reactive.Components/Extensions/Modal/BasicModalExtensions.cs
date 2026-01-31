using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public static class BasicModalExtensions {
        #region Present

        public static void Present(this IModal comp, Transform screen, bool animated = true) {
            ModalSystem.PresentModal(comp, screen, animated);
        }

        #endregion

        #region WithModal

        public static T WithModal<T>(this T holder, IModal modal, bool animated = true) where T : IComponentHolder<ButtonBase> {
            var comp = holder.Component;
            comp.OnClick += () => modal.Present(comp.ContentTransform, animated);

            return holder;
        }

        #endregion
    }
}