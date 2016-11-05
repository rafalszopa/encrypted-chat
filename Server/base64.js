var Base64 = function() {};

Base64.prototype.Encode = function(plainString) {
  return new Buffer(plainString).toString('base64');
};

Base64.prototype.Decode = function(base64String) {
  return new Buffer(base64String, 'base64').toString('ascii');
};

module.exports = new Base64();
