namespace Reactive.Components {
    public interface IKeyedControlCellBase<in TKey, in TParam> {
        void Init(TKey key, TParam param);
    }
}