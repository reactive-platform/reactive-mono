using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class ImageExtensions {
    /// <summary>
    /// Loads a remote picture and applies to the image.
    /// </summary>
    /// <param name="url">A url to load the data from.</param>
    /// <param name="onStart">Called when a picture has started loading.</param>
    /// <param name="onFinish">Called when a picture has finished or failed loading.</param>
    public static T WithWebSource<T>(
        this T comp,
        string url,
        Action? onStart = null,
        Action<bool>? onFinish = null
    ) where T : IComponentHolder<ISpriteRenderer>, IComponentHolder<IReactiveModuleBinder> {
        var binder = (comp as IComponentHolder<IReactiveModuleBinder>).Component;
        var renderer = (comp as IComponentHolder<ISpriteRenderer>).Component;

        if (ResolveModule(binder) is not { } loaderModule) {
            loaderModule = new(renderer);
            binder.BindModule(loaderModule);

            if (renderer is IObservableHost observableRenderer && renderer is ISpriteRenderer spriteRenderer) {
                observableRenderer.WithListener(x => ((ISpriteRenderer)x).Sprite, (newSprite) => {
                    if (newSprite != loaderModule.LoadedImage?.Sprite) {
                        loaderModule.StopLoading();
                    }
                });
            }
        }

        loaderModule.LoadRemote(url, onStart, onFinish);

        return comp;
    }

    /// <summary>
    /// Cancels a picture loading task if it's currently running.
    /// </summary>
    public static T CancelWebLoading<T>(this T comp) where T : IComponentHolder<ISpriteRenderer>, IComponentHolder<IReactiveModuleBinder> {
        var binder = (comp as IComponentHolder<IReactiveModuleBinder>).Component;

        ResolveModule(binder)?.StopLoading();

        return comp;
    }

    private static ImageLoaderModule? ResolveModule(IReactiveModuleBinder binder) {
        return binder.Modules.FirstOrDefault(x => x.GetType() == typeof(ImageLoaderModule)) as ImageLoaderModule;
    }
}