using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class ListViewCell<LItem> : ReactiveComponent, IListViewCell<LItem> {
        #region Factory

        private IReactiveComponent? _constructedComponent;

        public delegate IReactiveComponent Constructor(INotifyValueChanged<LItem> item);

        public ListViewCell(Constructor constructor) : base(false) {
            _constructedComponent = constructor(_observableItem!);
            ConstructAndInit();
        }

        public ListViewCell() { }

        protected override GameObject Construct() {
            return _constructedComponent?.Use(null) ?? base.Construct();
        }

        #endregion

        #region TableCell

        private ObservableValue<LItem?> _observableItem = new(default);

        void IListViewCell<LItem>.Init(LItem item) {
            _observableItem.Value = item;
            OnInit(item);
        }

        #endregion

        #region Abstraction

        public LItem Item => _observableItem!;

        public INotifyValueChanged<LItem> ObservableItem => _observableItem!;

        protected virtual void OnInit(LItem item) { }

        #endregion
    }
}