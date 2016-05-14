require("./style.css");
window.identityService = (() => {

    var $ = require("jquery");
    var ui = require("./gauge.js");
    var socket = require("./websocket");
    var template = require("./template.hbs");
    var loader = require("./loader.html");
    var container = "#identity-container";
    var faces_container = "#faces-container";

    var gauger = (value, id) => {
        var currentValue = value * 100;

        var opts = {
            lines: 12, // The number of lines to draw
            angle: 0, // The length of each line
            lineWidth: 0.2, // The line thickness
            pointer: {
                length: 0.9, // The radius of the inner circle
                strokeWidth: 0.035, // The rotation offset
                color: '#000000' // Fill color
            },
            limitMax: 'true',   // If true, the pointer will not go past the end of the gauge
            colorStart: '#3CE78B',   // Colors
            colorStop: '#3CE78B',    // just experiment with them
            strokeColor: '#E0E0E0',   // to see which ones work best for you
            generateGradient: true,
            percentColors: [[0.0, "#099A4D"], [0.50, "#FF6B5C"], [1.0, "#099A4D"]],
        };

        var target = document.querySelector(id); // your canvas element
        var gauge = new ui.Gauge(target);
        gauge.setOptions(opts); // create sexy gauge!
        gauge.maxValue = 100; // set max gauge value
        gauge.animationSpeed = 32; // set animation speed (32 is default value)
        gauge.set(currentValue); // set actual value
    };
    console.log(ui);

    var exports = {};
    exports.init = (id, server) => {

        $(document).ready(function () {
            $(id).append('<div id="identity-container"></div>');
            $(container).append(loader);
        });
        var q = async.queue(function (task, callback) {
            $("#face-loader").fadeOut(500);
            var model = JSON.parse(data);
            model = model.map((face) => {
                face.FaceAttributes.Smile = face.FaceAttributes.Smile * 100;
                face.FaceAttributes.Glasses = face.FaceAttributes.Glasses !== "NoGlasses";
                face.FaceAttributes.Age = Math.round(parseFloat(face.FaceAttributes.Age));
                return face;
            });

            console.log(model);

            var rendered = template(model);
            $(container).append(rendered);
            setTimeout(() =>
                model.forEach((identity => {
                    var beardId = "#" + identity.Identity.UserName + "-beard";
                    var moustacheId = "#" + identity.Identity.UserName + "-moustache";
                    var burnsId = "#" + identity.Identity.UserName + "-burns";
                    var ageId = "#" + identity.Identity.UserName + "-age";
                    gauger(Math.round(identity.FaceAttributes.Age), ageId);
                    gauger(identity.FaceAttributes.FacialHair.Beard, beardId);
                    gauger(identity.FaceAttributes.FacialHair.Moustache, moustacheId);
                    gauger(identity.FaceAttributes.FacialHair.Sideburns, burnsId);
                }))
            , 500);

            setTimeout(() => {
                $(faces_container).fadeIn(500);
                $(faces_container).remove();
                callback();
            }, 20000);
        }, 1);

        socket(server).then(events => {
            events.on('connection-open', data => { console.log(data) });

            events.on('message', data => {
                

            });

        });
    };
    return exports;
})();