﻿/**
 * Author Meshack Musundi
 * Source: https://www.codeproject.com/Articles/1181555/SignalChat-WPF-SignalR-Chat-Application
 */

using System.Collections.Specialized;
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;

namespace PolyPaint.Utilitaires
{
    public class BringNewItemIntoViewBehavior : Behavior<ItemsControl>
    {
        private INotifyCollectionChanged notifier;

        protected override void OnAttached()
        {
            base.OnAttached();
            notifier = AssociatedObject.Items as INotifyCollectionChanged;
            notifier.CollectionChanged += ItemsControl_CollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            notifier.CollectionChanged -= ItemsControl_CollectionChanged;
        }

        private void ItemsControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newIndex = e.NewStartingIndex;
                var newElement = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(newIndex);
                var item = (FrameworkElement)newElement;
                item?.BringIntoView();
            }
        }
    }
}