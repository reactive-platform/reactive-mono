using HarmonyLib;
using HMUI;
using JetBrains.Annotations;

namespace Reactive.BeatSaber;

[HarmonyPatch(typeof(ViewController), "__Activate")]
internal static class CompositionPatch {
    [UsedImplicitly]
    private static void Prefix(ViewController __instance) {
        __instance.gameObject.GetOrAddComponent<Composition>();
    }
}
