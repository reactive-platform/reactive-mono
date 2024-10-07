using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public enum RelativePlacement {
        LeftTop,
        LeftCenter,
        LeftBottom,
        TopRight,
        TopCenter,
        TopLeft,
        RightTop,
        RightCenter,
        RightBottom,
        BottomRight,
        BottomCenter,
        BottomLeft,
        Center
    }

    [PublicAPI]
    public static class ModalAlignmentExtensions {
        #region WithAnchor

        public static T WithAnchor<T>(
            this T comp,
            IReactiveComponent anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            IReactiveComponent anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<IReactiveComponent> anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor().ContentTransform,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<IReactiveComponent> anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor().ContentTransform,
                placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<RectTransform> anchor,
            RelativePlacement placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                anchor,
                () => placement,
                offset,
                unbindOnceOpened
            );
        }

        public static T WithAnchorImmediate<T>(
            this T comp,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2? offset = null
        ) where T : IModal, IReactiveComponent {
            return WithAnchor(
                comp,
                () => anchor,
                () => placement,
                offset,
                false,
                true
            );
        }

        public static T WithAnchor<T>(
            this T comp,
            Func<RectTransform> anchor,
            Func<RelativePlacement> placement,
            Vector2? offset = null,
            bool unbindOnceOpened = true,
            bool immediate = false
        ) where T : IModal, IReactiveComponent {
            if (immediate) {
                HandleModalOpened(comp, false);
            } else {
                comp.ModalOpenedEvent += HandleModalOpened;
            }
            return comp;

            void HandleModalOpened(IModal modal, bool opened) {
                var root = comp.ContentTransform.parent;
                if (root == null) return;
                CalculateRelativePlacement(
                    root,
                    anchor(),
                    placement(),
                    offset.GetValueOrDefault(new(0f, 0.5f)),
                    out var position,
                    out var pivot
                );
                comp.ContentTransform.localPosition = position;
                comp.ContentTransform.pivot = pivot;
                if (!immediate && unbindOnceOpened && comp is not ISharedModal) {
                    modal.ModalOpenedEvent -= HandleModalOpened;
                }
            }
        }

        private static void CalculateRelativePlacement(
            Transform root,
            RectTransform anchor,
            RelativePlacement placement,
            Vector2 offset,
            out Vector2 position,
            out Vector2 pivot
        ) {
            position = root.InverseTransformPoint(anchor.position);
            var rect = anchor.rect;
            var anchorHeightDiv = new Vector2(0f, rect.height / 2);
            var anchorWidthDiv = new Vector2(rect.width / 2, 0f);
            position = placement switch {
                RelativePlacement.LeftTop => position - anchorWidthDiv + anchorHeightDiv - offset,
                RelativePlacement.LeftCenter => position - anchorWidthDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.LeftBottom => position - anchorWidthDiv - anchorHeightDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.TopLeft => position + anchorHeightDiv - anchorWidthDiv + offset,
                RelativePlacement.TopCenter => position + anchorHeightDiv + offset,
                RelativePlacement.TopRight => position + anchorHeightDiv + anchorWidthDiv + new Vector2(-offset.x, offset.y),
                RelativePlacement.RightTop => position + anchorWidthDiv + anchorHeightDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.RightCenter => position + anchorWidthDiv + offset,
                RelativePlacement.RightBottom => position + anchorWidthDiv - anchorHeightDiv + offset,
                RelativePlacement.BottomLeft => position - anchorHeightDiv - anchorWidthDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.BottomCenter => position - anchorHeightDiv + new Vector2(offset.x, -offset.y),
                RelativePlacement.BottomRight => position - anchorHeightDiv + anchorWidthDiv - offset,
                RelativePlacement.Center => position + offset + rect.size * Vector2.one * 0.5f - rect.size * anchor.pivot,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
            pivot = placement switch {
                RelativePlacement.LeftTop => new(1f, 1f),
                RelativePlacement.LeftCenter => new(1f, 0.5f),
                RelativePlacement.LeftBottom => new(1f, 0f),
                RelativePlacement.TopLeft => new(0f, 0f),
                RelativePlacement.TopCenter => new(0.5f, 0f),
                RelativePlacement.TopRight => new(1f, 0f),
                RelativePlacement.RightTop => new(0f, 1f),
                RelativePlacement.RightCenter => new(0f, 0.5f),
                RelativePlacement.RightBottom => new(0f, 0f),
                RelativePlacement.BottomLeft => new(0f, 1f),
                RelativePlacement.BottomCenter => new(0.5f, 1f),
                RelativePlacement.BottomRight => new(1f, 1f),
                RelativePlacement.Center => Vector2.one * 0.5f,
                _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
            };
        }

        #endregion
    }
}