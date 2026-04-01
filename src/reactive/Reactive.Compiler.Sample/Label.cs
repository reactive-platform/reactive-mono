using UnityEngine;

namespace Reactive.Compiler.Sample;

public partial class Label : ReactiveComponent {
    [Required]
    public string Text { get; set; }
    
    [Required]
    public Color Color { get; set; }
}