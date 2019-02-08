// Based on https://github.com/justadudewhohacks/websocket-chat

const Chatroom = require("./Chatroom");

module.exports = function () {
    // mapping of all available chatrooms 
    // chatrooms est une Map : [key: chatroom.name, value: Chatroom]
    const chatrooms = new Map();
    // TODO : Arranger le type chatroom pour avoir l'historique des messages et les sockets id

    function addChatroom(chatroomName, socketId) {
        if(!isChatroom(chatroomName)) {
            let chatroom = Chatroom();
            chatroom.addUser(socketId);
            chatrooms.set(chatroomName, chatroom);
            return true;
        }
        return false;
    }

    function isChatroom(chatroomName) {
        return chatrooms.has(chatroomName);
    }

    function removeChatroom(chatroomName) {
        if(isChatroom(chatroomName)) {
            chatrooms.delete(chatroomName);
        }
    }

    function addClientToChatroom(chatroomName, socketId) {
        if (isChatroom(chatroomName)){
            let chatroom = chatrooms.get(chatroomName);
            if (!chatroom.hasUser(socketId)) {
                chatroom.addUser(socketId);
                return true;
            }
        }
        return false;
    }

    function isClientInChatroom(chatroomName, socketId) {
        if (isChatroom(chatroomName)){
            let chatroom = chatrooms.get(chatroomName);
            return chatroom.hasUser(socketId);
        }
        return false;
    }

    function removeClientFromChatroom(chatroomName, socketId) {
        if(isClientInChatroom(chatroomName, socketId)) {
            let chatroom = chatrooms.get(chatroomName);
            chatroom.removeUser(socketId);
            return true;
        }
        return false;
    }

    function getChatrooms() {
        let strKeys = JSON.stringify(Array.from(chatrooms.keys()));
        return strKeys;
    }

    return {
        addChatroom,
        isChatroom,
        removeChatroom,
        addClientToChatroom,
        isClientInChatroom,
        removeClientFromChatroom,
        getChatrooms
    }
}
