namespace Reactive.Components {
    public struct GraphicState {
        public GraphicState(bool hovered, bool pressed, bool active, bool interactable) {
            Hovered = hovered;
            Pressed = pressed;
            Active = active;
            Interactable = interactable;
        }

        public bool Hovered;
        public bool Pressed;
        public bool Active;
        public bool Interactable;
    }
}