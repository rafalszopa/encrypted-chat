var net = require('net');
var colors = require('colors');
var dh = require('./diffie-hellman');
var base64 = require('./base64');
var caesarShift = require('./caesarShift');
var xor = require('./xor');

var server = null;
var PORT = 9000;
clients = [];

console.log("Encrypted Chat Server v. 1.0".green);

function StartServer() {

  try {
    server = net.createServer();
  } catch(err) {
//    return;
  }

  server.on('connection', function(socket) {

    var clientAddress = socket.remoteAddress + ":" + socket.remotePort;
    console.log("New user connected: %s".green, clientAddress);

    var client = {
      soc: socket,
      encryption: "cezar",
      isKeyExchanged: false,
      p: dh.generatePrime(),
      g: dh.generatePrimitiveRoot(),
      a: dh.generateSecretNumber(10000, 9999999),
      A: null,
      B: null,
      Key: null
    };

    client.B = dh.computeB(client.g, client.a, client.p);
    clients.push(client);
    console.log("Liczba klientow: %s".blue, clients.length);

    socket.on('data', function(data) {

      var client = findClientBySocket(socket);

      // If received data are not in JSON format
      // Log the error and destroy socket
      try {
        var jsonData = JSON.parse(data);
      }
      catch(e) {
      //  console.log("Unrecognized data format. Error: %s".red, e.message);
        socket.destroy();
        return;
      }

      // If there is a request
      if(jsonData.hasOwnProperty("request") && Object.keys(jsonData).length == 1)
      {
        if(jsonData["request"] === "keys") {

          ob = {"p" : client.p, "g": client.g};
          socket.write(JSON.stringify(ob));
        }

      }
      else if(jsonData.hasOwnProperty("a") && Object.keys(jsonData).length === 1) {

        client.A = jsonData["a"];
        client.Key = dh.computeKey(client.A, client.a, client.p);

        ob = {"b" : client.B};
        socket.write(JSON.stringify(ob));

        client.isKeyExchanged = true;

      //  console.log("Ustalony, tajny klucz: %s".red, client.Key);

      }
      // Encryption mode
      else if(jsonData.hasOwnProperty("encryption") && Object.keys(jsonData).length === 1) {

        if(jsonData["encryption"] === "none")
          client.encryption = "none";
        if(jsonData["encryption"] === "cezar")
          client.encryption = "cezar";
        if(jsonData["encryption"] === "xor")
          client.encryption = "xor";
        else {
          Console.log("Encryption: %s was not recognized.".red, jsonData["encryption"]);
        }
      }
      // Message
      else if(jsonData.hasOwnProperty("msg") && jsonData.hasOwnProperty("from") && Object.keys(jsonData).length === 2) {

        if(!client.isKeyExchanged)
          return;

        console.log("Json: %s".red, jsonData["msg"]);

        var message = base64.Decode(jsonData["msg"]);
        var clientName = jsonData["from"];
        var decoded = "";

        if(client.encryption === "cezar")
          decoded = caesarShift.Code(message, -client.Key);
        else if(client.encryption === "xor")
          decoded = xor.Code(message, client.Key);

        console.log("Log from received(). Message: %s".cyan, decoded);

        Broadcast(socket, decoded, clientName);
      }
      // Unrecognized keys are treat as a cheat
      else {
  //      console.log("Unrecognized data format: %s".red, JSON.stringify(jsonData));
        socket.end();
        removeClientBySocket(socket);
        console.log("Liczba klientow: %s".blue, clients.length);
        return;
      }

//      console.log("[%s]: %s".yellow, clientAddress, data);
//      console.log("Encryption: " + client.encryption + ", Secret number: " + client.a + ", Key: " + client.A);
    });

    socket.on('error', function(err) {
      console.log("Error occured: %s".red, err.message);

      removeClientBySocket(socket);
      socket.end();
    });

    socket.on('close', function() {
      console.log("User %s disconnected.".red, clientAddress);

      // Remove client socket from an array

      removeClientBySocket(socket);
      socket.end();
      console.log("Liczba klientow: %s".blue, clients.length);
    });

  });

  server.listen(PORT, function() {
    console.log("Server is listening on port %s".green, PORT);
  });
}

function Broadcast(sender, message, senderName) {

//  console.log("Log from broadcast(). Message: %s".cyan, message);
  var encoded;
  // var toSend;

  if(clients.length > 0) {
    for(var i = 0; i < clients.length; i++) {
      if(clients[i].soc !== sender && clients[i].isKeyExchanged === true) {

        if(clients[i].encryption === "cezar")
          encoded = base64.Encode(caesarShift.Code(message, clients[i].Key));
        if(clients[i].encryption === "xor")
          { /*XOR Encode*/ }
        if(clients[i].encryption === "none")
          encoded = base64.Encode(message);

        //toSend = base64.Encode(decoded);

        console.log("Msg: %s".red, encoded);

        ob = {"msg": encoded, "from": senderName};

        clients[i].soc.write(JSON.stringify(ob));
      }
    }
  }
}

function findClientBySocket(socket) {
  for(var i = 0; i < clients.length; i++) {
    if(clients[i].soc === socket) {
      return clients[i];
    }
  }
  return null;
}

function removeClientBySocket(socket) {
  //var i = clients.indexOf(socket);

  for(var i = 0; i < clients.length; i++) {
    if(clients[i].soc === socket) {
      var j = i;
      break;
    }
  }

  if(j != -1) {
    clients.splice(j, 1);
  }
}

// function removeClientBySocket(socket) {
//   var i = clients.indexOf(socket);
//   if(i != -1) {
//     clients.splice(i, 1);
//   }
// }

StartServer();
