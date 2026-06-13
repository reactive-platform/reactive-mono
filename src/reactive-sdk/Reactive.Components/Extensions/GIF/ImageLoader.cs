using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using B83.Image.GIF;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Reactive.Components;

[PublicAPI]
public static class ImageLoader {
    public static IDictionary<string, CachedImage> CachedImages => images;

    private static readonly Dictionary<string, CachedImage> images = new();
    private static readonly HttpClient client = new();

    private static readonly Dictionary<string, SemaphoreSlim> semaphores = new();
    private static readonly object semaphoresLock = new();

    /// <summary>
    /// Loads an image from the provided location. Can be either a remote url, an assembly path or a file.
    /// </summary>
    /// <param name="location">A location to load the data from.</param>
    /// <param name="token">A cancellation token.</param>
    public static async Task<CachedImage?> LoadImage(string location, CancellationToken token) {
        var semaphore = GetSemaphore(location);
        await semaphore.WaitAsync(token);

        if (images.TryGetValue(location, out var image)) {
            return image;
        }

        try {
            if (IsRemote(location)) {
                if (IsPotentiallyAnimated(location)) {
                    image = await LoadAnyRemote(location, token);
                } else {
                    // If the image isn't animated, use an optimized request version
                    // to load directly to a native texture, avoiding managed allocations
                    image = await LoadStaticRemote(location);
                }
            } else {
                Stream? stream = null;

                if (TryGetAssembly(location, out var asm, out var asmPath)) {
                    //
                    stream = asm!.GetManifestResourceStream(asmPath);
                    //
                } else if (File.Exists(location)) {
                    //
                    stream = File.OpenRead(location);
                }

                if (stream != null) {
                    try {
                        image = await LoadImageFromStream(stream, null, token);
                    }
                    finally {
                        stream.Dispose();
                    }
                }
            }

            if (image != null) {
                images[location] = image;
            }

            return image;
        }
        finally {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Loads an image from the specified buffer.
    /// </summary>
    /// <param name="bytes">A buffer to load from.</param>
    /// <param name="token">A cancellation token.</param>
    public static async Task<CachedImage?> LoadImageFromBytes(byte[] bytes, CancellationToken token) {
        using var stream = new MemoryStream(bytes);
        
        return await LoadImageFromStream(stream, bytes, token);
    }
    
    public static void RemoveCached(string location) {
        images.Remove(location);
    }

    private static SemaphoreSlim GetSemaphore(string location) {
        lock (semaphoresLock) {
            if (!semaphores.TryGetValue(location, out var semaphore)) {
                semaphore = new(1, 1);
                semaphores[location] = semaphore;
            }

            return semaphore;
        }
    }

    #region Remote

    private static bool IsRemote(string location) {
        return location.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            location.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPotentiallyAnimated(string location) {
        return location.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<CachedImage?> LoadStaticRemote(string location) {
        using var req = UnityWebRequestTexture.GetTexture(location);

        var source = new TaskCompletionSource<CachedImage?>();

        req.SendWebRequest().completed += _ => {
            // ReSharper disable once AccessToDisposedClosure
            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = SpriteUtils.CreateSprite(tex);

            var cached = sprite != null ? new CachedImage(sprite) : null;

            source.SetResult(cached);
        };

        return await source.Task;
    }

    private static async Task<CachedImage?> LoadAnyRemote(string location, CancellationToken token) {
        using var stream = await client.GetStreamAsync(location);

        return await LoadImageFromStream(stream, null, token);
    }

    #endregion

    #region Assembly

    private static bool TryGetAssembly(string location, out Assembly? assembly, out string? path) {
        var parameters = location.Split(':');

        switch (parameters.Length) {
            case 1:
                path = parameters[0];
                assembly = Assembly.Load(path.Substring(0, path.IndexOf('.')));
                return true;
            case 2:
                path = parameters[1];
                assembly = Assembly.Load(parameters[0]);
                return true;
            default:
                assembly = null;
                path = null;
                return false;
        }
    }

    #endregion

    #region Stream

    private static async Task<CachedImage?> LoadImageFromStream(Stream stream, byte[]? bytes, CancellationToken token) {
        // Try to load as GIF first
        if (await TryLoadGifImage(stream, token) is { } gif) {
            return new CachedImage(gif);
        }

        // Reset stream position for fallback
        stream.Position = 0;

        try {
            // Load bytes or use a preloaded array
            bytes ??= await ReadStreamToBufferAsync(stream, token);

            // Load as static image (e.g. PNG, JPG)
            var sprite = SpriteUtils.CreateSprite(bytes);

            return new CachedImage(sprite!);
        } catch (Exception ex) {
            Debug.LogWarning($"Failed to create a static image: {ex.Message}");
            return null;
        }
    }

    private static Task<GIFImage?> TryLoadGifImage(Stream stream, CancellationToken token) {
        return Task.Run(
            () => {
                try {
                    // Important to leave open as it's just a wrapper
                    var reader = new BinaryReader(stream);

                    // Returns null if magic is invalid
                    return new GIFLoader().Load(reader);
                } catch (Exception ex) {
                    Debug.LogError($"Failed to create a GIF: {ex}");

                    return null;
                }
            },
            token
        );
    }

    private static async Task<byte[]> ReadStreamToBufferAsync(Stream stream, CancellationToken cancellationToken = default) {
        var contentSize = (int)stream.Length;
        var buffer = new byte[contentSize];
        var totalRead = 0;

        while (totalRead < contentSize) {
            var read = await stream.ReadAsync(buffer, totalRead, contentSize - totalRead, cancellationToken);
            if (read == 0) {
                throw new EndOfStreamException("Unexpected end of stream before expected content size.");
            }

            totalRead += read;
        }

        return buffer;
    }

    #endregion
}