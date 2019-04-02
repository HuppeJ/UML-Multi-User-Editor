﻿using PolyPaint.CustomInk.Strokes;
using PolyPaint.Enums;
using PolyPaint.Services;
using PolyPaint.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.Windows.Shapes.Path;
using sd = System.Drawing;
using s2d = System.Drawing.Drawing2D;

namespace PolyPaint.CustomInk
{
    public class CustomInkCanvas : InkCanvas
    {
        private CustomDynamicRenderer customRenderer = new CustomDynamicRenderer();

        public Dictionary<string, CustomStroke> StrokesDictionary = new Dictionary<string, CustomStroke>();

        private StrokeCollection clipboard;
        private Templates.Canvas canvas;

        public StylusPoint firstPoint;
        public bool isUpdatingLink = false;
        
        private StrokeCollection beingSelected = new StrokeCollection();
        private PathFigure selectionPath = new PathFigure();
        StrokeCollection oldSelectedStrokes = new StrokeCollection();
        
        Point oldLeftTopPoint = new Point(0, 0);
        Point newLeftTopPoint = new Point(0, 0);

        #region Dictonary
        public void AddStroke(CustomStroke stroke)
        {
            Strokes.Add(stroke);
            StrokesDictionary[stroke.guid.ToString()] = stroke;
        }

        public void RemoveStroke(CustomStroke stroke)
        {
            StrokesDictionary.Remove(stroke.guid.ToString());
            Strokes.Remove(stroke);
        }

        public void ReplaceStrokes(Stroke selectedStroke, StrokeCollection newStrokes)
        {
            foreach (CustomStroke stroke in newStrokes)
            {
                StrokesDictionary[stroke.guid.ToString()] = stroke;
            }
            Strokes.Replace(selectedStroke, newStrokes);
        }
        #endregion

        #region Links
        public void updateLink(int linkStrokeAnchor, LinkStroke linkBeingUpdated, ShapeStroke strokeToAttach, int strokeToAttachAnchor, Point pointPosition)
        {

            if (isUpdatingLink)
            {
                linkBeingUpdated.path[linkStrokeAnchor] = new Coordinates(pointPosition);

                if (linkStrokeAnchor == 0)
                {
                    RemoveShapeStrokeLinkFrom(linkBeingUpdated);

                    // mettre a jour la position du point initial (from)
                    linkBeingUpdated.from = new AnchorPoint(strokeToAttach?.guid.ToString(), strokeToAttachAnchor, "");
                    strokeToAttach?.linksFrom.Add(linkBeingUpdated.guid.ToString());

                }
                else if (linkStrokeAnchor == linkBeingUpdated.path.Count - 1)
                {
                    RemoveShapeStrokeLinkTo(linkBeingUpdated);

                    // mettre a jour la position du point final (to)
                    linkBeingUpdated.to = new AnchorPoint(strokeToAttach?.guid.ToString(), strokeToAttachAnchor, "");
                    strokeToAttach?.linksTo.Add(linkBeingUpdated.guid.ToString());
                }

                linkBeingUpdated.addStylusPointsToLink();
                RefreshChildren();
            }

        }

        private void RemoveShapeStrokeLinkTo(LinkStroke linkBeingUpdated)
        {
            if (linkBeingUpdated.to?.formId != null)
            {
                CustomStroke shapeStrokeTo;
                if (StrokesDictionary.TryGetValue(linkBeingUpdated.to?.formId, out shapeStrokeTo))
                {
                    (shapeStrokeTo as ShapeStroke).linksTo.Remove(linkBeingUpdated.guid.ToString());
                }
            }
        }

        private void RemoveShapeStrokeLinkFrom(LinkStroke linkBeingUpdated)
        {
            if (linkBeingUpdated.from?.formId != null)
            {
                CustomStroke shapeStrokeFrom;
                if (StrokesDictionary.TryGetValue(linkBeingUpdated.from?.formId, out shapeStrokeFrom))
                {
                    (shapeStrokeFrom as ShapeStroke).linksFrom.Remove(linkBeingUpdated.guid.ToString());
                }
            }
        }

        public void RefreshLinks(bool isStrokesMoved)
        {
            StrokeCollection selectedStrokes = GetSelectedStrokes();
            foreach (CustomStroke customStroke in Strokes)
            {
                if (customStroke.isLinkStroke())
                {
                    LinkStroke linkStroke = customStroke as LinkStroke;

                    if (linkStroke.isAttached())
                    {
                        if (selectedStrokes.Count == 1 && selectedStrokes.Contains(linkStroke) && !isStrokesMoved)
                        {
                            // keep the same stylus points if linkstroke is attached and is the only one moved
                            linkStroke.addStylusPointsToLink();
                        }
                        else
                        {
                            // si plusieurs points dans le path, les mettre a jour si la selectedStroke a bouge
                            if (selectedStrokes.Contains(linkStroke))
                            {
                                List<Coordinates> pathCopy = new List<Coordinates>(linkStroke.path);

                                StylusPoint point = linkStroke.StylusPoints[0];
                                double xDiff = point.X - linkStroke.path[0].x;
                                double yDiff = point.Y - linkStroke.path[0].y;

                                for (int i = 1; i < linkStroke.path.Count - 1; i++)
                                {
                                    linkStroke.path[i] = new Coordinates(linkStroke.path[i].x + xDiff, linkStroke.path[i].y + yDiff);
                                }
                            }
                            // update the free points of linkStrokes (from view)
                            if (linkStroke.from?.formId == null)
                            {
                                StylusPoint point = linkStroke.StylusPoints[0];
                                linkStroke.path[0] = new Coordinates(point.ToPoint());
                            }
                            else if (linkStroke.to?.formId == null)
                            {
                                StylusPoint point = linkStroke.StylusPoints[linkStroke.StylusPoints.Count - 1];
                                linkStroke.path[linkStroke.path.Count - 1] = new Coordinates(point.ToPoint());
                            }

                            // move the attached points of linkStrokes
                            foreach (CustomStroke selectedStroke in selectedStrokes)
                            {
                                if (linkStroke.from?.formId == selectedStroke.guid.ToString())
                                {
                                    Point fromPoint = linkStroke.GetFromPoint(this.Strokes);
                                    // mettre a jour les positions des points initial et final
                                    linkStroke.path[0] = new Coordinates(fromPoint);
                                }
                                if (linkStroke.to?.formId == selectedStroke.guid.ToString())
                                {
                                    Point toPoint = linkStroke.GetToPoint(this.Strokes);
                                    // mettre a jour les positions des points initial et final
                                    linkStroke.path[linkStroke.path.Count - 1] = new Coordinates(toPoint);
                                }

                            }

                            // update the linkstroke (view)
                            linkStroke.addStylusPointsToLink();
                        }
                    }
                    else if (selectedStrokes.Contains(linkStroke)) // update path if linkStroke is not attached and has been moved or resized
                    {
                        if (isStrokesMoved)
                        {
                            StylusPoint firstPoint = linkStroke.StylusPoints[0];
                            double xDiff = firstPoint.X - linkStroke.path[0].x;
                            double yDiff = firstPoint.Y - linkStroke.path[0].y;

                            linkStroke.path[0] = new Coordinates(firstPoint.ToPoint());

                            StylusPoint lastPoint = linkStroke.StylusPoints[linkStroke.StylusPoints.Count - 1];
                            linkStroke.path[linkStroke.path.Count - 1] = new Coordinates(lastPoint.ToPoint());

                            // si plusieurs points dans le path. gi ne fonctionne pas pour le resize :/
                            for (int i = 1; i < linkStroke.path.Count - 1; i++)
                            {
                               linkStroke.path[i] = new Coordinates(linkStroke.path[i].x + xDiff, linkStroke.path[i].y + yDiff);
                            }
                        } else //isResized, cannot be rotate, does not happen if many SelectedStrokes
                        {
                            /*for (int i = 0; i < linkStroke.path.Count; i++)
                            {
                                linkStroke.path[i] = new Coordinates((linkStroke.path[i].x - oldLeftTopPoint.X) * widthRatio + newLeftTopPoint.X, (linkStroke.path[i].y - oldLeftTopPoint.Y) * heightRatio + newLeftTopPoint.Y);
                            }

                            linkStroke.addStylusPointsToLink();*/
                        }

                    }
                }
            }
        }
        
        internal void modifyLinkStrokePath(LinkStroke linkStroke, Point mousePosition)
        {
            if (linkStroke.ContainsPoint(mousePosition))
            {
                int index = linkStroke.GetIndexforNewPoint(mousePosition);
                Path path = new Path();
                path.Data = linkStroke.GetGeometry();

                Children.Add(path);
                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(path);
                myAdornerLayer.Add(new LinkElbowAdorner(mousePosition, index, path, linkStroke, this));
            }


        }
        #endregion

        #region StrokeType dependency property
        public string StrokeType
        {
            get { return (string)GetValue(StrokeTypeProperty); }
            set { SetValue(StrokeTypeProperty, value); }
        }
        public static readonly DependencyProperty StrokeTypeProperty = DependencyProperty.Register(
          "StrokeType", typeof(string), typeof(CustomInkCanvas), new PropertyMetadata("CLASS_SHAPE"));
        #endregion
        #region SelectedStrokes dependency property
        public StrokeCollection SelectedStrokes
        {
            get { return (StrokeCollection)GetValue(SelectedStrokesProperty); }
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

            /*
            canvas = new Templates.Canvas(Guid.NewGuid().ToString(), "qwe", ConnectionService.username,
                ConnectionService.username, 0, null, new List<BasicShape>(), new List<Link>(), new int[] { 1, 1 });
            DrawingService.CreateCanvas(canvas);
            DrawingService.JoinCanvas("qwe");
            */

            DrawingService.AddStroke += OnRemoteStroke;
            DrawingService.RemoveStrokes += OnRemoveStrokes;
            DrawingService.UpdateStroke += OnUpdateStroke;
            DrawingService.UpdateSelection += OnRemoteSelection;
            DrawingService.UpdateDeselection += OnRemoteDeselection;
            DrawingService.OnResizeCanvas += OnResizeCanvas;
            DrawingService.SaveCanvas += ConvertInkCanvasToByteArray;
        }

        private void OnResizeCanvas(Coordinates dimensions)
        {
            Width = dimensions.x;
            Height = dimensions.y;
        }

        #region On.. event handlers
        // we don't want Ctrl + C and Ctrl + V
        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            if ((e.Key == Key.C || e.Key == Key.V) && (Keyboard.Modifiers & ModifierKeys.Control) ==
                ModifierKeys.Control)
            {
                //do nothing
                e.Handled = true;
            }
        }

        protected override void OnSelectionChanging(InkCanvasSelectionChangingEventArgs e) {
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            StrokeCollection strokesToSelect = new StrokeCollection();
            StrokeCollection strokesToRemove = new StrokeCollection();
            List<string> selectedIds = new List<string>();
            foreach (CustomStroke newStroke in GetSelectedStrokes())
            {
                selectedIds.Add(newStroke.guid.ToString());
                if(!DrawingService.localSelectedStrokes.Contains(newStroke.guid.ToString())) {
                    strokesToSelect.Add(newStroke);
                }
            }
            foreach (string id in DrawingService.localSelectedStrokes)
            {
                if (!selectedIds.Contains(id))
                {
                    strokesToRemove.Add(StrokesDictionary[id]);
                }
            }
            SelectedStrokes = GetSelectedStrokes();
            if (strokesToSelect.Count > 0)
            {
                DrawingService.SelectShapes(strokesToSelect);
            }
            if (strokesToRemove.Count > 0)
            {
                DrawingService.DeselectShapes(strokesToRemove);
            }
            if (GetSelectedStrokes().Count == 1 && 
                       GetSelectedStrokes()[0] is LinkStroke && 
                       !(GetSelectedStrokes()[0] as LinkStroke).isAttached()){
                ResizeEnabled = true;
            }
            else
            {
                ResizeEnabled = false;
            }
            RefreshChildren();
        }

        protected override void OnSelectionMoving(InkCanvasSelectionEditingEventArgs e)
        {
            foreach (CustomStroke stroke in SelectedStrokes)
            {
                Vector delta = new Vector(e.NewRectangle.X - e.OldRectangle.X, e.NewRectangle.Y - e.OldRectangle.Y);
                if (!stroke.isLinkStroke())
                {
                    (stroke as ShapeStroke).shapeStyle.coordinates.x += delta.X;
                    (stroke as ShapeStroke).shapeStyle.coordinates.y += delta.Y;
                } else
                {
                    LinkStroke linkStroke = stroke as LinkStroke;
                    for (int i = 0; i < linkStroke.path.Count; i++)
                    {
                        if(i == 0 && linkStroke.isAttached() && linkStroke.from?.formId != null)
                        {
                            continue;
                        }
                        if (i == linkStroke.path.Count - 1 && linkStroke.isAttached() && linkStroke.to?.formId != null)
                        {
                            continue;
                        }
                        Coordinates coords = (stroke as LinkStroke).path[i];
                        coords.x += delta.X;
                        coords.y += delta.Y;
                    }
                }
            }
            // base.OnSelectionMoving(e);
        }
        
        protected override void OnSelectionMoved(EventArgs e)
        {
            DrawingService.UpdateShapes(SelectedStrokes);
            DrawingService.UpdateLinks(SelectedStrokes);
            RefreshLinks(true);
            RefreshChildren();
        }

        public void ResizeShape(ShapeStroke shape, RectangleGeometry NewRectangle, RectangleGeometry OldRectangle)
        {
            StrokeCollection strokes = GetSelectedStrokes();

            double heightRatio = NewRectangle.Rect.Height / OldRectangle.Rect.Height;
            double widthRatio = NewRectangle.Rect.Width / OldRectangle.Rect.Width;

            Point topLeft = NewRectangle.Transform.Transform(NewRectangle.Rect.TopLeft);
            Point topRight = NewRectangle.Transform.Transform(NewRectangle.Rect.TopRight);
            Point bottomLeft = NewRectangle.Transform.Transform(NewRectangle.Rect.BottomLeft);
            Point bottomRight = NewRectangle.Transform.Transform(NewRectangle.Rect.BottomRight);
            PointCollection pts = new PointCollection { topLeft, topRight, bottomLeft, bottomRight };
            
            double maxX = -999999999;
            double maxY = -999999999;
            double minX = 999999999;
            double minY = 999999999;
            foreach (Point point in pts)
            {
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;
                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }
            Point newCenter = new Point((maxX - minX) / 2 + minX, (maxY - minY) / 2 + minY);

            shape.shapeStyle.width *= widthRatio;
            shape.shapeStyle.height *= heightRatio;
            shape.shapeStyle.coordinates.x = newCenter.X - NewRectangle.Rect.Width / 2;
            shape.shapeStyle.coordinates.y = newCenter.Y - NewRectangle.Rect.Height / 2;

            Stroke newStroke = shape.Clone();
            StrokeCollection newStrokes = new StrokeCollection { newStroke };
            ReplaceStrokes(shape, newStrokes);

            Select(new StrokeCollection { newStroke });
        }

        protected override void OnSelectionResized(EventArgs e)
        {
            RefreshLinks(false);
            RefreshChildren();
            DrawingService.UpdateShapes(GetSelectedStrokes());
        }

        protected override void OnSelectionResizing(InkCanvasSelectionEditingEventArgs e)
        {
            // Update selected strokes height and width
            StrokeCollection strokes = GetSelectedStrokes();

            foreach(CustomStroke stroke in strokes)
            {
                    LinkStroke linkStroke = stroke as LinkStroke;

                    if (!linkStroke.isAttached())
                    {
                        linkStroke.updatePositionResizeNotAttached(e.NewRectangle);
                    }
            }
        }

        protected override void OnStrokeErasing(InkCanvasStrokeErasingEventArgs e)
        {
            StrokeCollection strokesToDelete = new StrokeCollection();
            strokesToDelete.Add(e.Stroke);

            UpdateAnchorPointsAndLinks(strokesToDelete);
            RefreshLinks(false);

            DrawingService.RemoveShapes(strokesToDelete);
            base.OnStrokeErasing(e);
        }

        // Remove deleted shapeStrokes' guids from anchor points (to and from) in LinkStrokes
        // And remove deleted linkStokes from linksTo and linksFrom in ShapeStrokes
        private void UpdateAnchorPointsAndLinks(StrokeCollection strokesToDelete)
        {
            foreach (CustomStroke strokeToDelete in strokesToDelete)
            {
                // Remove deleted linkStokes from linksTo and linksFrom in ShapeStrokes
                if (strokeToDelete.isLinkStroke())
                {
                    LinkStroke linkStroke = strokeToDelete as LinkStroke;
                    RemoveShapeStrokeLinkFrom(linkStroke);
                    RemoveShapeStrokeLinkTo(linkStroke);
                }
                else
                {
                    // Remove deleted shapeStrokes' guids from anchor points (to and from) in LinkStrokes
                    ShapeStroke shapeStroke = strokeToDelete as ShapeStroke;

                    foreach (string linkStrokeGuid in shapeStroke.linksFrom)
                    {
                        CustomStroke linkStroke;
                        if (StrokesDictionary.TryGetValue(linkStrokeGuid, out linkStroke))
                        {
                            (linkStroke as LinkStroke).from = new AnchorPoint();
                            (linkStroke as LinkStroke).from.SetDefaults();
                        }
                    }
                    foreach (string linkStrokeGuid in shapeStroke.linksTo)
                    {
                        CustomStroke linkStroke;
                        if (StrokesDictionary.TryGetValue(linkStrokeGuid, out linkStroke))
                        {
                            (linkStroke as LinkStroke).to = new AnchorPoint();
                            (linkStroke as LinkStroke).to.SetDefaults();
                        }
                    }
                }
            }
        }

        protected override void OnStrokeErased(RoutedEventArgs e)
        {
            base.OnStrokeErased(e);
        }
        #endregion

        #region OnRemote...
        private void OnRemoteStroke(InkCanvasStrokeCollectedEventArgs e)
        {
            CustomStroke stroke = (CustomStroke)e.Stroke;
            AddStroke(stroke);
            AddTextBox(stroke);
        }

        private void OnRemoteSelection(StrokeCollection strokes)
        {
            RefreshChildren();
        }

        private void OnRemoteDeselection(StrokeCollection strokes)
        {
            RefreshChildren();
        }

        private void OnRemoveStrokes(StrokeCollection strokes)
        {
            foreach (CustomStroke stroke in strokes)
            {
                foreach (CustomStroke stroke2 in Strokes)
                {
                    if(stroke.guid.Equals(stroke2.guid))
                    {
                        RemoveStroke(stroke2);
                        break;
                    }
                }
            }
            RefreshChildren();
        }
        #endregion

        private void OnUpdateStroke(InkCanvasStrokeCollectedEventArgs e)
        {
            CustomStroke stroke = (CustomStroke)e.Stroke;
            foreach (CustomStroke stroke2 in Strokes)
            {
                if (stroke.guid.Equals(stroke2.guid))
                {
                    int index = Strokes.IndexOf(stroke2);
                    Strokes.RemoveAt(index);
                    Strokes.Insert(index, stroke);
                    break;
                }
            }
            RefreshChildren();
        }

        #region OnStrokeCollected
        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Remove the original stroke and add a custom stroke.
            Strokes.Remove(e.Stroke);

            StrokeTypes strokeType = (StrokeTypes)Enum.Parse(typeof(StrokeTypes), StrokeType);

            CustomStroke customStroke = CreateStroke(e.Stroke.StylusPoints, e, strokeType);

            AddStroke(customStroke);
            if (!customStroke.isLinkStroke())
            {
                DrawingService.CreateShape(customStroke as ShapeStroke);
            }
            else
            {
                DrawingService.CreateLink(customStroke as LinkStroke);
            }
            // firstPoint = customStroke.StylusPoints[0];
            SelectedStrokes = new StrokeCollection { Strokes[Strokes.Count - 1] };
            
            //drawingService.UpdateShape("id", 0, "strokeName", shapeStyle, new List<string>(), new List<string>());

            // Pass the custom stroke to base class' OnStrokeCollected method.
            InkCanvasStrokeCollectedEventArgs args = new InkCanvasStrokeCollectedEventArgs(customStroke);
            base.OnStrokeCollected(args);

            AddTextBox(customStroke);
        }


        private CustomStroke CreateStroke(StylusPointCollection pts, InkCanvasStrokeCollectedEventArgs e, StrokeTypes strokeType)
        {
            CustomStroke customStroke;
            switch (strokeType)
            {
                case StrokeTypes.CLASS_SHAPE:
                    customStroke = new ClassStroke(pts);
                    break;
                case StrokeTypes.ARTIFACT:
                    customStroke = new ArtifactStroke(pts);
                    break;
                case StrokeTypes.ACTIVITY:
                    customStroke = new ActivityStroke(pts);
                    break;
                case StrokeTypes.ROLE:
                    customStroke = new ActorStroke(pts);
                    break;
                case StrokeTypes.COMMENT:
                    customStroke = new CommentStroke(pts);
                    break;
                case StrokeTypes.PHASE:
                    customStroke = new PhaseStroke(pts);
                    break;
                case StrokeTypes.LINK:
                    customStroke = new LinkStroke(e.Stroke.StylusPoints);
                    break;
                default:
                    customStroke = new ClassStroke(pts);
                    break;

            }
            return customStroke;
        }

        private void AddTextBox(CustomStroke stroke)
        {
            Path path = new Path();
            switch (stroke.strokeType)
            {
                case (int)StrokeTypes.LINK:
                    double x = stroke.StylusPoints[0].X;
                    double y = stroke.StylusPoints[0].Y;
                    CreateNameTextBox(stroke, x, y);
                    CreateMultiplicityTextBox(stroke.StylusPoints, stroke as LinkStroke);
                    break;
                default:
                    break;
            }
        }

        private void CreateMultiplicityTextBox(StylusPointCollection stylusPoints, LinkStroke stroke)
        {
            double fromX = stylusPoints[0].X;
            double fromY = stylusPoints[0].Y - 20;

            double toX = stylusPoints[stylusPoints.Count - 1].X;
            double toY = stylusPoints[stylusPoints.Count - 1].Y - 20;

            CustomTextBox from = new CustomTextBox();
            from.Text = "" + stroke.from.multiplicity;
            from.Uid = stroke.guid.ToString();
            Children.Add(from);
            SetTop(from, fromY);
            SetLeft(from, fromX);

            CustomTextBox to = new CustomTextBox();
            to.Text = "" + stroke.to.multiplicity;
            to.Uid = stroke.guid.ToString();
            Children.Add(to);
            SetTop(to, toY);
            SetLeft(to, toX);
        }

        private void CreateNameTextBox(CustomStroke stroke, double x, double y)
        {
            CustomTextBox tb = new CustomTextBox();
            tb.Text = stroke.name;
            tb.Uid = stroke.guid.ToString();

            Children.Add(tb);
            SetTop(tb, y);
            SetLeft(tb, x);
        }
        #endregion

        #region RotateStrokesWithAngle
        public void RotateStrokesWithAngle(double rotation)
        {
            StrokeCollection strokes = GetSelectedStrokes();
            StrokeCollection selectedNewStrokes = new StrokeCollection();

            foreach (CustomStroke selectedStroke in strokes)
            {
                Stroke newStroke = selectedStroke.CloneRotated(rotation);
                StrokeCollection newStrokes = new StrokeCollection { newStroke };
                ReplaceStrokes(selectedStroke, newStrokes);

                selectedNewStrokes.Add(newStrokes);
            }
            
            Select(selectedNewStrokes);
            DrawingService.UpdateShapes(selectedNewStrokes);
            RefreshLinks(false);
        }
        #endregion

        #region PasteStrokes
        public void PasteStrokes()
        {
            StrokeCollection strokes = GetSelectedStrokes();
            bool isClipboard = false;
            if (strokes.Count == 0)
            {
                // strokes from clipboard will be pasted
                strokes = clipboard;
                isClipboard = true;
            }

            StrokeCollection newStrokes = new StrokeCollection();

            foreach (CustomStroke stroke in strokes)
            {
                
                if ((stroke as CustomStroke).isLinkStroke())
                {
                    LinkStroke linkStroke = new LinkStroke(stroke as LinkStroke, new StylusPointCollection { new StylusPoint(0, 0) });

                    linkStroke.from.SetDefaults();
                    linkStroke.to.SetDefaults();

                    for(int i = 0; i < linkStroke.path.Count; i ++)
                    {
                        linkStroke.path[i] = linkStroke.path[i] + new Point(20, 20);
                    }
                    linkStroke.addStylusPointsToLink();

                    AddStroke(linkStroke);
                    newStrokes.Add(linkStroke);
                    // call DrawingService
                } else
                {
                    ShapeStroke newStroke = stroke.Clone() as ShapeStroke;
                    newStroke.guid = Guid.NewGuid();
                    // change author???

                    if (!isClipboard)
                    {
                        Matrix translateMatrix = new Matrix();
                        translateMatrix.Translate(20.0, 20.0);
                        newStroke.Transform(translateMatrix, false);
                    }

                    ShapeStroke newShapeStroke = newStroke as ShapeStroke;
                    newShapeStroke.linksTo = new List<string> { };
                    newShapeStroke.linksFrom = new List<string> { };
                    newShapeStroke.shapeStyle = (newStroke as ShapeStroke).shapeStyle.Clone();
                    if(!isClipboard)
                        newShapeStroke.shapeStyle.coordinates = newShapeStroke.shapeStyle.coordinates + new Point(20, 20);

                    DrawingService.CreateShape(newShapeStroke);
                    AddStroke(newShapeStroke);
                    newStrokes.Add(newShapeStroke);
                }
            }
            Select(newStrokes);
            SelectedStrokes = newStrokes;
        }
        #endregion

        #region CutStrokes
        public void CutStrokes()
        {
            StrokeCollection selectedStrokes = GetSelectedStrokes();

            DeleteStrokes(selectedStrokes);
            // put selection in clipboard to be able to paste it
            clipboard = selectedStrokes;

            // cut selection from canvas
            CutSelection();
        }
        #endregion


        internal void DeleteStrokes(StrokeCollection selectedStrokes)
        {
            SelectedStrokes.Clear();
            Select(new StrokeCollection { });
            OnSelectionChanged(new EventArgs());
            foreach (CustomStroke stroke in selectedStrokes)
            {
                if(Strokes.Contains(stroke))
                {
                    RemoveStroke(stroke);
                }
            }
            UpdateAnchorPointsAndLinks(selectedStrokes);
            RefreshChildren();
            DrawingService.RemoveShapes(selectedStrokes);
        }

        #region RefreshChildren
        public void RefreshChildren()
        {
            // ne fonctionne pas :( fait que des strokes ne sont plus ajoutees apres une 2e
            //removeAdorners();

            Children.Clear();

            isUpdatingLink = false;
            
            StrokeCollection selectedStrokes = new StrokeCollection();

            // Add selection adorners to selectedStrokes
            foreach (CustomStroke selectedStroke in GetSelectedStrokes())
            {
                selectedStrokes.Add(selectedStroke);
                addAdorners(selectedStroke);
            }

            foreach (string strokeId in DrawingService.remoteSelectedStrokes)
            {
                foreach (CustomStroke stroke in Strokes)
                {
                    if(stroke.guid.ToString().Equals(strokeId))
                    {
                        AddRemoteSelectionAdorner(stroke);
                        break;
                    }
                }
            }
            // Add text boxes (names) to all strokes. And add dotted path if linkStroke is dotted
            foreach (CustomStroke stroke in Strokes)
            {
                AddTextBox(stroke);
                if (stroke.isLinkStroke() && (stroke as LinkStroke).style.type == 1) // dotted linkStroke
                {
                    Path path = new Path();
                    path.Data = stroke.GetGeometry();

                    Children.Add(path);
                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(path);
                    myAdornerLayer.Add(new DottedPathAdorner(path, stroke as LinkStroke, this));
                }
            }
            // Select(selectedStrokes);
        }

        private void addAdorners(CustomStroke selectedStroke)
        {
            Path path = new Path();
            path.Data = selectedStroke.GetGeometry();

            Children.Add(path);
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(path);

            if (!selectedStroke.isLinkStroke())
            {
                if (GetSelectedStrokes().Count == 1)
                {
                    myAdornerLayer.Add(new EditionAdorner(path, selectedStroke, this));
                    myAdornerLayer.Add(new RotateAdorner(path, selectedStroke, this));
                    myAdornerLayer.Add(new ResizeAdorner(path, selectedStroke, this));
                }
                myAdornerLayer.Add(new AnchorPointAdorner(path, selectedStroke, this));
                /*if (selectedStroke.strokeType == (int)StrokeTypes.CLASS_SHAPE)
                {
                    myAdornerLayer.Add(new ClassAdorner(path, selectedStroke, this));
                }*/
            } else
            {
                if(!(selectedStroke as LinkStroke).isAttached() && GetSelectedStrokes().Count == 1)
                {
                    myAdornerLayer.Add(new LinkRotateAdorner(path, selectedStroke as LinkStroke, this));
                }
                myAdornerLayer.Add(new LinkAnchorPointAdorner(path, selectedStroke as LinkStroke, this));
                myAdornerLayer.Add(new EditionAdorner(path, selectedStroke, this));
            }

        }

        private void AddRemoteSelectionAdorner(CustomStroke stroke)
        {
            Path path = new Path();
            path.Data = stroke.GetGeometry();

            Children.Add(path);
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(path);
            myAdornerLayer.Add(new RemoteSelectionAdorner(path, stroke, this));
        }

        public void addAnchorPoints()
        {
            foreach (CustomStroke stroke in Strokes)
            {
                if(stroke.GetType() != typeof(LinkStroke))
                {
                    Path path = new Path();
                    path.Data = stroke.GetGeometry();

                    Children.Add(path);
                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(path);
                    myAdornerLayer.Add(new AnchorPointAdorner(path, stroke, this));
                }
            }
        }
        
        // ne fonctionne pas :( fait que des strokes ne sont plus ajoutees apres une 2e stroke ajoutee
        private void removeAdorners()
        {
            for(int i = 0; i < Children.Count; i++)
            {
                if (Children[i].GetType() == typeof(Path))
                {
                    Children.RemoveAt(i);
                }
                else if (Children[i]?.GetType() == typeof(CustomTextBox))
                {
                    Children.RemoveAt(i);
                }

            }
            //List<UIElement> children = new List<UIElement>();

            //foreach (UIElement child in Children)
            //{
            //    if (child.GetType() == typeof(Path))
            //    {
            //        children.Add(child);
            //    }
            //}

            //foreach (UIElement child in children)
            //{
            //    Children.Remove(child);
            //}
        }
        #endregion

        
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (EditingMode == InkCanvasEditingMode.Select)
            {
                SelectedStrokes = GetSelectedStrokes();
                selectionPath.StartPoint = e.GetPosition(this);
                selectionPath.Segments = new PathSegmentCollection();
                selectionPath.IsClosed = true;
                beingSelected.Clear();
                foreach (CustomStroke stroke in SelectedStrokes)
                {
                    if (stroke.HitTestPointIncludingEdition(e.GetPosition(this)))
                    {
                        base.OnPreviewMouseDown(e);
                        return;
                    }
                }
                if (SelectedStrokes.Count > 0)
                {
                    double minX = 9999999;
                    double maxX = -9999999;
                    double minY = 9999999;
                    double maxY = -9999999;
                    foreach (CustomStroke stroke in SelectedStrokes)
                    {
                        if(stroke.GetEditingBounds().X < minX)
                        {
                            minX = stroke.GetEditingBounds().X;
                        }
                        if (stroke.GetEditingBounds().Y < minY)
                        {
                            minY = stroke.GetEditingBounds().Y;
                        }
                        if (stroke.GetEditingBounds().BottomRight.X > maxX)
                        {
                            maxX = stroke.GetEditingBounds().BottomRight.X;
                        }
                        if (stroke.GetEditingBounds().BottomRight.Y > maxY)
                        {
                            maxY = stroke.GetEditingBounds().BottomRight.Y;
                        }
                    }
                    if (new Rect(new Point(minX, minY), new Point(maxX, maxY)).Contains(e.GetPosition(this)))
                    {
                        base.OnPreviewMouseDown(e);
                        return;
                    }
                }
                selectionPath.Segments.Add(new LineSegment(e.GetPosition(this), true));
                foreach (CustomStroke stroke in Strokes)
                {
                    if (stroke.HitTestPoint(e.GetPosition(this)) && 
                        !DrawingService.remoteSelectedStrokes.Contains(stroke.guid.ToString()))
                    {
                        if (beingSelected.Any())
                            beingSelected.Clear();
                        beingSelected.Add(stroke);
                    }
                }
            }
            else if (EditingMode == InkCanvasEditingMode.EraseByStroke)
            {
                selectionPath.Segments = new PathSegmentCollection();
                selectionPath.Segments.Add(new LineSegment(e.GetPosition(this), true));
                beingSelected = new StrokeCollection();
                foreach (CustomStroke stroke in Strokes)
                {
                    if (stroke is ShapeStroke && stroke.HitTestPoint(e.GetPosition(this)) &&
                        !DrawingService.remoteSelectedStrokes.Contains(stroke.guid.ToString()))
                    {
                        beingSelected.Add(stroke);
                    }
                }
                if (beingSelected.Any())
                {
                    Strokes.Remove(beingSelected);
                    DrawingService.RemoveShapes(beingSelected);
                    RefreshChildren();
                    beingSelected.Clear();
                }
            } else
            {
                base.OnPreviewMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (EditingMode == InkCanvasEditingMode.Select)
            {
                if (selectionPath.Segments.Any())
                    selectionPath.Segments.Add(new LineSegment(e.GetPosition(this), true));
            }
            else if (EditingMode == InkCanvasEditingMode.EraseByStroke)
            {
                if (selectionPath.Segments.Any())
                {
                    beingSelected = new StrokeCollection();
                    foreach (CustomStroke stroke in Strokes)
                    {
                        if (stroke is ShapeStroke && stroke.HitTestPoint(e.GetPosition(this)) && 
                            !DrawingService.remoteSelectedStrokes.Contains(stroke.guid.ToString()))
                        {
                            beingSelected.Add(stroke);
                        }
                    }
                    if (beingSelected.Any())
                    {
                        Strokes.Remove(beingSelected);
                        DrawingService.RemoveShapes(beingSelected);
                        RefreshChildren();
                        beingSelected.Clear();
                    }
                }
            }
            else
            {
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (EditingMode == InkCanvasEditingMode.Select)
            {
                if (selectionPath.Segments.Any())
                {
                    if (selectionPath.Segments.Count < 5)
                    {
                        SelectedStrokes.Clear();
                        Select(beingSelected);
                        selectionPath.Segments.Clear();
                    }
                    else
                    {
                        beingSelected.Clear();
                        SelectedStrokes.Clear();
                        PathFigureCollection figures = new PathFigureCollection { selectionPath };
                        PathGeometry geometry = new PathGeometry(figures);
                        foreach (CustomStroke stroke in Strokes)
                        {
                            if (!DrawingService.remoteSelectedStrokes.Contains(stroke.guid.ToString())) {
                                if (stroke.isLinkStroke())
                                {
                                    bool isInside = true;
                                    foreach (Coordinates point in (stroke as LinkStroke).path)
                                    {
                                        isInside = geometry.FillContains(point.ToPoint());
                                        if (!isInside)
                                        {
                                            break;
                                        }
                                    }
                                    if (isInside)
                                    {
                                        beingSelected.Add(stroke);
                                    }
                                }
                                else
                                {
                                    RectangleGeometry strokeBound = new RectangleGeometry(stroke.GetCustomBound());
                                    if (geometry.FillContains(strokeBound))
                                    {
                                        beingSelected.Add(stroke);
                                    }
                                }
                            }
                        }
                        Select(beingSelected);
                        selectionPath.Segments.Clear();
                    }
                }
            }
            else if (EditingMode == InkCanvasEditingMode.EraseByStroke)
            {
                selectionPath.Segments.Clear();
            }
            else
            {
                base.OnMouseUp(e);
            }
        }

        public void RefreshSelectedShape(ShapeStroke stroke)
        {
            ShapeStroke strokeCopy = (ShapeStroke)stroke.Clone();
            StrokeCollection strokes = new StrokeCollection() { strokeCopy };
            Strokes.Replace(stroke, strokes);
            Select(strokes);
        }

        public void ConvertInkCanvasToByteArray()
        {
            var rect = new Rect(RenderSize);
            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(this), null, rect);
            }

            var rtb = new RenderTargetBitmap(
                (int)rect.Width, (int)rect.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                buffer = stream.ToArray();
                string thumbnailString = Convert.ToBase64String(buffer);
                DrawingService.SendCanvas(thumbnailString);
            }

            sd.Image image = (sd.Bitmap)((new sd.ImageConverter()).ConvertFrom(buffer));

            sd.Bitmap bitmap = ResizeImage(image, 80, 50);

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, sd.Imaging.ImageFormat.Png);
                byte [] resizedImageBuffer = stream.ToArray();
                string thumbnailString = Convert.ToBase64String(resizedImageBuffer);
                //DrawingService.SendCanvas(thumbnailString);
            }
        }

        
        private sd.Bitmap ResizeImage(sd.Image image, int width, int height)
        {
            var destRect = new sd.Rectangle(0, 0, width, height);
            var destImage = new sd.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = sd.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = s2d.CompositingMode.SourceCopy;
                graphics.CompositingQuality = s2d.CompositingQuality.HighQuality;
                graphics.InterpolationMode = s2d.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = s2d.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = s2d.PixelOffsetMode.HighQuality;

                using (var wrapMode = new sd.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(s2d.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, sd.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


    }
}
