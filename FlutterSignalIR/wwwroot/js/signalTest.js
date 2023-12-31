
//$(function () {
"use strict";


var clientValue = [];
var sayac = 0;
var iplist = [];
var currentIpAdressInClientsIds = [];
var clientIds = "";

//var connection = new signalR.HubConnectionBuilder().withUrl("/testHub").build();

// Bu yapı test edilecektir
var connection = new signalR.HubConnectionBuilder().withUrl("/testHub", {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets
}).build();



connection.on("Mesaj", function (signalIRDataModel) {

    connection.invoke("UyariVer", clientIds, cookieID, connection.connection.connectionId).catch(function (err) {
        return console.error("invoke():", err.toString());
    });

});


connection.on("UserConnected", function (id) {
    return id;

});


connection.on("UyariMesaji", function (mesaj) {
    return mesaj;
   
});



connection.start().then(function () {
    console.log("Sistem dinleniliyor...");
}).catch(function (err) {
    return console.error("start():", err.toString());
});


//});