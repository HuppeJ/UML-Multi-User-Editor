import { ICanevas, IEditCanevasData, IEditGalleryData } from "./interfaces/interfaces";
import CanvasManager from "./components/CanvasManager";
import CanvasDataStoreManager from "./components/CanvasDataStoreManager";
import { EMPTY_THUMBNAIL } from "../../constants/thumbnail";

export const CanvasTestRoom: string = "Canvas_test_room";

export default class CanvasSocketEvents {
    constructor(io: any, canvasManager: CanvasManager, canvasDataStoreManager: CanvasDataStoreManager) {
        io.on("connection", function (socket: any) {
            console.log(socket.id + " connected to Canvas server");

            socket.on("getCanvasLogHistory", function (dataStr: string) {
                try {
                    const data: IEditGalleryData = JSON.parse(dataStr);
                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevasName);
                    socket.emit("getCanvasLogHistoryResponse", canvasManager.getCanvasLogHistorySERI(canvasRoomId));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });

            socket.on("resetServerState", function () {
                try {
                    const response = {
                        isStateReset: canvasManager.resetServerState()
                    };
    
                    socket.emit("resetServerStateResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });

            socket.on("createCanvas", function (dataStr: string) {
                try {
                    console.log(dataStr);
                    const data: IEditCanevasData = JSON.parse(dataStr);
                    console.log(data);

                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevas.name);
    

                    const updatedData: IEditCanevasData = data;
                    updatedData.canevas.thumbnail = EMPTY_THUMBNAIL;

                    const response = {
                        isCreated: canvasManager.addCanvasRoom(canvasRoomId, updatedData),
                        canvasName: data.canevas.name
                    };
    
                    if (response.isCreated) {
                        // (broadcast)
                        io.sockets.emit("canvasCreated", canvasManager.getCanvasRoomsSERI());
                        console.log(socket.id + " created  canvasRoom " + data.canevas.name);
                        canvasManager.logHistory(canvasRoomId, data.username, `created the canvas`);
                        canvasDataStoreManager.addCanvas(data.canevas, canvasManager.getCanvasLogHistory(canvasRoomId))
                    } else {
                        console.log(socket.id + " failed to create canvasRoom " + data.canevas.name);
                    }
    
                    socket.emit("createCanvasResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });


            // TODO à compléter voir qu'est-ce qu'on fait lorsqu'il y a des utilisateurs dans une room
            socket.on("removeCanvas", function (dataStr: string) {
                try {
                    console.log(dataStr);
                    const data: IEditGalleryData = JSON.parse(dataStr);
                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevasName);
    
                    const response = {
                        isCanvasRoomRemoved: canvasManager.removeCanvasRoom(canvasRoomId, data)
                    };
    
                    if (response.isCanvasRoomRemoved) {
                        // TODO on gère cela ici? 
                        // io.sockets.clients(someRoom).forEach(function(s){
                        //     s.leave(someRoom);
                        // });
    
                        // (broadcast)
                        io.sockets.emit("canvasRemoved", canvasManager.getCanvasRoomsSERI());
                        console.log(socket.id + " removed canvasRoom " + data.canevasName);
                    } else {
                        console.log(socket.id + " failed to remove canvasRoom " + data.canevasName);
                    }
    
                    socket.emit("removeCanvasResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });

            socket.on("joinCanvasRoom", function (dataStr: string) {
                try {
                    console.log(dataStr);
                    const data: IEditGalleryData = JSON.parse(dataStr);
                    console.log(data);

                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevasName);
    
                    const response = {
                        isCanvasRoomJoined: canvasManager.addUserToCanvasRoom(canvasRoomId, data),
                        canvasName: data.canevasName
                    };

                    console.log(JSON.stringify(response));

    
                    if (response.isCanvasRoomJoined) {
                        socket.join(canvasRoomId);
                        console.log(socket.id + " joined canvasRoom " + data.canevasName);
                    } else {
                        console.log(socket.id + " failed to join canvasRoom " + data.canevasName);
                    }
    
                    socket.emit("joinCanvasRoomResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error joinCanvas]: ", e);
                }
            });

            socket.on("leaveCanvasRoom", function (dataStr: string) {
                try {
                    console.log("leaveCanvasRoom", dataStr);
                    const data: IEditGalleryData = JSON.parse(dataStr);
                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevasName);
    
                    const response = {
                        isCanvasRoomLeaved: canvasManager.removeUserFromCanvasRoom(canvasRoomId, data)
                    };
    
                    if (response.isCanvasRoomLeaved) {
                        socket.leave(canvasRoomId);
                        const canvas: ICanevas = canvasManager.getCanvas(canvasRoomId);
                        canvasDataStoreManager.updateCanvas(canvas, canvasManager.getCanvasLogHistory(canvasRoomId))
                        console.log(socket.id + " leaved canvasRoom " + data.canevasName);
                    } else {
                        console.log(socket.id + " failed to leave canvasRoom " + data.canevasName);
                    }
    
                    socket.emit("leaveCanvasRoomResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });

            socket.on("saveCanvas", function (dataStr: string) {
                try {
                    console.log("onsaveCanvas")

                    const data: IEditCanevasData = JSON.parse(dataStr);
                    const canvasRoomId: string = canvasManager.getCanvasRoomIdFromName(data.canevas.name);

                    const response = {
                        isCanvasSaved: canvasManager.saveCanvas(canvasRoomId, data)
                    };

                    if (response.isCanvasSaved) {
                        console.log(socket.id + " saved canvas " + data.canevas.name);
                        io.emit("canvasSaved", dataStr);
                    } else {
                        console.log(socket.id + " failed to save canvas " + data.canevas.name);
                    }

                    socket.emit("saveCanvasResponse", JSON.stringify(response));
                } catch (e) {
                    console.log("[Error]: ", e);
                }
            });

            // [**************************************************
            // Temporary for testing

            socket.on("joinCanvasTest", function () {
                socket.join(CanvasTestRoom);
                console.log(`joinCanvasTest`, CanvasTestRoom);
                io.to(CanvasTestRoom).emit("joinCanvasTestResponse", "joinedCanvasTestRoom");
            });

            socket.on("canvasUpdateTest", function (CanvasFormChanges: any) {
                console.log(`CanvasUpdateTest from ${socket.id}, response:`, CanvasFormChanges);
                io.to(CanvasTestRoom).emit("canvasUpdateTestResponse", CanvasFormChanges);
            });
            // ****************************************************************]

        });
    }
};