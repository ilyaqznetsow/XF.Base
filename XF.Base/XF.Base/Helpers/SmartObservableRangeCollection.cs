using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace XF.Base.Helpers
{
    public class SmartObservableRangeCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public void AddWithNotify(T item)
        {
            var index = Count;
            Add(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { item }, index));
        }

        public void InsertWithNotify(int index, T item)
        {
            Insert(index, item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { item }, index));
        }

        public void RemoveWithNotify(T item)
        {
            var index = IndexOf(item);
            if (index < 0) return;
            Remove(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }, index);
            OnCollectionChanged(args);
        }

        public void RemoveAtWithNotify(int at)
        {
            if (Count - 1 < at) return;
            var item = this[at];
            Remove(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }, at);
            OnCollectionChanged(args);
        }


        public void ClearWithNotify()
        {
            Clear();
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
        /// </summary>
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            var index = Count;
            foreach (var i in collection) Add(i);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList(), index));
        }

        /// <summary>
        /// Adds the elements of the specified collection to the index of the ObservableCollection(Of T).
        /// </summary>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var newItems = collection as T[] ?? collection.ToArray();
            if (newItems == null) throw new ArgumentNullException(nameof(collection));
            if (newItems.Length == 0) return;
            var newIndex = index;
            foreach (var i in newItems)
            {
                Insert(newIndex++, i);
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
        }

        /// <summary>
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T).
        /// </summary>
        public void RemoveRange(IEnumerable<T> collection)
        {
            var oldItems = collection as T[] ?? collection.ToArray();
            if (oldItems == null) throw new ArgumentNullException(nameof(collection));
            if (oldItems.Length == 0) return;
            var start = Items.IndexOf(oldItems.First());
            foreach (var i in oldItems) Items.Remove(i);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, start);
            OnCollectionChanged(args);
        }

        /// <summary>
        /// Clears the current collection and replaces it with the specified item.
        /// </summary>
        public void Replace(T item, T newItem)
        {
            var index = Items.IndexOf(item);
            Items.Remove(item);
            Items.Insert(index, newItem);


            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            var items = new List<T> { item };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, items, items, index);
            OnCollectionChanged(args);
            /* args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index);
             OnCollectionChanged(args);
             args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index);
             OnCollectionChanged(args);*/
        }

        /// <summary>
        /// Clears the current collection and replaces it with the specified collection.
        /// </summary>
        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class.
        /// </summary>
        public SmartObservableRangeCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">collection: The collection from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception>
        public SmartObservableRangeCollection(IList<T> collection) : base(collection)
        {
        }

        public SmartObservableRangeCollection(IEnumerable<T> collection) : base(collection.ToList())
        {
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs p0)
        {
            PropertyChanged?.Invoke(this, p0);
        }

        void OnCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
