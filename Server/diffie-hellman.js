var diffieHellman = function() {};

diffieHellman.prototype.generatePrimeAndPrimitiveRoot = function() {
  return { p: 23, g: 5 };
};

module.exports = new diffieHellman();
