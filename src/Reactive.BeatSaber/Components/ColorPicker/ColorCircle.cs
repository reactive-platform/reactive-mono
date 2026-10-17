using System;
using HMUI;
using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    /// <summary>
    /// A color selector component.
    /// </summary>
    [PublicAPI]
    public class ColorCircle : ReactiveComponent {
        #region Public API

        /// <summary>
        /// Represents a color related to the current handle position. Updates each time the handle is moved.
        /// </summary>
        public Color Color {
            get;
            private set {
                field = value;
                OnColorUpdated?.Invoke(value);
            }
        } = Color.white;

        /// <summary>
        /// Represents a color after releasing the handle. Updates only once you release the handle.
        /// </summary>
        public Color SavedColor {
            get;
            private set {
                field = value;
                OnColorSaved?.Invoke(value);
            }
        } = Color.white;

        public Action<Color>? OnColorSaved { get; set; }
        public Action<Color>? OnColorUpdated { get; set; }

        public void SetColor(Color color, bool notifyListeners = true) {
            Color.RGBToHSV(color, out var hue, out var saturation, out var value);

            // Hue is undefined for grayscale colors, so keeping the current one to avoid the handle jumping
            if (saturation == 0f || value == 0f) {
                hue = _hsv.Value.x;
            }

            _hsv.Value = new(hue, saturation, value);

            if (notifyListeners) {
                Color = CurrentColor;
                SavedColor = CurrentColor;
            }
        }

        #endregion

        #region Color Helpers

        private Color CurrentColor => HsvToColor(_hsv.Value);

        private static Color HsvToColor(Vector3 hsv) {
            return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        }

        #endregion

        #region Construct

        private State<Vector3> _hsv = null!;
        private RectTransform _wheelRing = null!;
        private RectTransform _squareCursor = null!;

        protected override GameObject Construct() {
            _hsv = Remember(new Vector3(0f, 0f, 1f));

            var hueColor = _hsv.Map(static x => Color.HSVToRGB(x.x, 1f, 1f));
            var color = _hsv.Map(HsvToColor);

            return new Layout {
                FlexItem = {
                    Size = 54.pt,
                    AspectRatio = 1f
                },

                FlexController = {
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Center
                },

                Children = {
                    new Background {
                        Name = "HueWheel",

                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = 0.pct
                        },

                        FlexController = {
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Center
                        },

                        Sprite = BeatSaberResources.Sprites.transparentPixel,
                        Material = null,

                        Do = x => x.WithPointerEvents(
                            onDown: y => HandlePointerDown(y, true),
                            onUpdate: y => HandlePointerMoved(y, true),
                            onUp: _ => HandlePointerUp()
                        ),

                        Children = {
                            CreateFill(GameResources.HueWheelIcon),
                            CreateFill(GameResources.HueWheelStrokeIcon),

                            new Layout {
                                Name = "HueRing",

                                FlexItem = {
                                    PositionType = PositionType.Absolute,
                                    Position = 0.pct
                                },

                                FlexController = {
                                    FlexDirection = FlexDirection.Row,
                                    JustifyContent = Justify.FlexEnd,
                                    AlignItems = Align.Center
                                },

                                Children = {
                                    CreateCursor(GameResources.CircleSmallIcon, hueColor)
                                        .AsFlexItem(size: new() { x = 11.pct }, aspectRatio: 1f)
                                }
                            }.Bind(ref _wheelRing)
                        }
                    },

                    new Background {
                        Name = "SaturationValue",

                        FlexItem = {
                            Size = 48.pct
                        },

                        FlexController = {
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Center
                        },

                        Sprite = BeatSaberResources.Sprites.transparentPixel,
                        Material = null,

                        Do = x => x.WithPointerEvents(
                            onDown: handler => HandlePointerDown(handler, false),
                            onUpdate: handler => HandlePointerMoved(handler, false),
                            onUp: _ => HandlePointerUp()
                        ),

                        Children = {
                            // The material draws saturation and value, while the color defines the hue
                            new Image {
                                FlexItem = {
                                    PositionType = PositionType.Absolute,
                                    Position = 0.pct
                                },

                                Sprite = GameResources.RoundRectTinyIcon,
                                ImageType = UnityEngine.UI.Image.Type.Sliced,
                                Material = GameResources.UISVColorPickerMaterial,
                                RaycastTarget = false,
                                sColor = hueColor
                            },

                            new Image {
                                FlexItem = {
                                    PositionType = PositionType.Absolute,
                                    Position = 0.pct
                                },

                                Sprite = GameResources.RectOutlineIcon,
                                ImageType = UnityEngine.UI.Image.Type.Sliced,
                                Material = GameResources.UISVColorPickerMaterial,
                                RaycastTarget = false,
                                sColor = hueColor
                            },

                            CreateCursor(GameResources.CircleTinyIcon, color).Bind(ref _squareCursor)
                        }
                    }
                }
            }.Use();
        }

        protected override void OnInitialize() {
            _hsv.AddCallback(this, static (x, _) => x.PlaceCursors(), lazy: true);
            PlaceCursors();
        }

        #endregion

        #region Construct Helpers

        private static Image CreateFill(Sprite sprite) {
            return new() {
                FlexItem = {
                    PositionType = PositionType.Absolute,
                    Position = 0.pct
                },

                Sprite = sprite,
                PreserveAspect = true,
                RaycastTarget = false
            };
        }

        private static Image CreateCursor(Sprite sprite, IState<Color> background) {
            const float lightColorThreshold = 0.7f;

            return new() {
                ContentTransform = {
                    sizeDelta = Vector2.zero
                },

                Sprite = sprite,
                PreserveAspect = true,
                RaycastTarget = false,

                sColor = background.Map(
                    static x => x.grayscale > lightColorThreshold ?
                        new(0f, 0f, 0f, 0.75f) : Color.white
                )
            };
        }

        #endregion

        #region Cursors

        private CurvedCanvasSettings? _curvedCanvasSettings;

        private void PlaceCursors() {
            const float squareCursorSize = 0.1f;

            var hsv = _hsv.Value;
            var point = new Vector2(hsv.y, hsv.z);
            var halfSize = Vector2.one * (squareCursorSize / 2f);

            _squareCursor.anchorMin = point - halfSize;
            _squareCursor.anchorMax = point + halfSize;
            _wheelRing.localEulerAngles = new(0f, 0f, hsv.x * 360f);
        }

        private void HandlePointerDown(PointerEventsHandler handler, bool wheel) {
            _curvedCanvasSettings = Content.GetComponentInParent<CurvedCanvasSettings>();
            HandlePointerMoved(handler, wheel);
        }

        private void HandlePointerMoved(PointerEventsHandler handler, bool wheel) {
            if (!handler.IsPressed || handler.EventData is not { } eventData || eventData.position == Vector2.zero || Canvas is not { } canvas) {
                return;
            }

            var transform = (RectTransform)handler.transform;
            var rect = transform.rect;
            var point = eventData.TranslateToLocalPoint(transform, canvas, _curvedCanvasSettings);
            var hsv = _hsv.Value;

            if (wheel) {
                var offset = point - rect.center;
                var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

                hsv.x = (angle < 0f ? angle + 360f : angle) / 360f;
            } else {
                hsv.y = Mathf.Clamp01((point.x - rect.xMin) / rect.width);
                hsv.z = Mathf.Clamp01((point.y - rect.yMin) / rect.height);
            }

            _hsv.Value = hsv;
            Color = CurrentColor;
        }

        private void HandlePointerUp() {
            _curvedCanvasSettings = null;
            SavedColor = CurrentColor;
        }

        #endregion
    }
}