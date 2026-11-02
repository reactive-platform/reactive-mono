using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Reactive.Yoga {
    internal static class YogaLibrary {
        // BSIPA puts Libs/Native/arm64 ahead of Libs/Native on PATH, and Wine on x64 stops at the ARM64 copy instead of skipping it.
        [ModuleInitializer]
        internal static void Preload() {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            var location = typeof(YogaLibrary).Assembly.Location;
            if (string.IsNullOrEmpty(location)) return;

            var nativeDir = Path.Combine(Path.GetDirectoryName(location)!, "Native");
            var path = RuntimeInformation.ProcessArchitecture switch {
                Architecture.X64 => Path.Combine(nativeDir, "yoga.dll"),
                Architecture.Arm64 => Path.Combine(nativeDir, "arm64", "yoga.dll"),
                _ => null
            };
            if (path != null && File.Exists(path)) {
                LoadLibraryW(path);
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW(string path);
    }
}
