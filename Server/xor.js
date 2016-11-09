var Xor = function() {};

Xor.prototype.Code = function(input, key) {

  var buffer = new Buffer(input, 'ascii');

  console.log("Raw input: %s", input);
  console.log("Char input:");

  // for(var i = 0; i < input.length; i++)
  //   console.log(buffer[i] + ", ");

  if(isLittleEndian)
    var pad = key.toString()[0];
  else
    var pad = key.toString()[a.toString().length - 1];

  pad = new Buffer(pad, "ascii");

  for(var i = 0; i < input.length; i++)
    buffer[i] = (buffer[i] ^ pad[0]);

  return buffer.toString('ascii');

}

Xor.prototype.isLittleEndian = function() {
    var a = new ArrayBuffer(4);
    var b = new Uint8Array(a);
    var c = new Uint32Array(a);
    b[0] = 0xa1;
    b[1] = 0xb2;
    b[2] = 0xc3;
    b[3] = 0xd4;
    if(c[0] == 0xd4c3b2a1) return true;
    if(c[0] == 0xa1b2c3d4) return false;
    else throw new Error("Some problem occured");
}

module.exports = new Xor();
