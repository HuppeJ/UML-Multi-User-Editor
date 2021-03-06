﻿using PolyPaint.Modeles;
using PolyPaint.Utilitaires;
using PolyPaint.VueModeles;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PolyPaint.Vues
{
    /// <summary>
    /// Interaction logic for JoinRoomPopup.xaml
    /// </summary>
    public partial class HistoryPopup
    {
        private VueModele dataContext = null;
        private WindowDrawing drawingview = null;

        public HistoryPopup()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(HistoryPopup_Loaded);
        }

        void HistoryPopup_Loaded(object sender, RoutedEventArgs e)
        {
            dataContext = DataContext as VueModele;
        }

        public void Initialize()
        {
            var parent = Parent;
            while (!(parent is WindowDrawing))
            {
                parent = LogicalTreeHelper.GetParent(parent);
            }

            drawingview = (WindowDrawing)parent;

            var scrollViewer = GetDescendantByType(historyList, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.ScrollToBottom();
        }

        // From https://stackoverflow.com/questions/10293236/accessing-the-scrollviewer-of-a-listbox-from-c-sharp
        private static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }

        private void ShowPreview(object sender, RoutedEventArgs e)
        {
            string thumbnail = ((Button)sender).Tag as string;
            dataContext.thumbnail = thumbnail;
            popUpHistoryPreviewVue.Initialize();
            popUpHistoryPreview.IsOpen = true;
            IsEnabled = false;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            ClosePopup();
            drawingview.ClosePopup();
        }

        internal void ClosePopup()
        {
            popUpHistoryPreview.IsOpen = false;
            IsEnabled = true;
        }
    }
}
