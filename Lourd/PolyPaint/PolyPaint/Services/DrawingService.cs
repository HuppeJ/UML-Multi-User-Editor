using Newtonsoft.Json.Linq;
using PolyPaint.CustomInk;
using PolyPaint.CustomInk.Strokes;
using PolyPaint.Enums;
using PolyPaint.Templates;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;

namespace PolyPaint.Services
{
    class DrawingService : ConnectionService
    {
        public static event Action<AccessCanvasResponse> JoinCanvasRoom;
        public static event Action<InkCanvasStrokeCollectedEventArgs> AddStroke;
        public static event Action<StrokeCollection> RemoveStrokes;
        public static event Action<InkCanvasStrokeCollectedEventArgs> UpdateStroke;
        public static event Action UpdateSelection;
        public static event Action UpdateDeselection;

        public static event Action<PublicCanvases> UpdatePublicCanvases;
        public static event Action<PrivateCanvases> UpdatePrivateCanvases;
        public static event Action<Coordinates> OnResizeCanvas;
        public static event Action RemoteReset;
        public static event Action BackToGallery;
        public static event Action LeaveDrawingAction;
        public static event Action SaveCanvas;
        public static event Action RefreshChildren;
        public static event Action ReintializeCanvas;
        public static event Action GoToTutorial;
        public static event Action CanvasCreationFailed;
        public static event Action<string> UpdateCanvasNameAction;

        public static event Action<History> UpdateHistory;

        private static JavaScriptSerializer serializer = new JavaScriptSerializer {
            MaxJsonLength = int.MaxValue
        };
        public static string canvasName;
        public static bool saving = false;
        public static Templates.Canvas currentCanvas;
        public static List<string> remoteSelectedStrokes = new List<string>();
        public static List<string> localSelectedStrokes = new List<string>();
        public static List<string> localAddedStrokes = new List<string>();
        public static bool isCanvasSizeRemotelyEditing = false;
        public static bool isCanvasSizeLocalyEditing = false;

        public DrawingService()
        {

        }

        public static void Initialize(object o)
        {
            socket.On(Socket.EVENT_RECONNECT, () =>
            {
                if (canvasName != null)
                {
                    // Stocker le mot de passe? ADD
                    JoinCanvas(canvasName, "");
                }
            });

            #region canvas .On
            socket.On("createCanvasResponse", (data) =>
            {
                CreateCanvasResponse response = serializer.Deserialize<CreateCanvasResponse>((string)data);
                if (response.isCreated)
                {
                    canvasName = response.canvasName;
                    RefreshCanvases();
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { CanvasCreationFailed(); }), DispatcherPriority.Render);
                }
            });

            socket.On("canvasCreated", (data) =>
            {
                RefreshCanvases();
            });

            socket.On("updateCanvasPasswordResponse", (data) =>
            {
                UpdateCanvasPasswordResponse response = serializer.Deserialize<UpdateCanvasPasswordResponse>((string)data);
                if (response.isPasswordUpdated)
                {
                    RefreshCanvases();
                }
            });

            socket.On("canvasPasswordUpdated", (data) =>
            {
                LeaveCanvas();
                Application.Current?.Dispatcher?.Invoke(new Action(() => { BackToGallery(); }), DispatcherPriority.Render);
            });

            socket.On("accessCanvasResponse", (data) =>
            {
                AccessCanvasResponse response = serializer.Deserialize<AccessCanvasResponse>((string)data);
                if (response.isPasswordValid)
                {
                    canvasName = response.canvasName;
                }
                Application.Current?.Dispatcher?.Invoke(new Action(() => { JoinCanvasRoom(response); }), DispatcherPriority.Render);
            });

            socket.On("canvasSelected", (data) =>
            {
                EditGalleryData response = serializer.Deserialize<EditGalleryData>((string)data);
                if (!username.Equals((string)response.username) && response.canevasName.Equals(canvasName))
                {
                    isCanvasSizeRemotelyEditing = true;
                }
            });

            socket.On("canvasDeselected", (data) =>
            {
                EditGalleryData response = serializer.Deserialize<EditGalleryData>((string)data);
                if (!username.Equals((string)response.username) && response.canevasName.Equals(canvasName))
                {
                    isCanvasSizeRemotelyEditing = false;
                }
            });

            socket.On("canvasResized", (data) =>
            {
                ResizeCanevasData response = serializer.Deserialize<ResizeCanevasData>((string)data);
                if (username != null && !username.Equals((string)response.username))
                {
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { OnResizeCanvas(response.dimensions); }), DispatcherPriority.Render);
                }
            });

            socket.On("getPublicCanvasResponse", (data) =>
            {
                PublicCanvases canvases = serializer.Deserialize<PublicCanvases>((string)data);

                ExtractCanvasesShapes(canvases.publicCanvas);

                try
                {
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdatePublicCanvases(canvases); }), DispatcherPriority.Render);
                }
                catch { }
            });

            socket.On("getPrivateCanvasResponse", (data) =>
            {
                PrivateCanvases canvases = serializer.Deserialize<PrivateCanvases>((string)data);

                ExtractCanvasesShapes(canvases.privateCanvas);
                Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdatePrivateCanvases(canvases); }), DispatcherPriority.Render);
            });
            #endregion

            #region links .On 
            socket.On("linkCreated", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    LinkStroke linkStroke = createLinkStroke(response.links[0]);
                    linkStroke.owner = response.username;
                    InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(linkStroke);
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { AddStroke(eventArgs); }), DispatcherPriority.ContextIdle);
                }
            });

            socket.On("linksDeleted", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic link in response.links)
                    {
                        strokes.Add(createLinkStroke(link));
                    }
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { RemoveStrokes(strokes); }), DispatcherPriority.ContextIdle);
                }
            });

            socket.On("linksUpdated", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    foreach (dynamic link in response.links)
                    {
                        InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(createLinkStroke(link));
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateStroke(eventArgs); }), DispatcherPriority.Render);
                    }
                }
            });

            socket.On("linksSelected", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic link in response.links)
                    {
                        LinkStroke linkStroke = createLinkStroke(link);
                        strokes.Add(linkStroke);
                        if (!remoteSelectedStrokes.Contains(linkStroke.guid.ToString()))
                            remoteSelectedStrokes.Add(linkStroke.guid.ToString());
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateSelection(); }), DispatcherPriority.Render);
                    }
                }
            });

            socket.On("linksDeselected", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic link in response.links)
                    {
                        LinkStroke linkStroke = createLinkStroke(link);
                        strokes.Add(linkStroke);
                        if (remoteSelectedStrokes.Contains(linkStroke.guid.ToString()))
                            remoteSelectedStrokes.Remove(linkStroke.guid.ToString());
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateDeselection(); }), DispatcherPriority.Render);
                    }
                }
            });
            #endregion

            #region forms .On
            socket.On("formCreated", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                //Console.WriteLine("FORM CREATED: \n" + "position: " + response.forms[0].shapeStyle.coordinates.x + ", " 
                //    + response.forms[0].shapeStyle.coordinates.y + "/n" + "size: " + response.forms[0].shapeStyle.width + ", "
                //    + response.forms[0].shapeStyle.height);
                if (!username.Equals((string)response.username))
                {
                    CustomStroke customStroke = createShapeStroke(response.forms[0]);
                    InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(customStroke);
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { AddStroke(eventArgs); }), DispatcherPriority.ContextIdle);
                }
            });

            socket.On("formsDeleted", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic shape in response.forms)
                    {
                        ShapeStroke shapeStroke = createShapeStroke(shape);
                        if (remoteSelectedStrokes.Contains(shapeStroke.guid.ToString()))
                        {
                            remoteSelectedStrokes.Remove(shapeStroke.guid.ToString());
                        }
                        strokes.Add(shapeStroke);
                    }
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { RemoveStrokes(strokes); }), DispatcherPriority.ContextIdle);
                }
            });

            socket.On("formsUpdated", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    foreach (dynamic shape in response.forms)
                    {
                        InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(createShapeStroke(shape));
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateStroke(eventArgs); }), DispatcherPriority.Render);
                    }
                }
            });

            socket.On("formsSelected", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic shape in response.forms)
                    {
                        ShapeStroke stroke = createShapeStroke(shape);
                        strokes.Add(stroke);
                        if (!remoteSelectedStrokes.Contains(stroke.guid.ToString()))
                            remoteSelectedStrokes.Add(stroke.guid.ToString());
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateSelection(); }), DispatcherPriority.Render);
                    }
                }
            });

            socket.On("formsDeselected", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                if (!username.Equals((string)response.username))
                {
                    StrokeCollection strokes = new StrokeCollection();
                    foreach (dynamic shape in response.forms)
                    {
                        ShapeStroke stroke = createShapeStroke(shape);
                        strokes.Add(stroke);
                        if (remoteSelectedStrokes.Contains(stroke.guid.ToString()))
                            remoteSelectedStrokes.Remove(stroke.guid.ToString());
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateDeselection(); }), DispatcherPriority.Render);
                    }
                }
            });
            #endregion

            socket.On("selectedForms", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                foreach (string id in response.selectedForms)
                {
                    if (!remoteSelectedStrokes.Contains(id))
                    {
                        remoteSelectedStrokes.Add(id);
                    }
                }
                Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateDeselection(); }), DispatcherPriority.Render);
            });

            socket.On("selectedLinks", (data) =>
            {
                dynamic response = JObject.Parse((string)data);
                foreach (string id in response.selectedLinks)
                {
                    if (!remoteSelectedStrokes.Contains(id))
                    {
                        remoteSelectedStrokes.Add(id);
                    }
                }
                Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateDeselection(); }), DispatcherPriority.Render);
            });

            socket.On("canvasSaved", (data) =>
            {
                EditCanevasData response = serializer.Deserialize<EditCanevasData>((string)data);
                RefreshCanvases();
                saving = false;
            });

            socket.On("getCanvasResponse", (data) =>
            {
                Templates.Canvas canvas = serializer.Deserialize<Templates.Canvas>((string)data);
                canvasName = canvas.name;
                currentCanvas = canvas;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { OnResizeCanvas(canvas.dimensions); }), DispatcherPriority.ContextIdle);
                localSelectedStrokes = new List<string>();
                localAddedStrokes = new List<string>();
                EditGalleryData editGallerysData = new EditGalleryData(username, canvasName, "");

                foreach (Link link in canvas.links)
                {
                    LinkStroke linkStroke = LinkStrokeFromLink(link);
                    InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(linkStroke);
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { AddStroke(eventArgs); }), DispatcherPriority.ContextIdle);
                }

                List<BasicShape> basicShapes = ExtractShapesFromCanvas(canvas);
                foreach (BasicShape shape in basicShapes)
                {
                    ShapeStroke shapeStroke = ShapeStrokeFromShape(shape);
                    InkCanvasStrokeCollectedEventArgs eventArgs = new InkCanvasStrokeCollectedEventArgs(shapeStroke);
                    Application.Current?.Dispatcher?.Invoke(new Action(() => { AddStroke(eventArgs); }), DispatcherPriority.ContextIdle);
                }

                socket.Emit("getSelectedForms", serializer.Serialize(editGallerysData));
                socket.Emit("getSelectedLinks", serializer.Serialize(editGallerysData));

                Application.Current?.Dispatcher?.Invoke(new Action(() => { RefreshChildren(); }), DispatcherPriority.ContextIdle);
            });

            socket.On("canvasReinitialized", (data) =>
            {
                localAddedStrokes = new List<string>();
                localSelectedStrokes = new List<string>();
                remoteSelectedStrokes = new List<string>();
                Application.Current?.Dispatcher?.Invoke(new Action(() => { RemoteReset(); }), DispatcherPriority.Render);
            });

            socket.On("getCanvasLogHistoryResponse", (data) =>
            {
                HistoryData[] historyData = serializer.Deserialize<HistoryData[]>((string)data);
                History history = new History(historyData);

                Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateHistory(history); }), DispatcherPriority.Render);
            });

            socket.On("disconnect", (data) =>
            {
                // Application.Current?.Dispatcher?.Invoke(new Action(() => { BackToGallery(); }), DispatcherPriority.ContextIdle);
            });

            RefreshCanvases();
        }

        #region Emit
        public static void CreateCanvas(Templates.Canvas canvas)
        {
            EditCanevasData editCanevasData = new EditCanevasData(username, canvas);
            editCanevasData.canevas.dimensions.x *= 2.1;
            editCanevasData.canevas.dimensions.y *= 2.1;
            currentCanvas = canvas;
            socket.Emit("createCanvas", serializer.Serialize(editCanevasData));
        }

        public static void ChangeCanvasProtection(string canvasName, string password)
        {
            EditGalleryData editGallerysData = new EditGalleryData(username, canvasName, password);
            socket.Emit("updateCanvasPassword", serializer.Serialize(editGallerysData));
        }

        public static void JoinCanvas(string roomName, string password)
        {
            EditGalleryData editGalleryData = new EditGalleryData(username, roomName, password);
            socket.Emit("accessCanvas", serializer.Serialize(editGalleryData));
        }

        public static void JoinCanvasRoomServer(string roomName, string password)
        {
            EditGalleryData editGalleryData = new EditGalleryData(username, roomName, password);
            socket.Emit("joinCanvasRoom", serializer.Serialize(editGalleryData));
        }

        public static void isResizingCanvas(bool isResizing)
        {
            isCanvasSizeLocalyEditing = isResizing;
            EditGalleryData editGalleryData = new EditGalleryData(username, canvasName);
            if (isResizing)
            {
                socket.Emit("selectCanvas", serializer.Serialize(editGalleryData));
            }
            else
            {
                socket.Emit("deselectCanvas", serializer.Serialize(editGalleryData));
            }
        }

        public static void ResizeCanvas(Coordinates coordinates)
        {
            ResizeCanevasData editCanevasData = new ResizeCanevasData(username, canvasName, coordinates);
            socket.Emit("resizeCanvas", serializer.Serialize(editCanevasData));
        }

        public static void DrawCanvas(Templates.Canvas canvas)
        {
            EditGalleryData editGalleryData = new EditGalleryData(username, canvasName);
            socket.Emit("getCanvas", serializer.Serialize(editGalleryData));
        }

        public static void LeaveCanvas()
        {
            EditGalleryData editGalleryData = new EditGalleryData(username, canvasName);
            socket.Emit("leaveCanvasRoom", serializer.Serialize(editGalleryData));
            RefreshCanvases();
        }

        public static void SendCanvas(string thumbnail)
        {
            Templates.Canvas canvas = new Templates.Canvas();
            canvas.name = canvasName;
            canvas.thumbnail = thumbnail;
            EditCanevasData editCanevasData = new EditCanevasData(username, canvas);
            // Commente pour pas buster le cloud (à uncomment avant la remise)
            socket.Emit("saveCanvas", serializer.Serialize(editCanevasData));
        }

        public static void RefreshCanvases()
        {
            socket.Emit("getPublicCanvas");
            socket.Emit("getPrivateCanvas", username);
        }

        public static void RefreshPublicCanvases()
        {
            socket.Emit("getPublicCanvas");
        }

        public static void ResetServer()
        {
            socket.Emit("resetServerState");
        }

        public static void CreateShape(ShapeStroke shapeStroke)
        {
            socket.Emit("createForm", serializer.Serialize(createUpdateFormsData(new StrokeCollection { shapeStroke })));
            localAddedStrokes.Add(shapeStroke.guid.ToString());
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        public static void CreateLink(LinkStroke linkStroke)
        {
            socket.Emit("createLink", serializer.Serialize(createUpdateLinksData(new StrokeCollection { linkStroke })));
            localAddedStrokes.Add(linkStroke.guid.ToString());
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        public static void RemoveShapes(StrokeCollection strokes)
        {
            foreach (CustomStroke stroke in strokes)
            {
                if (localSelectedStrokes.Contains(stroke.guid.ToString()))
                    localSelectedStrokes.Remove(stroke.guid.ToString());
            }

            EmitIfStrokes("deleteForms", createUpdateFormsData(strokes));
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }

            EmitIfStrokes("deleteLinks", createUpdateLinksData(strokes));
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        public static void Reset()
        {
            localSelectedStrokes = new List<string>();
            localAddedStrokes = new List<string>();
            remoteSelectedStrokes = new List<string>();
            socket.Emit("reinitializeCanvas", serializer.Serialize(new EditCanevasData(username, currentCanvas)));
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        private static void EmitIfStrokes(string eventString, UpdateFormsData shapes)
        {
            if (shapes.forms.Count > 0)
            {
                socket.Emit(eventString, serializer.Serialize(shapes));
                if (eventString != "selectForms")
                {
                    if (!saving)
                    {
                        saving = true;
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
                    }
                }
            }
        }
        private static void EmitIfStrokes(string eventString, UpdateLinksData links)
        {
            if (links.links.Count > 0)
            {
                socket.Emit(eventString, serializer.Serialize(links));
                if(eventString != "selectLinks")
                {
                    if (!saving)
                    {
                        saving = true;
                        Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
                    }
                }
            }
        }

        public static void UpdateShapes(StrokeCollection strokes)
        {
            EmitIfStrokes("updateForms", createUpdateFormsData(strokes));
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        public static void UpdateLinks(StrokeCollection strokes)
        {
            EmitIfStrokes("updateLinks", createUpdateLinksData(strokes));
            if (!saving)
            {
                saving = true;
                Application.Current?.Dispatcher?.Invoke(new Action(() => { SaveCanvas(); }), DispatcherPriority.Render);
            }
        }

        public static void SelectShapes(StrokeCollection strokes)
        {
            foreach (CustomStroke stroke in strokes)
            {
                if (!localSelectedStrokes.Contains(stroke.guid.ToString()))
                    localSelectedStrokes.Add(stroke.guid.ToString());
            }
            EmitIfStrokes("selectForms", createUpdateFormsData(strokes));
            EmitIfStrokes("selectLinks", createUpdateLinksData(strokes));
        }

        public static void DeselectShapes(StrokeCollection strokes)
        {
            foreach (CustomStroke stroke in strokes)
            {
                if (localSelectedStrokes.Contains(stroke.guid.ToString()))
                    localSelectedStrokes.Remove(stroke.guid.ToString());
            }
            EmitIfStrokes("deselectForms", createUpdateFormsData(strokes));
            EmitIfStrokes("deselectLinks", createUpdateLinksData(strokes));
        }
        #endregion

        private static UpdateFormsData createUpdateFormsData(StrokeCollection strokes)
        {
            List<BasicShape> forms = new List<BasicShape>();
            foreach (CustomStroke customStroke in strokes)
            {
                if (!customStroke.isLinkStroke())
                {
                    if (customStroke.strokeType == (int)StrokeTypes.CLASS_SHAPE)
                    {
                        BasicShape basicShape = (customStroke as ClassStroke).GetBasicShape().forServer();
                        if (basicShape.linksTo == null)
                        {
                            basicShape.linksTo = new List<string>();
                        }
                        if (basicShape.linksFrom == null)
                        {
                            basicShape.linksFrom = new List<string>();
                        }
                        if (basicShape.shapeStyle == null)
                        {
                            basicShape.shapeStyle = new ShapeStyle(new Coordinates(0,0), 1, 1, 0, "#000000", 0, "#00000000");
                        }
                        forms.Add(basicShape);
                    }
                    else
                    {
                        BasicShape basicShape = (customStroke as ShapeStroke).GetBasicShape().forServer();
                        if (basicShape.linksTo == null)
                        {
                            basicShape.linksTo = new List<string>();
                        }
                        if (basicShape.linksFrom == null)
                        {
                            basicShape.linksFrom = new List<string>();
                        }
                        if (basicShape.shapeStyle == null)
                        {
                            basicShape.shapeStyle = new ShapeStyle(new Coordinates(0, 0), 1, 1, 0, "#000000", 0, "#00000000");
                        }
                        forms.Add(basicShape);
                    }
                }
            }

            return new UpdateFormsData(username, canvasName, forms);
        }

        private static UpdateLinksData createUpdateLinksData(StrokeCollection strokes)
        {
            List<Link> links = new List<Link>();
            foreach (CustomStroke customStroke in strokes)
            {
                if (customStroke.isLinkStroke())
                {
                    Link link = (customStroke as LinkStroke).GetLinkShape();
                    if(link.style == null)
                    {
                        link.style = new LinkStyle("#000000", 0, 0);
                    }
                    if (link.path == null || link.from == null || link.to == null || link.path.Count == 0)
                    {
                        continue;
                    }
                    links.Add(link);
                }
            }

            return new UpdateLinksData(username, canvasName, links);
        }

        private static LinkStroke createLinkStroke(dynamic link)
        {
            for (int i = 0; i < link.path.Count; i++)
            {
                link.path[i].x /= 2.1;
                link.path[i].y /= 2.1;
            }

            LinkStroke linkStroke = new LinkStroke(link.ToObject<Link>(), new StylusPointCollection { new StylusPoint(0, 0) });
            linkStroke.guid = Guid.Parse((string)link.id);

            linkStroke.to = linkStroke.to.GetForLourd();
            linkStroke.from = linkStroke.from.GetForLourd();

            return linkStroke;
        }

        private static ShapeStroke createShapeStroke(dynamic shape)
        {
            StylusPointCollection points = new StylusPointCollection {
                new StylusPoint((double)shape.shapeStyle.coordinates.x / 2.1, (double)shape.shapeStyle.coordinates.y / 2.1)
            };

            shape.shapeStyle.height = (double)shape.shapeStyle.height / 2.1;
            shape.shapeStyle.width = (double)shape.shapeStyle.width / 2.1;
            shape.shapeStyle.coordinates.x = (double)shape.shapeStyle.coordinates.x / 2.1;
            shape.shapeStyle.coordinates.y = (double)shape.shapeStyle.coordinates.y / 2.1;

            ShapeStroke shapeStroke;
            StrokeTypes type = (StrokeTypes)shape.type;

            switch (type)
            {
                case StrokeTypes.CLASS_SHAPE:
                    shapeStroke = new ClassStroke(shape.ToObject<ClassShape>(), points);
                    break;
                case StrokeTypes.ARTIFACT:
                    shapeStroke = new ArtifactStroke(shape.ToObject<BasicShape>(), points);
                    break;
                case StrokeTypes.ACTIVITY:
                    shapeStroke = new ActivityStroke(shape.ToObject<BasicShape>(), points);
                    break;
                case StrokeTypes.ROLE:
                    shapeStroke = new ActorStroke(shape.ToObject<BasicShape>(), points);
                    break;
                case StrokeTypes.COMMENT:
                    shapeStroke = new CommentStroke(shape.ToObject<BasicShape>(), points);
                    break;
                case StrokeTypes.PHASE:
                    shapeStroke = new PhaseStroke(shape.ToObject<BasicShape>(), points);
                    break;
                case StrokeTypes.FLOATINGTEXT:
                    shapeStroke = new FloatingTextStroke(shape.ToObject<BasicShape>(), points);
                    break;
                default:
                    shapeStroke = new ClassStroke(shape.ToObject<ClassShape>(), points);
                    break;

            }
            shapeStroke.guid = Guid.Parse((string)shape.id);

            return shapeStroke;
        }

        private static ShapeStroke ShapeStrokeFromShape(BasicShape shape)
        {
            StrokeTypes type = (StrokeTypes)shape.type;
            ShapeStroke shapeStroke;

            StylusPointCollection points = new StylusPointCollection();
            StylusPoint point = new StylusPoint(shape.shapeStyle.coordinates.x, shape.shapeStyle.coordinates.y);
            points.Add(point);

            switch (type)
            {
                case StrokeTypes.CLASS_SHAPE:
                    shapeStroke = new ClassStroke((ClassShape)shape, points);
                    break;
                case StrokeTypes.ARTIFACT:
                    shapeStroke = new ArtifactStroke(shape, points);
                    break;
                case StrokeTypes.ACTIVITY:
                    shapeStroke = new ActivityStroke(shape, points);
                    break;
                case StrokeTypes.ROLE:
                    shapeStroke = new ActorStroke(shape, points);
                    break;
                case StrokeTypes.COMMENT:
                    shapeStroke = new CommentStroke(shape, points);
                    break;
                case StrokeTypes.PHASE:
                    shapeStroke = new PhaseStroke(shape, points);
                    break;
                case StrokeTypes.FLOATINGTEXT:
                    shapeStroke = new FloatingTextStroke(shape, points);
                    break;
                default:
                    shapeStroke = new ActorStroke(shape, points);
                    break;
            }

            return shapeStroke;
        }

        private static LinkStroke LinkStrokeFromLink(Link link)
        {
            StrokeTypes type = (StrokeTypes)link.type;
            for (int i = 0; i < link.path.Count; i++)
            {
                link.path[i].x /= 2.1;
                link.path[i].y /= 2.1;
            }

            link.from = link.from.GetForLourd();
            link.to = link.to.GetForLourd();

            return new LinkStroke(link, new StylusPointCollection { new StylusPoint(0, 0) });
        }

        private static void ExtractCanvasesShapes(List<Templates.Canvas> canvasesList)
        {
            if (canvasesList.Count > 0)
            {
                foreach (Templates.Canvas canvas in canvasesList)
                {
                    canvas.shapes = ExtractShapesFromCanvas(canvas);
                }
            }
        }

        private static List<BasicShape> ExtractShapesFromCanvas(Templates.Canvas canvas)
        {
            List<BasicShape> basicShapeList = new List<BasicShape>();
            dynamic shapes = canvas.shapes;
            for (int i = 0; i < shapes.Length; i++)
            {
                // If there are 8 attributes, it's a class, else, it's a basicShape
                if (shapes[i].Count == 8)
                {
                    string id = shapes[i]["id"];
                    int type = shapes[i]["type"];
                    string name = shapes[i]["name"];
                    var shapeStyle = GetShapeStyle(shapes[i]["shapeStyle"]);
                    List<string> linksTo = GetStringList(shapes[i]["linksTo"]);
                    List<string> linksFrom = GetStringList(shapes[i]["linksFrom"]);
                    List<string> attributes = GetStringList(shapes[i]["attributes"]);
                    List<string> methods = GetStringList(shapes[i]["methods"]);
                    basicShapeList.Add(new ClassShape(id, type, name, shapeStyle, linksTo, linksFrom, attributes, methods));
                }
                else
                {
                    string id = shapes[i]["id"];
                    int type = shapes[i]["type"];
                    string name = shapes[i]["name"];
                    var shapeStyle = GetShapeStyle(shapes[i]["shapeStyle"]);
                    List<string> linksTo = GetStringList(shapes[i]["linksTo"]);
                    List<string> linksFrom = GetStringList(shapes[i]["linksFrom"]);
                    basicShapeList.Add(new BasicShape(id, type, name, shapeStyle, linksTo, linksFrom));
                }
            }

            return basicShapeList;
        }

        private static dynamic GetShapeStyle(dynamic shapeStyle)
        {
            double width = (double)shapeStyle["width"] / 2.1;
            double height = (double)shapeStyle["height"] / 2.1;
            double rotation = (double)shapeStyle["rotation"];
            string borderColor = shapeStyle["borderColor"];
            int borderStyle = shapeStyle["borderStyle"];
            string backgroundColor = shapeStyle["backgroundColor"];
            double x = (double)shapeStyle["coordinates"]["x"] / 2.1;
            double y = (double)shapeStyle["coordinates"]["y"] / 2.1;
            Coordinates coordinates = new Coordinates(x, y);

            return new ShapeStyle(coordinates, width, height, rotation, borderColor, borderStyle, backgroundColor);
        }

        private static dynamic GetStringList(dynamic items)
        {
            List<string> list = new List<string>();

            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    list.Add((string)items[i]);
                }
            }

            return list;
        }
       
        public static void AddClassFromCode(InkCanvasStrokeCollectedEventArgs eventArgs)
        {
            Application.Current?.Dispatcher?.Invoke(new Action(() => { AddStroke(eventArgs); }), DispatcherPriority.ContextIdle);
        }

        internal static void OpenTutorial()
        {
            Application.Current?.Dispatcher?.Invoke(new Action(() => { GoToTutorial(); }), DispatcherPriority.Render);
        }

        internal static void LeaveDrawing()
        {
            Application.Current?.Dispatcher?.Invoke(new Action(() => { LeaveDrawingAction(); }), DispatcherPriority.Render);
        }

        internal static void GetHistoryLog(object o)
        {
            EditGalleryData editGalleryData = new EditGalleryData(username, canvasName);
            socket.Emit("getCanvasLogHistory", serializer.Serialize(editGalleryData));
        }

        internal static void UpdateCanvasName(string name)
        {
            Application.Current?.Dispatcher?.Invoke(new Action(() => { UpdateCanvasNameAction(name); }), DispatcherPriority.Render);
        }
    }
}
