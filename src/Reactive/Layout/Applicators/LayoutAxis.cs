using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// The values of <see cref="LayoutDimensions"/> an applicator can handle separately.
/// </summary>
[PublicAPI]
[Flags]
public enum LayoutAxis {
    PosX = 1 << 0,
    PosY = 1 << 1,
    
    SizeX = 1 << 2,
    SizeY = 1 << 3,

    Pos = PosX | PosY,
    Size = SizeX | SizeY,

    X = PosX | SizeX,
    Y = PosY | SizeY,

    All = Pos | Size
}
