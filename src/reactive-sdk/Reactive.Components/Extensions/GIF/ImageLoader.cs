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
    public static async Task<CachedImage?> LoadImage(string location, CancellationToken token) {
        if (images.TryGetValue(location, out var image)) {
            return image;
        }

        var stream = await GetDataAsync(location, token);

        image = await LoadImage(stream, null, token);
        
        if (image != null) {
            images[location] = image;
        }

        return image;
    }

    internal static async Task<Stream> GetDataAsync(string location, CancellationToken token) {
        if (location.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || location.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
            var response = await client.GetAsync(location, token);
            return await response.Content.ReadAsStreamAsync();
        } else if (File.Exists(location)) {
            using (FileStream fileStream = File.OpenRead(location))
            using (MemoryStream memoryStream = new(new byte[fileStream.Length], true))
            {
                await fileStream.CopyToAsync(memoryStream);
                return memoryStream;
            }
        } else {
            AssemblyFromPath(location, out Assembly asm, out string newPath);
            return await GetResourceAsync(asm, newPath);
        }
    }

    internal static void AssemblyFromPath(string inputPath, out Assembly assembly, out string path) {
        string[] parameters = inputPath.Split(':');
        switch (parameters.Length) {
            case 1:
                path = parameters[0];
                assembly = Assembly.Load(path.Substring(0, path.IndexOf('.')));
                break;
            case 2:
                path = parameters[1];
                assembly = Assembly.Load(parameters[0]);
                break;
            default:
                throw new Exception($"Could not process resource path {inputPath}");
        }
    }

    internal static async Task<Stream> GetResourceAsync(Assembly asm, string resourceName) {
        using Stream resourceStream = asm.GetManifestResourceStream(resourceName);
        using MemoryStream memoryStream = new(new byte[resourceStream.Length], true);

        await resourceStream.CopyToAsync(memoryStream);

        return memoryStream;
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
            var sprite = SpriteUtils.CreateSprite(bytes);

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
                    // Important to leave open as it's just a wrapper
                    var reader = new BinaryReader(stream);
                    
                    // Returns null if magic is invalid
                    return new GIFLoader().Load(reader);
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