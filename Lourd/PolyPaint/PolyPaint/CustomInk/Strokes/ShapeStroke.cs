﻿using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using System.Globalization;
using PolyPaint.Templates;
using System.Collections.Generic;

namespace PolyPaint.CustomInk
{
    public abstract class ShapeStroke : CustomStroke
    {
        public ShapeStyle shapeStyle;
        public List<string> linksTo;
        public List<string> linksFrom;
        public List<Point> anchorPoints;

        public ShapeStroke(StylusPointCollection pts) : base(pts)
        {
            guid = Guid.NewGuid();
            name = "This is a stroke";

            Point lastPoint = pts[pts.Count - 1].ToPoint();
            Coordinates coordinates = new Coordinates(lastPoint.X, lastPoint.Y);

            shapeStyle = new ShapeStyle(coordinates,100,100,0, "#FFFFFFFF", 0,"none");

            while (StylusPoints.Count > 1)
            {
                StylusPoints.RemoveAt(0);
            }
            
            linksTo = new List<string>();
            linksFrom = new List<string>();
        }

        
        public ShapeStroke(StylusPointCollection pts, BasicShape basicShape) : base(pts)
        {
            guid = Guid.Parse(basicShape.id);
            name = basicShape.name;
            type = basicShape.type;
            shapeStyle = basicShape.shapeStyle;

            Point point = new Point(shapeStyle.coordinates.x, shapeStyle.coordinates.y);

            for (double i = point.X; i < shapeStyle.width + point.X; i += 0.5)
            {
                for (double j = point.Y; j < shapeStyle.height + point.Y; j += 0.5)
                {
                    StylusPoints.Add(new StylusPoint(i, j));
                }
            }
        }

        public Point GetAnchorPoint(int anchorNumber)
        {
            double xCenter = GetCenter().X;
            double yCenter = GetCenter().Y;
            double margin = 10;
            double halfWidth = GetBounds().Width / 2 + margin;
            double halfHeight = GetBounds().Height / 2 + margin;

            Point pointRotatedAroundOrigin;

            switch (anchorNumber)
            {
                case 0:
                    pointRotatedAroundOrigin = rotatePoint(0, halfHeight);
                    break;
                case 1:
                    pointRotatedAroundOrigin = rotatePoint(-halfWidth, 0);
                    break;
                case 2:
                    pointRotatedAroundOrigin = rotatePoint(0, -halfHeight);
                    break;
                default:
                    pointRotatedAroundOrigin = rotatePoint(halfWidth, 0);
                    break;
            }

            return new Point(xCenter - pointRotatedAroundOrigin.X, yCenter - pointRotatedAroundOrigin.Y);
        }
        
        public virtual BasicShape GetBasicShape()
        {
            BasicShape basicShape = new BasicShape(guid.ToString(), type, name, shapeStyle, linksTo, linksFrom);
            return basicShape;
        }

        public override void updatePosition()
        {
            Coordinates newCoordinates = new Coordinates(GetBounds().X, GetBounds().Y);
            shapeStyle.coordinates = newCoordinates;
        }
    }
}