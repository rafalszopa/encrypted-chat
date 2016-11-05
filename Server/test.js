var array = ["a", "b", "c", "d", "e", "f"];
console.log(array);

var i = array.indexOf("d");

if(i != -1) {
  array.splice(i, 1);
}

console.log(array);
