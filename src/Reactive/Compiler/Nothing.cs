using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Reactive.Compiler;

/// <summary>
/// A type that represents nothing. Used as a dummy to segregate
/// dummy overloads from their parameterless analogs.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[EditorBrowsable(EditorBrowsableState.Never)]
public struct Nothing;