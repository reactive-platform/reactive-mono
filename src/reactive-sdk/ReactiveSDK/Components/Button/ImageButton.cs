using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public class ImageButton : ColoredButton, IChildrenProvider{
        #region UI Props

        public new ICollection<ILayoutItem> Children => base.Children;

        #endregion

        #region UI Components

        public Image Image { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color) {
            if (Colors != null) {
                Image.Color = color;
            }
        }

        protected override void OnInteractableChange(bool interactable) {
            UpdateColor();
        }

        #endregion

        #region Setup

        protected override void Construct(RectTransform rect) {
            //background
            Image = new Image {
                Name = "Background"
            }.WithRectExpand();
            Image.Use(rect);
            //content
            base.Construct(rect);
        }

        #endregion
    }
}