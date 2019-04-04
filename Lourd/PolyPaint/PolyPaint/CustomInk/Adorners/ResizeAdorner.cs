﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using PolyPaint.CustomInk.Strokes;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System;
using System.Windows.Ink;
using PolyPaint.Services;
using PolyPaint.Vues;
using PolyPaint.Templates;

namespace PolyPaint.CustomInk
{
    class ResizeAdorner : Adorner
    {
        List<Thumb> anchors;
        List<StrokeResizePointThumb> cheatAnchors;
        Thumb moveThumb;

        VisualCollection visualChildren;

        // The bounds of the Strokes;
        Rect strokeBounds = Rect.Empty;
        public CustomStroke customStroke;

        public CustomInkCanvas canvas;

        RotateTransform rotationPreview;
        RotateTransform rotation;

        private Path resizePreview;
        private Path outerBoundPath;

        Vector unitX = new Vector(1, 0);
        Vector unitY = new Vector(0, 1);

        RectangleGeometry NewRectangle = new RectangleGeometry();
        RectangleGeometry OldRectangle = new RectangleGeometry();

        public ResizeAdorner(UIElement adornedElement, CustomStroke customStroke, CustomInkCanvas actualCanvas)
            : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);

            if (customStroke is ShapeStroke)
                strokeBounds = customStroke.GetCustomBound();
            else
                strokeBounds = (customStroke as LinkStroke).GetStraightBounds();

            resizePreview = new Path();
            resizePreview.Stroke = Brushes.Black;
            resizePreview.StrokeDashArray = new DoubleCollection { 5, 2 };
            resizePreview.StrokeThickness = 1;
            visualChildren.Add(resizePreview);

            this.customStroke = customStroke;
            canvas = actualCanvas;

            Point center = customStroke.GetCenter();
            if(customStroke is ShapeStroke)
                rotation = new RotateTransform((customStroke as ShapeStroke).shapeStyle.rotation, center.X, center.Y);
            else
                rotation = new RotateTransform(0, center.X, center.Y);
            while (rotation.Angle < 0)
            {
                rotation.Angle += 360;
            }

            outerBoundPath = new Path();
            outerBoundPath.Stroke = Brushes.Black;
            outerBoundPath.StrokeDashArray = new DoubleCollection { 5, 2 }; 
            outerBoundPath.StrokeThickness = 1;
            Rect rect = customStroke.GetCustomBound();
            rect.X -= 5;
            rect.Y -= 5;
            rect.Width += 10;
            rect.Height += 10;
            outerBoundPath.Data = new RectangleGeometry(rect, 0, 0, rotation);
            visualChildren.Add(outerBoundPath);

            moveThumb = new Thumb();
            moveThumb.Cursor = Cursors.SizeAll;
            moveThumb.Height = strokeBounds.Height + 10;
            moveThumb.Width = strokeBounds.Width + 10;
            moveThumb.Background = Brushes.Transparent;
            moveThumb.DragDelta += new DragDeltaEventHandler(Move_DragDelta);
            moveThumb.DragCompleted += new DragCompletedEventHandler(Move_DragCompleted);
            moveThumb.DragStarted += new DragStartedEventHandler(All_DragStarted);
            moveThumb.PreviewMouseUp += new MouseButtonEventHandler(LeftMouseUp);
            moveThumb.RenderTransform = new RotateTransform(rotation.Angle, moveThumb.Width / 2, moveThumb.Height / 2); ;

            visualChildren.Add(moveThumb);

            unitX = rotation.Value.Transform(unitX);
            unitY = rotation.Value.Transform(unitY);
            // RenderTransform = rotation;

            anchors = new List<Thumb>();
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());
            anchors.Add(new Thumb());

            int index = 0;
            foreach (Thumb anchor in anchors)
            {
                anchor.Width = 10;
                anchor.Height = 10;
                anchor.Background = new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#FFBBBBBB"), 
                    (Color)ColorConverter.ConvertFromString("#FF646464"), 45);
                anchor.BorderBrush = Brushes.Black;
                if (rotation.Angle % 360 >= 360 - 45 / 2 || rotation.Angle % 360 <= 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        default:
                            break;
                    }
                } else if (rotation.Angle % 360 > 45 / 2 && rotation.Angle % 360 <= 90 - 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        default:
                            break;
                    }
                } else if (rotation.Angle % 360 > 90 - 45 / 2 && rotation.Angle % 360 <= 90 + 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 90 + 45 / 2 && rotation.Angle % 360 <= 135 + 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 180 - 45 / 2 && rotation.Angle % 360 <= 180 + 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 225 - 45 / 2 && rotation.Angle % 360 <= 225 + 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 270 - 45 / 2 && rotation.Angle % 360 <= 270 + 45 / 2)
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            anchor.DragDelta += new DragDeltaEventHandler(Top_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 1:
                            anchor.DragDelta += new DragDeltaEventHandler(Right_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 2:
                            anchor.DragDelta += new DragDeltaEventHandler(Bottom_DragDelta);
                            anchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 3:
                            anchor.DragDelta += new DragDeltaEventHandler(Left_DragDelta);
                            anchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 4:
                            anchor.DragDelta += new DragDeltaEventHandler(TopLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        case 5:
                            anchor.DragDelta += new DragDeltaEventHandler(TopRight_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 6:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomLeft_DragDelta);
                            anchor.Cursor = Cursors.SizeNS;
                            break;
                        case 7:
                            anchor.DragDelta += new DragDeltaEventHandler(BottomRight_DragDelta);
                            anchor.Cursor = Cursors.SizeWE;
                            break;
                        default:
                            break;
                    }
                }
                anchor.DragStarted += new DragStartedEventHandler(All_DragStarted);
                anchor.DragCompleted += new DragCompletedEventHandler(All_DragCompleted);
                if(!(customStroke is LinkStroke) || !(customStroke as LinkStroke).isAttached())
                    visualChildren.Add(anchor);
                index++;
            }

            cheatAnchors = new List<StrokeResizePointThumb>();
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 0));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 1));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 2));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 3));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 4));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 5));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 6));
            cheatAnchors.Add(new StrokeResizePointThumb(customStroke, canvas, 7));

            index = 0;
            foreach (Thumb cheatAnchor in cheatAnchors)
            {
                cheatAnchor.Width = 1;
                cheatAnchor.Height = 1;
                if (rotation.Angle % 360 >= 315 || rotation.Angle % 360 <= 45)
                {
                    switch (index)
                    {
                        case 0:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 1:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 2:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 3:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 4:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 5:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 6:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 7:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 45 && rotation.Angle % 360 <= 135)
                {
                    switch (index)
                    {
                        case 0:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 1:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 2:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 3:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 4:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 5:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 6:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 7:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        default:
                            break;
                    }
                }
                else if (rotation.Angle % 360 > 135 && rotation.Angle % 360 <= 225)
                {
                    switch (index)
                    {
                        case 0:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 1:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 2:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 3:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 4:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 5:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 6:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 7:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 1:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 2:
                            cheatAnchor.Cursor = Cursors.SizeNWSE;
                            break;
                        case 3:
                            cheatAnchor.Cursor = Cursors.SizeNESW;
                            break;
                        case 4:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 5:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        case 6:
                            cheatAnchor.Cursor = Cursors.SizeNS;
                            break;
                        case 7:
                            cheatAnchor.Cursor = Cursors.SizeWE;
                            break;
                        default:
                            break;
                    }
                }

                canvas.Children.Add(cheatAnchor);
                index++;
            }

            rotationPreview = rotation.Clone();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (strokeBounds.IsEmpty)
            {
                return finalSize;
            }
            // Top
            ArrangeAnchor(0, 0, -(strokeBounds.Height / 2));
            // Right
            ArrangeAnchor(1, strokeBounds.Width / 2, 0);
            // Bottom
            ArrangeAnchor(2, 0, strokeBounds.Height / 2);
            // Left
            ArrangeAnchor(3, -(strokeBounds.Width / 2), 0);
            // TopLeft
            ArrangeAnchor(4, -(strokeBounds.Width / 2), -(strokeBounds.Height / 2));
            // TopRight
            ArrangeAnchor(5, strokeBounds.Width / 2, -(strokeBounds.Height / 2));
            // BottomLeft
            ArrangeAnchor(6, -(strokeBounds.Width / 2), strokeBounds.Height / 2);
            // BottomRight
            ArrangeAnchor(7, strokeBounds.Width / 2, strokeBounds.Height / 2);

            resizePreview.Arrange(new Rect(finalSize));
            outerBoundPath.Arrange(new Rect(new Size(canvas.ActualWidth, canvas.ActualHeight)));

            Rect handleRect = new Rect(strokeBounds.X - 5,
                                  strokeBounds.Y - 5,
                                  strokeBounds.Width + 10,
                                  strokeBounds.Height + 10);
            moveThumb.Arrange(handleRect);

            return finalSize;
        }

        private void ArrangeAnchor(int anchorNumber, double xOffset, double yOffset)
        {
            if (xOffset > 0)
            {
                xOffset += 5;
            }
            if (xOffset < 0)
            {
                xOffset -= 5;
            }
            if (yOffset < 0)
            {
                yOffset -= 5;
            }
            if (yOffset > 0)
            {
                yOffset += 5;
            }
            // The rectangle that determines the position of the Thumb.
            Rect handleRect = new Rect(strokeBounds.X + xOffset,
                                  strokeBounds.Y + yOffset,
                                  strokeBounds.Width,
                                  strokeBounds.Height);

            handleRect.Transform(rotation.Value);
            // Draws the thumb and the rectangle around the strokes.
            anchors[anchorNumber].Arrange(handleRect);
            cheatAnchors[anchorNumber].Arrange(handleRect);
        }

        void All_DragStarted(object sender, DragStartedEventArgs e)
        {
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y,
                                      customStroke.GetCustomBound().Width,
                                      customStroke.GetCustomBound().Height);
            OldRectangle = new RectangleGeometry(rectangle, 0, 0, rotation);
        }

        void LeftMouseUp(object sender, MouseEventArgs e)
        {
            if (customStroke is LinkStroke)
            {
                canvas.modifyLinkStrokePath(customStroke as LinkStroke, e.GetPosition(canvas));
            }
        }
        
        void All_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Point actualPos = Mouse.GetPosition(this);
            if (e.HorizontalChange == 0 && e.VerticalChange == 0)
            {
                visualChildren.Remove(resizePreview);
                InvalidateArrange();
                return;
            }
            
            visualChildren.Remove(resizePreview);

            canvas.ResizeShape(customStroke, NewRectangle, OldRectangle);

            canvas.RefreshLinks(false);
            canvas.RefreshChildren();

            InvalidateArrange();

            DrawingService.UpdateShapes(new StrokeCollection { customStroke });
        }

        void Move_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.HorizontalChange == 0 && e.VerticalChange == 0)
            {
                visualChildren.Remove(resizePreview);
                InvalidateArrange();
                return;
            }


            visualChildren.Remove(resizePreview);

            canvas.MoveShape(NewRectangle.Rect.X - OldRectangle.Rect.X, NewRectangle.Rect.Y - OldRectangle.Rect.Y);

            canvas.RefreshLinks(false);
            canvas.RefreshChildren();

            InvalidateArrange();

            DrawingService.UpdateShapes(new StrokeCollection { customStroke });
        }

        #region DragDelta
        private void generatePreview(Rect rectangle)
        {
            NewRectangle = new RectangleGeometry(rectangle, 0, 0, rotationPreview);
            resizePreview.Data = NewRectangle;
            resizePreview.Arrange(new Rect(new Size(canvas.ActualWidth, canvas.ActualHeight)));
        }

        private Vector calculateDelta(DragDeltaEventArgs e)
        {
            Point center = customStroke.GetCenter();
            RotateTransform rotationInverse = new RotateTransform(360 - rotation.Angle, center.X, center.Y);
            Vector dragVect = new Vector(e.HorizontalChange, e.VerticalChange);
            dragVect = rotationInverse.Value.Transform(dragVect);
            
            return dragVect;
        }

        void Move_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector dragVect = new Vector(e.HorizontalChange, e.VerticalChange);
            dragVect = rotation.Value.Transform(dragVect);

            if (dragVect.X != 0 || dragVect.Y != 0)
            {
                rotationPreview.CenterX = customStroke.GetCenter().X + dragVect.X;
                rotationPreview.CenterY = customStroke.GetCenter().Y + dragVect.Y;
                Rect rectangle = new Rect(customStroke.GetCustomBound().X + dragVect.X,
                                          customStroke.GetCustomBound().Y + dragVect.Y,
                                          customStroke.GetCustomBound().Width,
                                          customStroke.GetCustomBound().Height);
                if(customStroke is LinkStroke && (customStroke as LinkStroke).isAttached())
                {
                    LinkStroke linkStroke = customStroke.Clone() as LinkStroke;
                    linkStroke.path = new List<Coordinates>();
                    foreach (Coordinates coord in (customStroke as LinkStroke).path)
                    {
                        linkStroke.path.Add(new Coordinates(coord.ToPoint()));
                    }

                    for (int i = 0; i < linkStroke.path.Count; i++)
                    {
                        if (i == 0 && linkStroke.isAttached() && linkStroke.from?.formId != null)
                        {
                            continue;
                        }
                        if (i == linkStroke.path.Count - 1 && linkStroke.isAttached() && linkStroke.to?.formId != null)
                        {
                            continue;
                        }
                        Coordinates coords = linkStroke.path[i];
                        coords.x += e.HorizontalChange;
                        coords.y += e.VerticalChange;
                    }
                    rectangle = linkStroke.GetStraightBounds();
                }
                generatePreview(rectangle);
            }
        }

        void Top_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.Y > customStroke.GetCustomBound().Height - 21)
            {
                delta.Y = customStroke.GetCustomBound().Height - 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y + delta.Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width),
                                      Math.Max(21, customStroke.GetCustomBound().Height - delta.Y));
            generatePreview(rectangle);
        }

        void Right_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.X < -customStroke.GetCustomBound().Width + 21)
            {
                delta.X = -customStroke.GetCustomBound().Width + 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width + delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height));
            generatePreview(rectangle);
        }

        void Bottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.Y < -customStroke.GetCustomBound().Height + 21)
            {
                delta.Y = -customStroke.GetCustomBound().Height + 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width),
                                      Math.Max(21, customStroke.GetCustomBound().Height + delta.Y));
            generatePreview(rectangle);
        }

        void Left_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.X > customStroke.GetCustomBound().Width - 21)
            {
                delta.X = customStroke.GetCustomBound().Width - 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X + delta.X,
                                      customStroke.GetCustomBound().Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width - delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height));
            generatePreview(rectangle);
        }

        void TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            ShapeStroke shapeStroke = customStroke as ShapeStroke;
            if (delta.Y > customStroke.GetCustomBound().Height - 21)
            {
                delta.Y = customStroke.GetCustomBound().Height - 21;
            }
            if (delta.X > customStroke.GetCustomBound().Width - 21)
            {
                delta.X = customStroke.GetCustomBound().Width - 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X + delta.X,
                                      customStroke.GetCustomBound().Y + delta.Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width - delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height - delta.Y));
            generatePreview(rectangle);
        }

        void TopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.Y > customStroke.GetCustomBound().Height - 21)
            {
                delta.Y = customStroke.GetCustomBound().Height - 21;
            }
            if (delta.X < -customStroke.GetCustomBound().Width + 21)
            {
                delta.X = -customStroke.GetCustomBound().Width + 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y + delta.Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width + delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height - delta.Y));
            generatePreview(rectangle);
        }

        void BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.Y < -customStroke.GetCustomBound().Height + 21)
            {
                delta.Y = -customStroke.GetCustomBound().Height + 21;
            }
            if (delta.X > customStroke.GetCustomBound().Width - 21)
            {
                delta.X = customStroke.GetCustomBound().Width - 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X + delta.X,
                                      customStroke.GetCustomBound().Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width - delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height + delta.Y));
            generatePreview(rectangle);
        }

        void BottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = calculateDelta(e);
            if (delta.Y < -customStroke.GetCustomBound().Height + 21)
            {
                delta.Y = -customStroke.GetCustomBound().Height + 21;
            }
            if (delta.X < -customStroke.GetCustomBound().Width + 21)
            {
                delta.X = -customStroke.GetCustomBound().Width + 21;
            }
            Rect rectangle = new Rect(customStroke.GetCustomBound().X,
                                      customStroke.GetCustomBound().Y,
                                      Math.Max(21, customStroke.GetCustomBound().Width + delta.X),
                                      Math.Max(21, customStroke.GetCustomBound().Height + delta.Y));
            generatePreview(rectangle);
        }
        #endregion

        // Override the VisualChildrenCount and 
        // GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount
        {
            get { return visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }


    }
}