using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public class ImageLoaderModule(ISpriteRenderer renderer) : IReactiveModule {
    private CancellationTokenSource _tokenSource = new();
    private CachedImage? _image;
    private Task? _loadTask;

    public void StopLoading() {
        if (!_loadTask?.IsCompleted ?? false) {
            _tokenSource.Cancel();
            _tokenSource = new();
        }
    }

    public void LoadRemote(string url) {
        StopLoading();

        _image = null;
        _loadTask = LoadRemoteInternal(url, _tokenSource.Token);
    }

    private async Task LoadRemoteInternal(string url, CancellationToken token) {
        try {
            var image = await ImageLoader.LoadRemoteImage(url, token);

            if (image == null) {
                Debug.LogError("Remote picture has failed to load");
                return;
            }

            if (token.IsCancellationRequested) {
                return;
            }

            renderer.Sprite = image.Sprite;
            _image = image;
        } catch (Exception ex) {
            Debug.LogError($"The load has has failed: {ex}");
        }
    }

    public void OnUpdate() {
        if (_image?.IsAnimated ?? false) {
            _image.ManualUpdate(Time.deltaTime);
        }
    }

    public void OnBind() { }

    public void OnUnbind() {
        StopLoading();
    }
}