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
      if(clients[i].soc !== sender && clients[i].isKeyExchanged == true) {
        clients[i].soc.write(message);
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


    var client = {
      name: "Mariusz",
      soc: socket,
      encryption: "none",
      isKeyExchanged: false,
      p: dh.generatePrime(),
      g: dh.generatePrimitiveRoot(),
      a: dh.generateSecretNumber(10000, 9999999),
      A: null,
      B: null,
      Key: null
    };

    client.B = dh.computeB(client.g, client.a, client.p);
    //ob = {"b" : client.B};
    //socket.write(JSON.stringify(ob));

    clients.push(client);

    socket.on('data', function(data) {

      var client = findClientBySocket(socket);
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

      // If there is a request
      if(jsonData.hasOwnProperty("request") && Object.keys(jsonData).length == 1) {
        if(jsonData["request"] === "keys") {

          ob = {"p" : client.p, "g": client.g};
          socket.write(JSON.stringify(ob));
        }

      // If there is a
      } else if(jsonData.hasOwnProperty("a") && Object.keys(jsonData).length === 1) {
        //console.log("Odebraliśmy a. Jego wartość to: %s".red, jsonData["a"]);

        client.A = jsonData["a"];
        client.Key = dh.computeKey(client.A, client.a, client.p);

        ob = {"b" : client.B};
        socket.write(JSON.stringify(ob));

        client.isKeyExchanged = true;

        console.log("Ustalony, tajny klucz: %s".red, client.Key);
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
      console.log("Name: " + client.name + ", Encryption: " + client.encryption + ", Secret number: " + client.a + ", Key: " + client.A);
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

function findClientBySocket(socket) {
  for(var i = 0; i < clients.length; i++) {
    if(clients[i].soc === socket) {
      return clients[i];
    }
  }
  return null;
}

StartServer();
