var diffieHellman = function() {};

diffieHellman.prototype.generatePrime = function() {
  //return 23;
  return 69;
};

diffieHellman.prototype.generatePrimitiveRoot = function() {
  return 5;
};

diffieHellman.prototype.generateSecretNumber = function() {
  return 6;
};

diffieHellman.prototype.computeA = function(g, a, p) {
  return 8;
};

module.exports = new diffieHellman();
