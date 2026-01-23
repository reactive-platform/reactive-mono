using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public class ListCell<TItem> : ReactiveComponent, IListCell<TItem> {
    public delegate IReactiveComponent Constructor(IState<TItem> item);

    #region Factory

    public ListCell(Constructor constructor) : this() {
        this.constructor = constructor;
    }

    public ListCell() : base(false) { }

    internal Constructor? constructor;
    private IReactiveComponent? _constructedComponent;

    protected override GameObject Construct() {
        if (constructor == null) {
            return base.Construct();
        }

        _constructedComponent = constructor(_observableItem!);
        return _constructedComponent.Use(null);
    }

    protected override void OnInitialize() {
        if (_constructedComponent is ReactiveComponent comp) {
            ExposeLayoutFirstComponent(comp);
        }
    }

    #endregion

    #region Cell

    public TItem Item => _observableItem!;
    public IState<TItem> ObservableItem => _observableItem!;

    private State<TItem>? _observableItem;

    void IListCell<TItem>.Init(TItem item) {
        if (!IsInitialized) {
            _observableItem = new(item);
            ConstructAndInit();
        }
        _observableItem!.Value = item;

        OnInit(item);
    }

    protected virtual void OnInit(TItem item) { }

    #endregion
}