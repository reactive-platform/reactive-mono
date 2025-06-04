using System;

namespace Reactive.Components {
    public interface IListViewCell<in LItem> {
        void Init(LItem item);
    }
}