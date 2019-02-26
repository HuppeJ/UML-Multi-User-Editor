﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace PolyPaint.CustomInk
{
    class CustomInkCanvas : InkCanvas
    {
        private CustomDynamicRenderer customRenderer = new CustomDynamicRenderer();

        private StrokeCollection clipboard;

        #region StrokeType dependency property
        public string StrokeType
        {
            get { return (string)GetValue(StrokeTypeProperty); }
            set { SetValue(StrokeTypeProperty, value); }
        }
        public static readonly DependencyProperty StrokeTypeProperty = DependencyProperty.Register(
          "StrokeType", typeof(string), typeof(CustomInkCanvas), new PropertyMetadata("class"));
        #endregion

        #region SelectedStrokes dependency property
        public StrokeCollection SelectedStrokes
        {
            get { return (StrokeCollection) GetValue(SelectedStrokesProperty); }
            set { SetValue(SelectedStrokesProperty, value); }
        }
        public static readonly DependencyProperty SelectedStrokesProperty = DependencyProperty.Register(
          "SelectedStrokes", typeof(StrokeCollection), typeof(CustomInkCanvas), new PropertyMetadata(new StrokeCollection()));
        #endregion

        public CustomInkCanvas() : base()
        {
            // Use the custom dynamic renderer on the custom InkCanvas.
            DynamicRenderer = customRenderer;

            clipboard = new StrokeCollection();
        }

        protected override void OnSelectionChanged(EventArgs e) {
            SelectedStrokes = this.GetSelectedStrokes();
        }

        #region OnStrokeCollected
        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Remove the original stroke and add a custom stroke.
            Strokes.Remove(e.Stroke);

            Stroke customStroke;

            switch (StrokeType)
            {
                case "artifact":
                    customStroke = new ArtifactStroke(e.Stroke.StylusPoints);
                    break;
                case "activity":
                    customStroke = new ActivityStroke(e.Stroke.StylusPoints);
                    break;
                case "actor":
                    customStroke = new ActorStroke(e.Stroke.StylusPoints);
                    break;
                case "class":
                    customStroke = new ClassStroke(e.Stroke.StylusPoints);
                    break;
                default:
                    customStroke = new ClassStroke(e.Stroke.StylusPoints);
                    break;
               
            }
            Strokes.Add(customStroke);

            // Visual visual = this.GetVisualChild(this.Children.Count - 1);

            //AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(visual);
            //myAdornerLayer.Add(new AnchorPointAdorner(visual));

            // Pass the custom stroke to base class' OnStrokeCollected method.
            InkCanvasStrokeCollectedEventArgs args = new InkCanvasStrokeCollectedEventArgs(customStroke);
            base.OnStrokeCollected(args);
        }
        #endregion

        #region PasteStrokes
        public void PasteStrokes()
        {
            StrokeCollection strokes = GetSelectedStrokes();

            if (strokes.Count == 0)
            {
                // strokes from clipboard will be pasted
                strokes = clipboard;
            }

            foreach (Stroke stroke in strokes)
            {
                Stroke newStroke = stroke.Clone();

                // TODO : 2 options. 1- Avoir un compteur de Paste qui incremente a chaque Paste, le reinitialiser quand 
                // nouveau OnSelectionChanged. 2- Coller au coin du canvas
                // Voir quoi faire avec le client leger
                Matrix translateMatrix = new Matrix();
                translateMatrix.Translate(20.0, 20.0);
                newStroke.Transform(translateMatrix, false);

                Strokes.Add(newStroke);
            }
        }
        #endregion

        #region CutStrokes
        public void CutStrokes()
        {
            StrokeCollection selection = GetSelectedStrokes();
            // put selection in clipboard to be able to paste it
            clipboard = selection;

            // cut selection from canvas
            CutSelection();
        }
        #endregion
    }
}
