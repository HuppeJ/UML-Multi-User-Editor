﻿using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System;
using PolyPaint.Enums;
using PolyPaint.Templates;
using System.Globalization;

namespace PolyPaint.CustomInk
{
    public class ArtifactStroke : ShapeStroke
    {
        private Point topLeft;
        private Point topRightUp;
        private Point topRightDown;
        private Point topRightInside;
        private Point bottomRight;
        private Point bottomLeft;
        private Point line1Left;
        private Point line1Right;
        private Point line2Left;
        private Point line2Right;
        private Point line3Left;
        private Point line3Right;
        private Point line4Left;
        private Point line4Right;
        private Point line5Left;
        private Point line5Right;

        public ArtifactStroke(StylusPointCollection pts) : base(pts)
        {
            name = "Artifact";
            shapeStyle.width = 80;
            shapeStyle.height = 80;
            shapeStyle.coordinates = new Coordinates(pts[pts.Count - 1].ToPoint());

            UpdateShapePoints();

            strokeType = (int)StrokeTypes.ARTIFACT;
        }

        public ArtifactStroke(BasicShape basicShape, StylusPointCollection pts) : base(pts, basicShape)
        {

        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            base.DrawCore(drawingContext, drawingAttributes);
            UpdateShapePoints();
            
            LineSegment top = new LineSegment(topRightUp, true);
            LineSegment diag = new LineSegment(topRightDown, true);
            LineSegment right = new LineSegment(bottomRight, true);
            LineSegment bottom = new LineSegment(bottomLeft, true);
            PathSegmentCollection segments = new PathSegmentCollection { top, diag, right, bottom };
            PathFigure figure = new PathFigure();
            figure.Segments = segments;
            figure.IsClosed = true;
            figure.StartPoint = topLeft;
            PathFigureCollection figures = new PathFigureCollection { figure };
            PathGeometry geometry = new PathGeometry();
            geometry.Figures = figures;

            drawingContext.DrawGeometry(fillColor, pen, geometry);

            drawingContext.DrawLine(pen, topRightUp, topRightInside);
            drawingContext.DrawLine(pen, topRightInside, topRightDown);

            drawingContext.DrawLine(pen, line1Left, line1Right);
            drawingContext.DrawLine(pen, line2Left, line2Right);
            drawingContext.DrawLine(pen, line3Left, line3Right);
            drawingContext.DrawLine(pen, line4Left, line4Right);
            drawingContext.DrawLine(pen, line5Left, line5Right);

            FormattedText formattedText = new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Arial"), 12, Brushes.Black);

            formattedText.MaxTextWidth = shapeStyle.width;
            formattedText.TextAlignment = TextAlignment.Center;
            formattedText.MaxTextHeight = 100;

            drawingContext.DrawText(formattedText, GetCustomBound().BottomLeft);
        }

        public override Rect GetBounds()
        {
            return Rect.Empty;
        }

        public override Rect GetCustomBound()
        {
            double width = shapeStyle.width;
            double height = shapeStyle.height;

            Rect rect = new Rect(shapeStyle.coordinates.x, shapeStyle.coordinates.y,
                width, height);

            return rect;
        }

        internal override bool HitTestPoint(Point point)
        {
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            return GetCustomBound().Contains(rotationTransform.Inverse.Transform(point));
        }

        internal override bool HitTestPointIncludingEdition(Point point)
        {
            Rect bounds = GetEditingBounds();
            return bounds.Contains(point);
        }

        public override Rect GetEditingBounds()
        {
            Rect bounds = GetCustomBound();
            RotateTransform rotationTransform = new RotateTransform(shapeStyle.rotation, GetCenter().X, GetCenter().Y);
            Point topLeft = rotationTransform.Transform(bounds.TopLeft);
            Point topRight = rotationTransform.Transform(bounds.TopRight);
            Point bottomLeft = rotationTransform.Transform(bounds.BottomLeft);
            Point bottomRight = rotationTransform.Transform(bounds.BottomRight);
            double minX = Math.Min(Math.Min(Math.Min(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
            double maxX = Math.Max(Math.Max(Math.Max(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
            double minY = Math.Min(Math.Min(Math.Min(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);
            double maxY = Math.Max(Math.Max(Math.Max(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);

            bounds = new Rect(new Point(minX - 15, minY - 15), new Point(maxX + 15, maxY + 15));
            return bounds;
        }

        private void UpdateShapePoints()
        {
            double width = shapeStyle.width;
            double height = shapeStyle.height;

            topLeft = shapeStyle.coordinates.ToPoint();

            topRightUp = new Point(topLeft.X + width * 0.75, topLeft.Y);

            topRightDown = new Point(topLeft.X + width, topLeft.Y + height * 0.2);

            topRightInside = new Point(topLeft.X + width * 0.75, topLeft.Y + height * 0.2);

            bottomLeft = new Point(topLeft.X, topLeft.Y + height);

            bottomRight = new Point(topLeft.X + width, topLeft.Y + height);

            line1Left = new Point(topLeft.X + width * 0.2, topLeft.Y + height * 0.2);
            line1Right = new Point(topLeft.X + width * 0.55, topLeft.Y + height * 0.2);

            line2Left = new Point(topLeft.X + width * 0.2, topLeft.Y + height * 0.35);
            line2Right = new Point(topLeft.X + width * 0.8, topLeft.Y + height * 0.35);

            line3Left = new Point(topLeft.X + width * 0.2, topLeft.Y + height * 0.50);
            line3Right = new Point(topLeft.X + width * 0.8, topLeft.Y + height * 0.50);

            line4Left = new Point(topLeft.X + width * 0.2, topLeft.Y + height * 0.65);
            line4Right = new Point(topLeft.X + width * 0.8, topLeft.Y + height * 0.65);

            line5Left = new Point(topLeft.X + width * 0.2, topLeft.Y + height * 0.8);
            line5Right = new Point(topLeft.X + width * 0.8, topLeft.Y + height * 0.8);
        }

        public override Point GetCenter()
        {
            Rect rect = GetCustomBound();
            return new Point(rect.X + shapeStyle.width / 2, rect.Y + shapeStyle.height / 2);
        }
    }
}