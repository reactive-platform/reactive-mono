using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reactive.Compiler;

/// <summary>
/// Reads the build configuration a project exposes with <c>&lt;CompilerVisibleProperty Include="Configuration" /&gt;</c>.
/// </summary>
internal static class BuildConfiguration {
    private const string ConfigurationKey = "build_property.Configuration";

    /// <summary>
    /// Provides whether the compilation is a debug build. False when the project doesn't expose the configuration.
    /// </summary>
    public static IncrementalValueProvider<bool> CreateIsDebugProvider(IncrementalGeneratorInitializationContext context) {
        return context.AnalyzerConfigOptionsProvider.Select(static (options, _) => IsDebug(options.GlobalOptions));
    }

    /// <summary>
    /// Checks whether the options describe a debug build. False when the configuration isn't there.
    /// </summary>
    public static bool IsDebug(AnalyzerConfigOptions options) {
        return options.TryGetValue(ConfigurationKey, out var configuration) &&
            string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase);
    }
}
