import { ICanevas, IEditCanevasData, IUpdateFormsData, IUpdateLinksData, IEditGalleryData, IResizeCanevasData, IHistoryData, ILink } from "../interfaces/interfaces";
import { mapToObj } from "../../../utils/mapToObj";

export interface ISelectedFormsValue {
    username: string,
    form: any,
}

export interface ISelectedLinksValue {
    username: string,
    link: ILink,
}

export const HISTORY_MAX_SIZE: number = 15;

export default class CanvasRoom {
    public connectedUsers: any;  // connectedUsers is a Set : [key: username]
    public selectedForms: any;  // selectedForms is a Map : [key: formId, value: username]
    public selectedLinks: any;  // selectedLinks is a Map : [key: linkId, value: username]
    public canvasSelected: boolean;  // selectedLinks is a Map : [key: formId, value: username]
    public history: IHistoryData[]; 
    public date: Date;

    constructor(public canvas: ICanevas) {
        this.connectedUsers = new Set();
        this.selectedForms = new Map<string, string>();
        this.selectedLinks = new Map<string, string>();
        this.canvasSelected = false;
        this.history = [];
        this.date = new Date();
    }

    public logHistory(username: string, message: string) {
        const newCanvas = Object.assign({}, this.canvas);
        this.date = new Date();
        const newLog: IHistoryData = {
            username: username,
            message: message,  
            timestamp: this.date.getTime().toString(),
            canevas: newCanvas
        }

        if (this.history.length < HISTORY_MAX_SIZE){
            this.history.push(newLog);
        } else {
            const newHistory = [];
            for (let i = 1; i < HISTORY_MAX_SIZE; i++) {
                newHistory.push(this.history[i]);
            }
            newHistory.push(newLog);
            this.history = newHistory;
        }

        return true;
    }

    public addUser(username: string) {
        if (!this.hasUser(username)) {
            this.connectedUsers.add(username);
        }
    }

    public removeUser(username: string) {
        if (this.hasUser(username)) {
            this.connectedUsers.delete(username);
        }
    }

    public hasUser(username: string) {
        return this.connectedUsers.has(username);
    }

    public getConnectedUsersSERI(): string {
        return JSON.stringify({
            connectedUsers: Array.from(this.connectedUsers)
        });
    }

    public isCanvasPublic() {
        return this.canvas.accessibility === 1; // Voir: enum AccessibilityTypes 
    }

    public isCanvasPrivate() {
        return this.canvas.accessibility === 0; // Voir: enum AccessibilityTypes 
    }

    public isAuthorOfCanvase(username: string) {
        return this.canvas.author === username;
    }

    public isPasswordValid(data: IEditGalleryData) {
        return this.canvas.password === data.password;
    }

    public updataCanvasPassword(data: IEditGalleryData) {
        this.canvas.password = data.password;
        return true;
    }
    
    public isCanvasSaved(data: IEditCanevasData) {
        if (data.canevas.thumbnail != "") {
            this.canvas.thumbnail = data.canevas.thumbnail;
            this.history[this.history.length - 1].canevas.thumbnail = data.canevas.thumbnail;
        }

        return true;
    }

    /***********************************************
    * Functions related to Forms
    ************************************************/
    public addForm(data: IUpdateFormsData): boolean {
        try {
            if (data.forms.length !== 1) {
                throw new Error(`You can only create one Form at a time.`);
            }

            this.canvas.shapes.push(data.forms[0]);
            return true;
        } catch (e) {
            console.log("[Error] in addForm", e);
            return false;
        }
    }

    public updateForms(data: IUpdateFormsData): boolean {
        try {
            let formIsUpdated: boolean = false;

            data.forms.forEach((form) => {
                formIsUpdated = false;
                this.canvas.shapes.forEach((shape, index) => {
                    if (shape.id === form.id) {
                        this.canvas.shapes[index] = form;
                        formIsUpdated = true;
                    }
                });

                if (!formIsUpdated) {
                    throw new Error(`There is no form with the id: "${form.id}" in the canvas : "${this.canvas.name}".`);
                }
            });

            return true;
        } catch (e) {
            console.log("[Error] in updateForms", e);
            return false;
        }
    }

    // Note : Il ne faut pas qu'il y ait de dupliqué dans les forms à delete
    public deleteForms(data: IUpdateFormsData): boolean {
        try {
            let formIsDeleted: boolean = false;

            data.forms.forEach((form) => {
                formIsDeleted = false;
                this.canvas.shapes = this.canvas.shapes.filter((shape) => {
                    if (shape.id === form.id) {
                        formIsDeleted = true;
                        return false;
                    }

                    return true;
                });

                if (!formIsDeleted) {
                    throw new Error(`There is no form with the id: "${form}" in the canvas : "${this.canvas.name}".`);
                }
            });

            // TODO : this.deselectForms(data);


            return true;
        } catch (e) {
            console.log("[Error] in deleteForms", e);
            return false;
        }
    }

    // Note : Il ne faut pas qu'il y ait de dupliqué dans les forms à selectionner
    public selectForms(data: IUpdateFormsData): boolean {
        try {
            // If one form doesn't exist an Error will be thrown
            this.doFormsExist(data);

      
            // Check if all forms are not selected, if a form is already selected throw an Error.
            data.forms.forEach((form) => {
                if (this.selectedForms.has(form.id)) {
                    throw new Error(`The form with the id: "${form.id}" is already selected in the canvas : "${this.canvas.name}".`);
                }
            });

            // If all forms are not selected, select them
            data.forms.forEach((form) => {
                this.selectedForms.set(form.id, {
                    username: data.username,
                    form: form
                } as ISelectedFormsValue);
            });


            return true;
        } catch (e) {
            console.log("[Error] in selectForms", e);
            return false;
        }
    }



    // Note : Il ne faut pas qu'il y ait de dupliqué dans les forms à selectionner
    public deselectForms(data: IUpdateFormsData): boolean {
        try {
            // If one form doesn't exist an Error will be thrown
            this.doFormsExist(data);

            // Deselect all forms
            data.forms.forEach((form) => {
                this.selectedForms.delete(form.id);
            });

            return true;
        } catch (e) {
            console.log("[Error] in deselectForms", e);
            return false;
        }
    }


    /***********************************************
    * Functions related to Links
    ************************************************/
    public addLink(data: IUpdateLinksData): boolean {
        try {
            if (data.links.length !== 1) {
                throw new Error(`You can only create one Link at a time.`);
            }
            // TODO : Check if linkTo and linkFrom point to existing forms?
            this.canvas.links.push(data.links[0]);
            return true;
        } catch (e) {
            console.log("[Error] in addLink", e);
            return false;
        }
    }

    public updateLinks(data: IUpdateLinksData): boolean {
        try {
            //TODO : Check if linkTo and linkFrom point to existing forms?
            let linkIsUpdated: boolean = false;
            data.links.forEach((newLink) => {
                linkIsUpdated = false;
                this.canvas.links.forEach((link, index) => {
                    if (newLink.id === link.id) {
                        this.canvas.links[index] = newLink;
                        linkIsUpdated = true;
                    }
                });

                if (!linkIsUpdated) {
                    throw new Error(`There is no form with the id: "${newLink.id}" in the canvas : "${this.canvas.name}".`);
                }
            });

            return true;
        } catch (e) {
            console.log("[Error] in updateForms", e);
            return false;
        }
    }

    // Note : Il ne faut pas qu'il y ait de dupliqué dans les forms à delete
    public deleteLinks(data: IUpdateLinksData): boolean {
        try {
            // TODO : Check if linkTo and linkFrom point to existing forms?
            let linkIsDeleted: boolean = false;
            data.links.forEach((newlink) => {
                linkIsDeleted = false;
                this.canvas.links = this.canvas.links.filter((link) => {
                    if (newlink.id === link.id) {
                        linkIsDeleted = true;
                        return false;
                    }

                    return true;
                });

                if (!linkIsDeleted) {
                    throw new Error(`There is no link with the id: "${newlink.id}" in the canvas : "${this.canvas.name}".`);
                }
            });

            // TODO : this.deselectLinks(data);


            return true;
        } catch (e) {
            console.log("[Error] in deleteLinks", e);
            return false;
        }
    }

    // Note : Il ne faut pas qu'il y ait de dupliqué dans les links à selectionner
    public selectLinks(data: IUpdateLinksData): boolean {
        try {
            // If one form doesn't exist an Error will be thrown
            this.doLinksExist(data);

            // Check if all links are not selected, if a form is already selected throw an Error.
            data.links.forEach((link) => {
                if (this.selectedLinks.has(link)) {
                    throw new Error(`The form with the id: "${link}" is already selected in the canvas : "${this.canvas.name}".`);
                }
            });

            // If all links are not selected, select them
            data.links.forEach((link) => {
                this.selectedLinks.set(link.id, {
                    username: data.username,
                    link: link
                } as ISelectedLinksValue);
            });

            return true;
        } catch (e) {
            console.log("[Error] in selectLinks", e);
            return false;
        }
    }



    // Note : Il ne faut pas qu'il y ait de dupliqué dans les links à selectionner
    public deselectLinks(data: IUpdateLinksData): boolean {
        try {
            // If one form doesn't exist an Error will be thrown
            this.doLinksExist(data);

            // Deselect all links
            data.links.forEach((link) => {
                this.selectedLinks.delete(link.id);
            });

            return true;
        } catch (e) {
            console.log("[Error] in selectLinks", e);
            return false;
        }
    }

    /***********************************************
    * Functions related to the Canvas
    ************************************************/
    public resize(data: IResizeCanevasData): boolean {
        try {
            this.canvas.dimensions = data.dimensions;
            return true;
        } catch (e) {
            console.log("[Error] in resize", e);
            return false;
        }
    }

    public reinitialize(data: IEditCanevasData): boolean {
        try {
            this.canvas.shapes = [];
            this.canvas.links = [];
            return true;
        } catch (e) {
            console.log("[Error] in reinitialize", e);
            return false;
        }
    }

    public selectCanvas(data: IEditGalleryData): boolean {
        try {
            if (this.canvasSelected) {
                return false;
            }

            this.canvasSelected = true;
            return true;
        } catch (e) {
            console.log("[Error] in selectCanvas", e);
            return false;
        }
    }

    public deselectCanvas(data: IEditGalleryData): boolean {
        try {
            if (this.canvasSelected) {
                this.canvasSelected = false;
                return true;
            }

            return false;
        } catch (e) {
            console.log("[Error] in selectCanvas", e);
            return false;
        }
    }

    /***********************************************
    * Utils functions 
    ************************************************/
    private doFormsExist(data: IUpdateFormsData): boolean {
        let formExist: boolean = false;

        data.forms.forEach((form) => {
            formExist = false;
            this.canvas.shapes.forEach((shape) => {
                if (shape.id === form.id) {
                    formExist = true;
                }
            });

            if (!formExist) {
                throw new Error(`There is no form with the id: "${form}" in the canvas : "${this.canvas.name}".`);
            }
        });

        return true;
    }

    private doLinksExist(data: IUpdateLinksData): boolean {
        let linkExist: boolean = false;

        data.links.forEach((link) => {
            linkExist = false;
            this.canvas.links.forEach((lien) => {
                if (lien.id === link.id) {
                    linkExist = true;
                }
            });

            if (!linkExist) {
                throw new Error(`There is no link with the id: "${link}" in the canvas : "${this.canvas.name}".`);
            }
        });

        return true;
    }

    // toJSON is automatically used by JSON.stringify
    toJSON() {
        return Object.assign({}, this, {
            // convert fields that need converting
            selectedForms: JSON.parse(this.getSelectedFormsSERI()),
            connectedUsers: Array.from(this.connectedUsers)
        });
    }

    public getSelectedFormsSERI(): string {
        return JSON.stringify({
            selectedForms: Array.from(this.selectedForms.keys())
        });
    }

    public getSelectedLinksSERI(): string {
        return JSON.stringify({
            selectedLinks: Array.from(this.selectedLinks.keys())
        });
    }

    public getSelectedFormsByUser(username: string): any[] {
        let selectedFormsArray: string[] = Array.from(this.selectedForms.keys());

        const formsSelectedByUser: any[] = [];

       selectedFormsArray.forEach((formId) => {
            const selectedFormValue: ISelectedFormsValue = this.selectedForms.get(formId);
            if (selectedFormValue.username == username) {
                formsSelectedByUser.push(selectedFormValue.form);
            }
        });

        return formsSelectedByUser;
    }

    public getSelectedLinksByUser(username: string): any[] {
        let selectedLinksArray: string[] = Array.from(this.selectedLinks.keys());

        const linksSelectedByUser: any[] = [];

       selectedLinksArray.forEach((linkId) => {
            const selectedLinkValue: ISelectedLinksValue = this.selectedLinks.get(linkId);
            if (selectedLinkValue.username == username) {
                linksSelectedByUser.push(selectedLinkValue.link);
            }
        });

        return linksSelectedByUser;
    }

    public getCanvasLogHistorySERI(): string {
        return JSON.stringify(this.history);
    }

    public getCanvasLogHistory(): IHistoryData[] {
        return this.history;
    }

    // // fromJSON is used to convert an serialized version
    // // of the User to an instance of the class
    // static fromJSON(json: ICanvasRoom | string): User {
    //     if (typeof json === 'string') {
    //         // if it's a string, parse it first
    //         return JSON.parse(json, User.reviver);
    //     } else {
    //         // create an instance of the User class
    //         let user = Object.create(User.prototype);
    //         // copy all the fields from the json object
    //         return Object.assign(user, json, {
    //             // convert fields that need converting
    //             created: new Date(json.created),
    //             account: Account.fromJSON(json.account)
    //         });
    //     }
    // }
}
