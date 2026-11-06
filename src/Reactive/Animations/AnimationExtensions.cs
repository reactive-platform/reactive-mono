using JetBrains.Annotations;

namespace Reactive;

// ReSharper disable InconsistentNaming
[PublicAPI]
public static class AnimationExtensions {
    extension(float t) {
        public AnimationDuration sec => new(t);
        public AnimationDuration ms => new(t / 1000f);
        public AnimationTiming fact => AnimationTiming.Fact(t);
    }
    
    extension(int t) {
        public AnimationDuration sec => new(t);
        public AnimationDuration ms => new(t / 1000f);
        public AnimationTiming fact => AnimationTiming.Fact(t);
    }
}