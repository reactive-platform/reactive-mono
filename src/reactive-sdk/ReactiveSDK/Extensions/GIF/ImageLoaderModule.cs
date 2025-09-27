using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public class ImageLoaderModule(ISpriteRenderer renderer) : IReactiveModule {
    public CachedImage? LoadedImage;

    private CancellationTokenSource _tokenSource = new();
    private Task? _loadTask;

    public void StopLoading() {
        if (!_loadTask?.IsCompleted ?? false) {
            _tokenSource.Cancel();
            _tokenSource = new();
        }

        LoadedImage = null;
    }

    public void LoadRemote(string url, Action? onStart, Action<bool>? onFinish) {
        StopLoading();

        LoadedImage = null;
        _loadTask = LoadRemoteInternal(url, onStart, onFinish, _tokenSource.Token);
    }

    private async Task LoadRemoteInternal(
        string url,
        Action? onStart, 
        Action<bool>? onFinish,
        CancellationToken token
    ) {
        try {
            onStart?.Invoke();

            var image = await ImageLoader.LoadImage(url, token);

            if (image == null) {
                Debug.LogError("Remote picture has failed to load");
                return;
            }

            if (token.IsCancellationRequested) {
                return;
            }

            renderer.Sprite = image.Sprite;
            LoadedImage = image;

            onFinish?.Invoke(true);
        } catch (TaskCanceledException) {
            // do nothing
        } catch (Exception ex) {
            Debug.LogError($"Image loading has failed: {ex}");
            
            onFinish?.Invoke(false);
        }
    }

    public void OnUpdate() {
        if (LoadedImage?.IsAnimated ?? false) {
            LoadedImage.ManualUpdate(Time.deltaTime);
        }
    }

    public void OnBind() { }

    public void OnUnbind() {
        StopLoading();
    }
}