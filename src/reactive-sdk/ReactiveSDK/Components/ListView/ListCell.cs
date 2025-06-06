using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public class ListCell<TItem> : ReactiveComponent, IListCell<TItem> {
    public delegate IReactiveComponent Constructor(INotifyValueChanged<TItem> item);

    #region Factory

    public ListCell(Constructor constructor) : this() {
        this.constructor = constructor;
    }

    public ListCell() : base(false) { }

    internal Constructor? constructor;

    protected override GameObject Construct() {
        if (constructor == null) {
            return base.Construct();
        }

        var component = constructor(_observableItem!);
        return component.Use(null);
    }

    #endregion

    #region Cell

    public TItem Item => _observableItem!;
    public INotifyValueChanged<TItem> ObservableItem => _observableItem!;

    private ObservableValue<TItem>? _observableItem;

    void IListCell<TItem>.Init(TItem item) {
        if (!IsInitialized) {
            _observableItem = new(item);
            ConstructAndInit();
        } else {
            _observableItem!.Value = item;
        }

        OnInit(item);
    }

    protected virtual void OnInit(TItem item) { }

    #endregion
}