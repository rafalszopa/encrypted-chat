var diffieHellman = function() {};

diffieHellman.prototype.generatePrime = function() {
  return 23;
};

diffieHellman.prototype.generatePrimitiveRoot = function() {
  return 5;
};

diffieHellman.prototype.generateSecretNumber = function(min, max) {
  max = Math.floor(max);
  min = Math.ceil(min);
  return Math.floor(Math.random() * (max - min) + min);
};

diffieHellman.prototype.computeKey = function(A, a, p) {
  var key = A;

  if(a === 0)
    return 1 % p;

  for(var i = 0; i < a - 1; i++) {
    key = (key * A) % p;
  }

  return key;
};

diffieHellman.prototype.computeB = function(g, a, p) {

  var result = g;

  if(a === 0) {
    return 1 % p;
  }
  for(var i = 0; i < a - 1; i++) {
    result = (result * g) % p;
  }
  return result;
};

module.exports = new diffieHellman();
