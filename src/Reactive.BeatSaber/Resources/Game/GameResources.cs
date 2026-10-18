using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#nullable disable

namespace Reactive.BeatSaber;

[PublicAPI]
[CreateAssetMenu(fileName = "BeatSaberSDK_GameResources", menuName = "Reactive/BeatSaberSDK/GameResources")]
public class GameResources : ScriptableObject {
    #region Initialization

    static GameResources() {
        Init();
    }

    private static GameResources _instance;

#if COMPILE_EDITOR
    private static void Init() {
        if (_instance != null) return;
        var bundles = AssetDatabase.FindAssets("BeatSaberSDK_GameResources");
        var guid = bundles.First();
        var path = AssetDatabase.GUIDToAssetPath(guid);
        _instance = AssetDatabase.LoadAssetAtPath<GameResources>(path);
    }
#else
    internal static void Init() {
        if (_instance != null) return;
        _instance = CreateInstance<GameResources>();
        
        _instance.buttonClickSignal = Find<Signal>("UIButtonWasPressed");
        
        _instance.closeIcon = Find<Sprite>("CloseIcon");
        _instance.searchIcon = Find<Sprite>("SearchIcon");
        _instance.arrowOutlineIcon = Find<Sprite>("ArrowOutline");
        _instance.arrowUpIcon = Find<Sprite>("ArrowUp");
        _instance.arrowIcon = Find<Sprite>("ArrowIcon");
        _instance.editIcon = Find<Sprite>("EditIcon");
        _instance.caretIcon = Find<Sprite>("Caret");
        _instance.verticalIndicatorIcon = Find<Sprite>("VerticalRoundRect8");
        _instance.circleIcon = Find<Sprite>("FullCircle64");
        _instance.hueWheelIcon = Find<Sprite>("HueWheel");
        _instance.hueWheelStrokeIcon = Find<Sprite>("HueWheelStroke");
        _instance.circleSmallIcon = Find<Sprite>("CircleSmall");
        _instance.circleTinyIcon = Find<Sprite>("CircleTiny");
        _instance.roundRectTinyIcon = Find<Sprite>("RoundRectTiny");
        _instance.rectOutlineIcon = Find<Sprite>("RectOutline");
        _instance.roundRect10Icon = Find<Sprite>("RoundRect10");
        _instance.roundRect10BorderIcon = Find<Sprite>("RoundRect10Border");
        _instance.roundRect10OutlineIcon = Find<Sprite>("RoundRect10Outline");
        
        _instance.uiNoGlowMaterial = Find<Material>("UINoGlow");
        _instance.uiFontMaterial = Find<Material>(
            "Teko-Medium SDF Curved Softer",
            x => x.mainTexture.name == "Teko-Medium SDF Atlas"
        );
        _instance.uiFogBackgroundMaterial = Find<Material>("UIFogBG");
        _instance.uiSVColorPickerMaterial = Find<Material>("UISVColorPicker");
        
        _instance.animatedButtonMaterial = Find<Material>("AnimatedButton");
        _instance.animatedButtonBorderMaterial = Find<Material>("AnimatedButtonBorder");
    }

    private static T Find<T>(string name, Func<T, bool> func = null) where T : Object {
        return Resources.FindObjectsOfTypeAll<T>().First(x => x.name == name && (func?.Invoke(x) ?? true));
    }
#endif

    #endregion

    #region Serialized
    
    public Signal buttonClickSignal;

    public Material uiNoGlowMaterial;
    public Material uiFontMaterial;
    public Material uiFogBackgroundMaterial;

    public Sprite closeIcon;
    public Sprite searchIcon;
    public Sprite arrowOutlineIcon;
    public Sprite arrowUpIcon;
    public Sprite arrowIcon;
    public Sprite editIcon;
    public Sprite caretIcon;
    public Sprite circleIcon;
    public Sprite verticalIndicatorIcon;
    public Sprite hueWheelIcon;
    public Sprite hueWheelStrokeIcon;
    public Sprite circleSmallIcon;
    public Sprite circleTinyIcon;
    public Sprite roundRectTinyIcon;
    public Sprite rectOutlineIcon;
    public Sprite roundRect10Icon;
    public Sprite roundRect10BorderIcon;
    public Sprite roundRect10OutlineIcon;
    public Material uiSVColorPickerMaterial;

    internal Material animatedButtonMaterial;
    internal Material animatedButtonBorderMaterial;

    #endregion

    #region Internal

    internal static Material AnimatedButtonMaterial => _instance.animatedButtonMaterial;
    
    internal static Material AnimatedButtonBorderMaterial => _instance.animatedButtonBorderMaterial;

    internal static Material UISVColorPickerMaterial => _instance.uiSVColorPickerMaterial;
    
    #endregion
    
    #region Static

    public static Signal ButtonClickSignal => _instance.buttonClickSignal;
    
    public static Material UIFogBackgroundMaterial => _instance.uiFogBackgroundMaterial;

    public static Material UINoGlowMaterial => _instance.uiNoGlowMaterial;

    public static Material UIFontMaterial => _instance.uiFontMaterial;

    public static Sprite ArrowIcon => _instance.arrowIcon;
    
    public static Sprite ArrowOutlineIcon => _instance.arrowOutlineIcon;
    
    public static Sprite ArrowUpIcon => _instance.arrowUpIcon;
    
    public static Sprite SearchIcon => _instance.searchIcon;
    
    public static Sprite CloseIcon => _instance.closeIcon;
    
    public static Sprite EditIcon => _instance.editIcon;
    
    public static Sprite CaretIcon => _instance.caretIcon;
    
    public static Sprite CircleIcon => _instance.circleIcon;

    public static Sprite VerticalIndicatorIcon => _instance.verticalIndicatorIcon;

    public static Sprite HueWheelIcon => _instance.hueWheelIcon;

    public static Sprite HueWheelStrokeIcon => _instance.hueWheelStrokeIcon;
    
    public static Sprite CircleSmallIcon => _instance.circleSmallIcon;

    public static Sprite CircleTinyIcon => _instance.circleTinyIcon;

    public static Sprite RoundRectTinyIcon => _instance.roundRectTinyIcon;

    public static Sprite RectOutlineIcon => _instance.rectOutlineIcon;

    public static Sprite RoundRect10Icon => _instance.roundRect10Icon;

    public static Sprite RoundRect10BorderIcon => _instance.roundRect10BorderIcon;

    public static Sprite RoundRect10OutlineIcon => _instance.roundRect10OutlineIcon;
    
    #endregion
}