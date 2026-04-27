using UnityEngine;

namespace Reactive.Compiler.Sample;

public partial class LabelBase : ReactiveComponent {
    [Required]
    public string Text { get; set; }
}

public partial class Label : LabelBase {
    public Color Color { get; set; }
}