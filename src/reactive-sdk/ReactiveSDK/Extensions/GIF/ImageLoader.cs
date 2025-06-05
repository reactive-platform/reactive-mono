using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B83.Image.GIF;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class ImageLoader {
    public static IDictionary<string, CachedImage> CachedImages => images;

    private static readonly Dictionary<string, CachedImage> images = new();
    private static readonly HttpClient client = new();

    /// <summary>
    /// Loads an image from a remote url.
    /// </summary>
    /// <param name="url">A url to fetch the data from.</param>
    /// <returns>A loaded image or null.</returns>
    public static async Task<CachedImage?> LoadRemoteImage(string url, CancellationToken token) {
        if (images.TryGetValue(url, out var image)) {
            return image;
        }

        var response = await client.GetAsync(url, token);
        var stream = await response.Content.ReadAsStreamAsync();

        image = await LoadImage(stream, null, token);
        
        if (image != null) {
            images[url] = image;
        }

        return image;
    }

    /// <summary>
    /// Loads an image using a byte array.
    /// </summary>
    /// <param name="data">An array to load from.</param>
    /// <returns>A loaded image or null.</returns>
    public static Task<CachedImage?> LoadImage(byte[] data, CancellationToken token) {
        using var stream = new MemoryStream(data);

        return LoadImage(stream, data, token);
    }

    private static async Task<CachedImage?> LoadImage(Stream stream, byte[]? bytes, CancellationToken token) {
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
            var sprite = ReactiveUtils.CreateSprite(bytes);

            return new CachedImage(sprite!);
        } catch (Exception ex) {
            Debug.LogWarning($"Failed to create static image: {ex.Message}");
            return null;
        }
    }

    private static Task<GIFImage?> TryLoadGifImage(Stream stream, CancellationToken token) {
        return Task.Run(
            () => {
                try {
                    // Returns null if magic is invalid
                    return new GIFLoader().Load(stream);
                } catch (Exception ex) {
                    Debug.LogError($"Failed to load GIF: {ex}");

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
}