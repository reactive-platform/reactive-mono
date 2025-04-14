using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public static class BasicModalExtensions {
        #region Present

        public static void Present<T>(this T comp, Transform screen, bool animated = true) where T : IModal, IReactiveComponent {
            ModalSystem.PresentModal(comp, screen, animated);
        }

        #endregion

        #region WithModal

        public static T WithModal<T, TModal>(this T holder, TModal modal, bool animated = true)
            where T : IComponentHolder<ButtonBase>
            where TModal : IModal, IReactiveComponent {
            var comp = holder.Component;
            comp.OnClick += () => modal.Present(comp.ContentTransform, animated);

            return holder;
        }

        #endregion
    }
}