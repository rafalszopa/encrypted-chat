var readlineSync = require('readline-sync');
var colors = require('colors');
var net = require('net');

var HOST = "127.0.0.1";
var PORT = "9000";
var USERNAME = "rafals92";

var client = null;

function StartClient() {
  // Open a connection
  if(client) {
    console.log("Connection is already opened.".red);
    return false;
  }

  client = new net.Socket();

  client.on('data', function(data) {
    console.log("Received: %s".cyan, data);

    setTimeout(function() {
      EnterData();
    }, 0);
  });

  client.on('error', function(err) {
    client.destroy();
    client = null;
    console.log("ERROR: Connection could not be opened. Msg: %s".red, err.message);
  });

  try {
    client.connect(PORT, HOST, function() {
      console.log("Connection opened successfully.".green);
      setTimeout(function() {
        EnterData();
      }, 0);
    });
  } catch(err) {
    console.log("Error: %s".red, err.message);
  }

}

function EnterData() {
  var data = readlineSync.question("Enter message: ");

  if(data == "exit") {
    return;
  }
  console.log("Message has sent.");
  client.write(data);

  // setTimeout(function() {
  //   EnterData();
  // }, 0);
}

StartClient();

// (function() {
//
//   while (true) {
//
//   }
//
// })();

// function SendData(data) {
//
//   client.write(data);
//
//   // EnterData();
// }
function CloseConnection() {
  if(!client) {
    console.log("Connection is not opened or closed".red);
    setTimeout(function() {
      menu();
    }, 0);
    return;
  }

  client.destroy();
  client = null;

  console.log("Connection closed successfully.".green);
  setTimeout(function() {
    menu();
  }, 0);
}
