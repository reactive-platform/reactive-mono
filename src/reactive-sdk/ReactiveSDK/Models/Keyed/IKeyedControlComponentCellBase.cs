namespace Reactive.Components {
    public interface IKeyedControlComponentCellBase<in TKey, in TParam> {
        void Init(TKey key, TParam param);
    }
}