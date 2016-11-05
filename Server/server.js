var net = require('net');
var colors = require('colors');
var dh = require('./diffie-hellman');
var base64 = require('./base64');

var server = null;

var PORT = 9000;
clients = [];

console.log("Encrypted Chat Server v. 1.0".green);

function Broadcast(sender, message) {
  if(clients.length > 0) {
    for(var i = 0; i < clients.length; i++) {
      if(clients[i] !== sender) {
        clients[i].write(message);
      }
    }
  }
}

function Test(socket) {
  console.log(socket.remoteAddress);
  console.log("KSKDKD");
}

function StartServer() {

  try {
    server = net.createServer();
  } catch(err) {
    console.log("Error occured: ".red, err.message);
    return;
  }

  server.on('connection', function(socket) {

    var clientAddress = socket.remoteAddress + ":" + socket.remotePort;
    console.log("New user connected: %s".green, clientAddress);

    // Authenticate client. If OK add client to array.
    // Else: reject connection.


    clients.push(socket);

    socket.on('data', function(data) {

      // If received data are not in JSON format
      // Log the error and destroy socket
      try {
        var jsonData = JSON.parse(data);
      }
      catch(e) {
        console.log("Unrecognized data format. Error: %s".red, e.message);
        socket.destroy();
        return;
      }

      // If there is request
      if(jsonData.hasOwnProperty("request") && Object.keys(jsonData).length == 1) {
        if(jsonData["request"] === "keys") {
          ob = dh.generatePrimeAndPrimitiveRoot();
          socket.write(JSON.stringify(ob));
        }

      // If there is a
      } else if(jsonData.hasOwnProperty("a") && Object.keys(jsonData).length === 1) {

      // If there is encryption mode
      } else if(jsonData.hasOwnProperty("encryption") && Object.keys(jsonData).length === 1) {

      } else if(jsonData.hasOwnProperty("msg") && jsonData.hasOwnProperty("from") && Object.keys(jsonData).length === 2) {

      // Unrecognized keys are treat as a cheat
      } else {
        console.log("Unrecognized data format: %s".red, JSON.stringify(jsonData));
        socket.destroy();
        return;
      }


      console.log("[%s]: %s".yellow, clientAddress, data);
      Broadcast(socket, data);

      //var jsonData = JSON.parse(data);

      //if(jsonData.hasOwnProperty("request")) {
//        console.log(jsonData.request);
  //    }

      // var receivedJson = JSON.parse(data);
      // console.log(receivedJson);
      // if(receivedJson.hasOwnProperty("msg")) {
      //   console.log("Obiekt ma klucz: " + receivedJson.msg);
      // }

    });

    socket.on('error', function(err) {
      console.log("Error occured: %s".red, err.message);
    });

    socket.on('close', function() {
      console.log("User %s disconnected.".red, clientAddress);

      // Remove client socket from an array
      var i = clients.indexOf(socket);
      if(i != -1) {
        clients.splice(i, 1);
      }
    });

  });

  server.listen(PORT, function() {
    console.log("Server is listening on port %s".green, PORT);
  });

}


StartServer();
