var door1 : GameObject;
var door2 : GameObject;
var doorcontroller : int;
var left : float;
var right : float;
var left2 : float;
var right2 : float;


function Start() {

doorcontroller = 0;

left = door1.transform.position.x-2;
right = door1.transform.position.x;
left2 = door2.transform.position.x+2;
right2 = door2.transform.position.x;

}


function Update () {

if(doorcontroller == 1){

door1.transform.position = Vector3.Lerp(door1.transform.position, Vector3(left, door1.transform.position.y, door1.transform.position.z), Time.deltaTime*3);
door2.transform.position = Vector3.Lerp(door2.transform.position, Vector3(left2, door2.transform.position.y, door2.transform.position.z), Time.deltaTime*3);

}

if(doorcontroller == 0){

door1.transform.position = Vector3.Lerp(door1.transform.position, Vector3(right, door1.transform.position.y, door1.transform.position.z), Time.deltaTime*3);
door2.transform.position = Vector3.Lerp(door2.transform.position, Vector3(right2, door2.transform.position.y, door2.transform.position.z), Time.deltaTime*3);

}

}


function OnTriggerEnter (other : Collider) {

doorcontroller = 1;

}



function OnTriggerExit(other : Collider) {

doorcontroller = 0;

}