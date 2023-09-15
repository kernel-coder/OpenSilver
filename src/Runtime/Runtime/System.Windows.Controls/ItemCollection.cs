
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using OpenSilver.Internal;
using OpenSilver.Internal.Controls;

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    public sealed class ItemCollection : PresentationFrameworkCollection<object>, INotifyCollectionChanged
    {
        private bool _isUsingItemsSource;
        private IEnumerable _itemsSource; // base collection
        private WeakEventListener<ItemCollection, INotifyCollectionChanged, NotifyCollectionChangedEventArgs> _collectionChangedListener;

        private bool _isUsingListWrapper;
        private EnumerableWrapper _listWrapper;

        private IInternalFrameworkElement _modelParent;

        internal ItemCollection(IInternalFrameworkElement parent) : base(true)
        {
            this._modelParent = parent;
        }

        private Func<object, bool> _filterFunction;
        internal Func<object, bool> FilterFunction
        {
            get => _filterFunction;
            set
            {
                _filterFunction = value;

                if (_filterFunction == null)
                {
                    _filteredItemsWrapper = null;
                }
                else
                {
                    if (_filteredItemsWrapper == null)
                    {
                        _filteredItemsWrapper = new FilteredEnumerableWrapper(this);
                        _filteredItemsWrapper.SetSource(_isUsingItemsSource ? _itemsSource : this);
                    }
                    else if (_filteredItemsWrapper != null)
                    {
                        _filteredItemsWrapper.Refresh();
                    }
                }
            }
        }


        private FilteredEnumerableWrapper _filteredItemsWrapper;
        internal IList FilteredItems => _filteredItemsWrapper; 

        internal override bool IsFixedSizeImpl
        {
            get { return this.IsUsingItemsSource; }
        }

        internal override bool IsReadOnlyImpl
        {
            get { return this.IsUsingItemsSource; }
        }

        internal override void AddOverride(object value)
        {
            if (this.IsUsingItemsSource)
            {
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
            }

            this.SetModelParent(value);
            this.AddInternal(value);            
        }

        internal override void ClearOverride()
        {
            if (this.IsUsingItemsSource)
            {
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
            }            

            foreach (var item in this)
            {
                this.ClearModelParent(item);
            }

            this.ClearInternal();
        }

        internal override void InsertOverride(int index, object value)
        {
            if (this.IsUsingItemsSource)
            {
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
            }
            
            this.SetModelParent(value);
            this.InsertInternal(index, value);
        }

        internal override void RemoveAtOverride(int index)
        {
            if (this.IsUsingItemsSource)
            {
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
            }
            
            object removedItem = this.GetItemInternal(index);
            this.ClearModelParent(removedItem);
            this.RemoveAtInternal(index);
        }

        internal override object GetItemOverride(int index)
        {
            if (this.IsUsingItemsSource)
            {
                return this.SourceList[index];
            }

            return this.GetItemInternal(index);
        }

        internal override void SetItemOverride(int index, object value)
        {
            if (this.IsUsingItemsSource)
            {
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
            }

            object originalItem = this.GetItemInternal(index);
            this.ClearModelParent(originalItem);
            this.SetModelParent(value);
            this.SetItemInternal(index, value);
        }

        internal override bool ContainsImpl(object value)
        {
            if (this.IsUsingItemsSource)
            {
                return this.SourceList.Contains(value);
            }

            return base.ContainsImpl(value);
        }

        internal override int IndexOfImpl(object value)
        {
            if (this.IsUsingItemsSource)
            {
                return this.SourceList.IndexOf(value);
            }

            return base.IndexOfImpl(value);
        }

        internal override IEnumerator<object> GetEnumeratorImpl()
        {
            if (this.IsUsingItemsSource)
            {
                return this.GetEnumeratorPrivateItemsSourceOnly();
            }

            return base.GetEnumeratorImpl();
        }

        private IEnumerator<object> GetEnumeratorPrivateItemsSourceOnly()
        {
            IEnumerator enumerator = this.SourceList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                base.CollectionChanged += value;
            }
            remove
            {
                base.CollectionChanged -= value;
            }
        }

        internal IEnumerator LogicalChildren
        {
            get
            {
                if (this.IsUsingItemsSource)
                {
                    return EmptyEnumerator.Instance;
                }

                return this.GetEnumerator();
            }
        }

        internal bool IsUsingItemsSource
        {
            get
            {
                return this._isUsingItemsSource;
            }
        }

        internal IEnumerable SourceCollection
        {
            get
            {
                return this._itemsSource;
            }
        }

        internal IList SourceList
        {
            get
            {
                if (this._isUsingListWrapper)
                {
                    return this._listWrapper;
                }

                return (IList)this._itemsSource;
            }
        }

        internal override int CountInternal
        {
            get
            {
                if (this.IsUsingItemsSource)
                {
                    return this.SourceList.Count;
                }
                else
                {
                    return base.CountInternal;
                }
            }
        }

        internal void SetItemsSource(IEnumerable value)
        {
            if (!this.IsUsingItemsSource && this.CountInternal != 0)
            {
                throw new InvalidOperationException("Items collection must be empty before using ItemsSource.");
            }

            if (value == null)
            {
                throw new InvalidOperationException("ItemsSource must not be null");
            }

            this.TryUnsubscribeFromCollectionChangedEvent();

            this._itemsSource = value;
            this._isUsingItemsSource = true;

            if (_filterFunction != null && _filteredItemsWrapper == null)
            {
                _filteredItemsWrapper = new FilteredEnumerableWrapper(this);                
            }
           
            if (_filteredItemsWrapper != null)
            {
                _filteredItemsWrapper.SetSource(_itemsSource);
            }

            this.TrySubscribeToCollectionChangedEvent(value);

            this.InitializeSourceList(value);

            this.UpdateCountProperty();

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void ClearItemsSource()
        {
            if (_filteredItemsWrapper != null)
            {
                _filteredItemsWrapper = null;
            }

            if (this.IsUsingItemsSource)
            {
                // return to normal mode
                this.TryUnsubscribeFromCollectionChangedEvent();

                this._itemsSource = null;
                this._listWrapper = null;
                this._isUsingItemsSource = false;
                this._isUsingListWrapper = false;

                this.UpdateCountProperty();

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void InitializeSourceList(IEnumerable sourceCollection)
        {
            IList sourceAsList = sourceCollection as IList;
            if (sourceAsList == null)
            {
                this._listWrapper = new EnumerableWrapper();
                this._listWrapper.SetSource(sourceCollection);
                this._isUsingListWrapper = true;
            }
            else
            {
                this._listWrapper = null;
                this._isUsingListWrapper = false;
            }
        }

        internal void RefreshFilteredItems()
        {
            if (_filteredItemsWrapper != null)
            {
                _filteredItemsWrapper?.Refresh();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void ValidateCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count != 1)
                    {
                        throw new NotSupportedException("Range actions are not supported.");
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 1)
                    {
                        throw new NotSupportedException("Range actions are not supported.");
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
                    {
                        throw new NotSupportedException("Range actions are not supported.");
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new NotSupportedException(string.Format("Unexpected collection change action '{0}'.", e.Action));
            }
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ValidateCollectionChangedEventArgs(e);

            // Update list wrapper
            if (this._isUsingListWrapper)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        this._listWrapper.InsertOverride(e.NewStartingIndex, e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        this._listWrapper.RemoveAtOverride(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        this._listWrapper.MoveOverride(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        this._listWrapper.ReplaceAtOverride(e.OldStartingIndex, e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        this._listWrapper.Refresh();
                        break;
                }
            }

            this.UpdateCountProperty();

            // Raise collection changed
            this.OnCollectionChanged(e);
        }

        protected internal override void __OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_filteredItemsWrapper != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        this._filteredItemsWrapper.InsertOverride(e.NewStartingIndex, e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        this._filteredItemsWrapper.RemoveAtOverride(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        this._filteredItemsWrapper.MoveOverride(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        this._filteredItemsWrapper.ReplaceAtOverride(e.OldStartingIndex, e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        this._filteredItemsWrapper.Refresh();
                        break;
                }
            }

            base.__OnCollectionChanged(e);
        }

        private void SetModelParent(object item)
        {
            if (this._modelParent != null)
            {
                this._modelParent.AddLogicalChild(item);
            }
        }

        private void ClearModelParent(object item)
        {
            if (this._modelParent != null)
            {
                this._modelParent.RemoveLogicalChild(item);
            }
        }

        private void TrySubscribeToCollectionChangedEvent(IEnumerable collection)
        {
            if (collection is INotifyCollectionChanged incc)
            {
                _collectionChangedListener = new(this, incc)
                {
                    OnEventAction = static (instance, source, args) => instance.OnSourceCollectionChanged(source, args),
                    OnDetachAction = static (listener, source) => source.CollectionChanged -= listener.OnEvent,
                };
                incc.CollectionChanged += _collectionChangedListener.OnEvent;
            }
        }

        private void TryUnsubscribeFromCollectionChangedEvent()
        {
            if (_collectionChangedListener != null)
            {
                _collectionChangedListener.Detach();
                _collectionChangedListener = null;
            }
        }

        private class EnumerableWrapper : List<object>
        {
            protected IEnumerable _sourceCollection;

            public void SetSource(IEnumerable source)
            {
                Debug.Assert(source != null);
                _sourceCollection = source;
                Refresh();
            }

            public void Refresh()
            {
                this.Clear();

                IEnumerator enumerator = this._sourceCollection?.GetEnumerator();
                while (enumerator?.MoveNext() == true)
                {
                    this.AddOverride(enumerator.Current);
                }
            }
            
            public virtual void AddOverride(object item)
            {
                this.Add(item);
            }

            public virtual void InsertOverride(int index, object item)
            {
                Insert(index, item);              
            }

            public virtual void RemoveAtOverride(int index)
            {
                RemoveAt(index);
            }

            public virtual void ReplaceAtOverride(int index, object item)
            {
                this[index] = item;
            }

            public virtual void MoveOverride(int oldIndex, int newIndex)
            {
                if (oldIndex == newIndex)
                {
                    return;
                }

                var item = this[oldIndex];

                this.RemoveAt(oldIndex);
                this.Insert(newIndex, item);
            }
        }

        private class FilteredEnumerableWrapper : EnumerableWrapper
        {
            private ItemCollection _owner;

            public FilteredEnumerableWrapper(ItemCollection owner)
            {
                Debug.Assert(owner != null);
                this._owner = owner;
            }

            private int IdxSource2Me(int idx)
            {
                return this.Count + idx - _sourceCollection.Count();
            }

            private object ItemInSourceAt(int index)
            {
                int idx = 0;
                foreach (object item in _sourceCollection)
                {
                    if (idx == index)
                    {
                        return item;
                    }

                    idx++;
                }

                throw new ArgumentOutOfRangeException("at", "expected value less then " + idx);
            }

            public override void AddOverride(object item)
            {
                if (_owner.FilterFunction != null && _owner.FilterFunction(item))
                {
                    this.Add(item);
                }
            }

            public override void InsertOverride(int index, object item)
            {
                if (_owner.FilterFunction != null && _owner.FilterFunction(item))
                {
                    Insert(IdxSource2Me(index), item);
                }
            }

            public override void RemoveAtOverride(int index)
            {
                if (this.Contains(ItemInSourceAt(index)))
                {
                    RemoveAt(IdxSource2Me(index));
                }
            }

            public override void ReplaceAtOverride(int index, object item)
            {
                this[IdxSource2Me(index)] = item;
            }

            public override void MoveOverride(int oldIndex, int newIndex)
            {
                if (oldIndex == newIndex)
                {
                    return;
                }

                var myOldIndex = IdxSource2Me(oldIndex);
                var item = this[myOldIndex];

                this.RemoveAt(myOldIndex);
                this.Insert(IdxSource2Me(newIndex), item);
            }
        }
    }
}