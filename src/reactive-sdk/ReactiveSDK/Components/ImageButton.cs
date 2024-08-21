using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class ImageButton : ButtonBase {
        public new ICollection<ILayoutItem> Children => base.Children;

        public Image Image { get; private set; } = null!;

        protected override GameObject Construct() {
            Image = new();
            return Image.Use();
        }
    }
}