import CanvasRoom from "./CanvasRoom";
import { IEditCanevasData, IEditGalleryData, IUpdateFormsData, IUpdateLinksData, IResizeCanevasData, IHistoryData, ICanvasDataStore, ICanevas } from "../interfaces/interfaces";
import { CANVAS_ROOM_ID } from "../../../constants/RoomID";
import { mapToObj } from "../../../utils/mapToObj";
import CanvasDataStoreManager from "./CanvasDataStoreManager";

export default class CanvasManager {
    public canvasRooms: Map<string, CanvasRoom>; // [key: canvasRoomId, value: canvasRoom]

    constructor(private canvasDataStoreManager: CanvasDataStoreManager) {     
        this.canvasRooms = new Map<string, CanvasRoom>();
    }

    public async initializeCanvasManagerState() {
        const canvases: ICanvasDataStore[] = await this.canvasDataStoreManager.getAllCanvases();
        canvases.forEach((canvasDataStore: ICanvasDataStore) => {
            const canvasRoomId: string = this.getCanvasRoomIdFromName(canvasDataStore.canvasName);
            if (this.loadCanvasRoom(canvasRoomId, canvasDataStore.canvas)) {
                this.setHistory(canvasRoomId, canvasDataStore.canvasHistory)
            }
        });
    }

    public setHistory(canvasRoomId: string, history: IHistoryData[],) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.history = history;
    }

    public loadCanvasRoom(canvasRoomId: string, canvas: ICanevas): boolean {
        if (this.canvasRooms.has(canvasRoomId)) {
            return false;
        }

        const canvasRoom = new CanvasRoom(canvas);
        this.canvasRooms.set(canvasRoomId, canvasRoom);
        return true;
    }

    public getCanvasRoomIdFromName(canvasName: string): string {
        return `${CANVAS_ROOM_ID}_${canvasName}`;
    }

    public getNameFromCanvasRoomId(canvasRoomId: string): string {
        return this.canvasRooms.get(canvasRoomId).canvas.name;
    }

    public resetServerState(): boolean {
        this.canvasRooms.clear();
        return true;
    }

    public saveCanvas(canvasRoomId: string, data: IEditCanevasData): boolean {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.isCanvasSaved(data);

    }

    public logHistory(canvasRoomId: string, username: string, message: string): boolean {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.logHistory(username, message);
    }

    public getCanvasLogHistorySERI(canvasRoomId: string): string {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return "";
        }

        return canvasRoom.getCanvasLogHistorySERI();
    }

    public getCanvasLogHistory(canvasRoomId: string): IHistoryData[] {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return [];
        }

        return canvasRoom.getCanvasLogHistory();
    }

    public accessCanvas(canvasRoomId: string, data: IEditGalleryData): boolean {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.isPasswordValid(data);
    }

    public addCanvasRoom(canvasRoomId: string, data: IEditCanevasData) {
        if (this.canvasRooms.has(canvasRoomId)) {
            return false;
        }

        const canvasRoom = new CanvasRoom(data.canevas);
        this.canvasRooms.set(canvasRoomId, canvasRoom);
        return true;
    }

    public removeCanvasRoom(canvasRoomId: string, data: IEditGalleryData) {
        if (this.canvasRooms.has(canvasRoomId)) {
            this.canvasRooms.delete(canvasRoomId);
            return true;
        }

        return false;
    }

    public addUserToCanvasRoom(canvasRoomId: string, data: IEditGalleryData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (canvasRoom) {
            canvasRoom.addUser(data.username);
            return true;
        }

        return false;
    }

    public removeUserFromCanvasRoom(canvasRoomId: string, data: IEditGalleryData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (canvasRoom) {
            canvasRoom.removeUser(data.username);
            return true;
        }

        return false;
    }

    // TODO : Non utilisée et Fonction pas testée... 
    public isUserInCanvas(canvasRoomId: string, unsername: string) {
        if (this.canvasRooms.has(canvasRoomId)) {
            const canvasRoom = this.canvasRooms.get(canvasRoomId);
            return canvasRoom.hasUser(unsername);
        }

        return false;
    }

    public getCanvasRoomFromUsername(unsername: any): string {
        for (const [canvasRoomId, canvasRoom] of this.canvasRooms.entries()) {
            if (canvasRoom.hasUser(unsername)) {
                return canvasRoomId;
            }
        }

        return null;
    }

    /***********************************************
    * Functions related to Forms
    ************************************************/
    public addFormToCanvas(canvasRoomId: string, data: IUpdateFormsData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            console.log("canvasRoom doesn't exist:" + canvasRoomId);
            return false;
        }

        return canvasRoom.addForm(data);
    }

    public updateCanvasForms(canvasRoomId: string, data: IUpdateFormsData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.updateForms(data);
    }

    public deleteCanvasForms(canvasRoomId: string, data: IUpdateFormsData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.deleteForms(data);
    }

    public selectCanvasForms(canvasRoomId: string, data: IUpdateFormsData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.selectForms(data);
    }

    public deselectCanvasForms(canvasRoomId: string, data: IUpdateFormsData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.deselectForms(data);
    }


    /***********************************************
    * Functions related to Links
    ************************************************/
    public addLinkToCanvas(canvasRoomId: string, data: IUpdateLinksData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.addLink(data);
    }

    public updateCanvasLinks(canvasRoomId: string, data: IUpdateLinksData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.updateLinks(data);
    }

    public deleteCanvasLinks(canvasRoomId: string, data: IUpdateLinksData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.deleteLinks(data);
    }

    public selectCanvasLinks(canvasRoomId: string, data: IUpdateLinksData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.selectLinks(data);
    }

    public deselectCanvasLinks(canvasRoomId: string, data: IUpdateLinksData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.deselectLinks(data);
    }



    /***********************************************
    * Functions related to the Canvas
    ************************************************/
    public resizeCanvas(canvasRoomId: string, data: IResizeCanevasData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.resize(data);
    }

    public reinitializeCanvas(canvasRoomId: string, data: IEditCanevasData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.reinitialize(data);
    }

    public selectCanvas(canvasRoomId: string, data: IEditGalleryData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.selectCanvas(data);
    }

    public deselectCanvas(canvasRoomId: string, data: IEditGalleryData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.deselectCanvas(data);
    }

    public updataCanvasPassword(canvasRoomId: string, data: IEditGalleryData) {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return false;
        }

        return canvasRoom.updataCanvasPassword(data);
    }

    /***********************************************
    * Serialize / Deserialize
    ************************************************/
    public getSelectedFormsInCanvasRoomSERI(canvasRoomId: string, data: IEditGalleryData): string {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return null;
        }

        return canvasRoom.getSelectedFormsSERI();
    }

    public getSelectedLinksInCanvasRoomSERI(canvasRoomId: string, data: IEditGalleryData): string {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        if (!canvasRoom) {
            return null;
        }

        return canvasRoom.getSelectedLinksSERI();
    }

    public getCanvas(canvasRoomId: string): ICanevas {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        return canvasRoom.canvas;
    }


    public getCanvasSERI(canvasRoomId: string): string {
        const canvasRoom: CanvasRoom = this.canvasRooms.get(canvasRoomId);
        return JSON.stringify(canvasRoom.canvas);
    }

    public getCanvasRoomsSERI(): string {
        return JSON.stringify({
            canvasRooms: mapToObj(this.canvasRooms)
        });
    }

    public getPublicCanvasSERI(): string {
        const publicCanvasArray = [];

        for (const [canvasRoomId, canvasRoom] of this.canvasRooms.entries()) {
            if (canvasRoom.isCanvasPublic()) {
                publicCanvasArray.push(canvasRoom.canvas);
            }
        }

        return JSON.stringify({
            publicCanvas: publicCanvasArray
        });
    }

    public getPrivateCanvasSERI(username: string): string {
        const privateCanvasArray = [];

        for (const [canvasRoomId, canvasRoom] of this.canvasRooms.entries()) {
            if (canvasRoom.isCanvasPrivate() && canvasRoom.isAuthorOfCanvase(username)) {
                privateCanvasArray.push(canvasRoom.canvas);
            }
        }

        return JSON.stringify({
            privateCanvas: privateCanvasArray
        });
    }

    toJSON() {
        return Object.assign({}, this, {
            canvasRooms: mapToObj(this.canvasRooms)
        });
    }
}
