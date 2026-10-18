using JetBrains.Annotations;

namespace Reactive.Components;

[PublicAPI]
public static class ImageExtensions {
    extension(IReactiveModuleBinder binder) {
        /// <summary>
        /// Creates a GIF player and binds it.
        /// </summary>
        public GifPlayer RememberGifPlayer() {
            var player = new GifPlayer();
            binder.BindModule(player);
            return player;
        }
    }
}
