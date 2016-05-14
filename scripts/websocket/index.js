var events = require('events');
var eventEmitter = new events.EventEmitter();
var Promise = require("promise");

module.exports = (url) => {

    return new Promise((resolve, reject) => {
        console.log("Connecting to " + url);
        try {
            var socket = new WebSocket("ws://" + url);
        } catch (error) {
            return reject(error);
        }

        socket.onmessage = (event) => {
            eventEmitter.emit('message', event.data);
        }
     
        socket.onerror = (event) => {
            eventEmitter.emit('error', event.data);
        }

        socket.onclose = (event) => {
            eventEmitter.emit('close', event.data);
        }

        socket.onopen = () => {
            eventEmitter.emit('connection-open', "Connection to " + url);
           
        };
        return resolve(eventEmitter);


    });

};