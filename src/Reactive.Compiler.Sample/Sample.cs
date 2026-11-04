using Reactive.Yoga;
using UnityEngine;

namespace Reactive.Compiler.Sample;

public class Sample : ReactiveComponent {
    protected override GameObject Construct() {
        var text = Remember("");
        var color = RememberAnimated(Color.blue, 200.ms);

        text.AddCallback(Debug.Log);
        color.AddCallback(static x => Debug.Log(x));

        return new Layout {
            FlexController = {
                FlexDirection = FlexDirection.Column,
                ConstrainHorizontal = false
            },

            FlexItem = {
                Size = new() { y = 20.pt },
            },

            Children = {
                new Label {
                    FlexItem = {
                        Size = new() { x = 10.pt }
                    },

                    sText = RememberDerived(
                        text, color,
                        (t, c) => $"The label is {c} and displays {t}"
                    ),

                    sColor = color,
                }
            }
        }.Use();
    }
}

public class Sample2 : ReactiveComponent {
    private State<string> _text = null!;

    // Try changing to state_} and {}: it won't compile
    [StateGen(Patterns = ["state_{}", "st{}", "s_{}"])]
    protected override GameObject Construct() {
        _text = Remember("");

        return new Label {
            state_Text = _text.Map(x => $"Hello, {x}!"),
            stColor = _text.Map(_ => Color.blue),
        }.Use();
    }
}