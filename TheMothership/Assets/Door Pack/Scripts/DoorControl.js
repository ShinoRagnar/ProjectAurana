var door1 : GameObject;
var doorcontroller : int;
var up : float;
var down : float;



function Start() {

doorcontroller = 0;

up = door1.transform.position.y+3;
down = door1.transform.position.y;


}


function Update () {

if(doorcontroller == 1){

door1.transform.position = Vector3.Lerp(door1.transform.position, Vector3(door1.transform.position.x, up, door1.transform.position.z), Time.deltaTime*3);

}

if(doorcontroller == 0){

door1.transform.position = Vector3.Lerp(door1.transform.position, Vector3(door1.transform.position.x, down, door1.transform.position.z), Time.deltaTime*3);

}

}


function OnTriggerEnter (other : Collider) {

doorcontroller = 1;

}



function OnTriggerExit(other : Collider) {

doorcontroller = 0;

}