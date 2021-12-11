using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class controllerDr : MonoBehaviourPunCallbacks
{
    internal enum driveType{
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField]private driveType drive;

    internal enum gearBox{
        automatic,
        manual
    }
    [SerializeField]private gearBox gearChange;

    [Header("TO be changed Later")]
    public float maxVelocity;
    public AudioSource audiosource;
    //other classes ->
    //public GameManager manager;

    //[Header("lighgts")]
    //public GameObject Lights;

    [Header("Variables")]
    public float handBrakeFrictionMultiplier = 2f;
    public float totalPower;
    public float brakeForce;
    public float maxRPM , minRPM;
    public float KPH;
    public float wheelsRPM;
    public float engineRPM;
    public float[] gears;
    public float[] gearChangeSpeed;
    public int gearNum = 0;
    public bool reverse = false;
    public AnimationCurve enginePower;

    //private inputManager IM;
    public WheelCollider[] wheels;
    public GameObject[] wheelMesh;
    public GameObject centerOfMass;
    private Rigidbody rigidbody;
    public float vertical;
    public float horizontal;
    public float brake;
    public bool handbrake;

    //car Shop Values
    public int carPrice ;
    public string carName;
    public int upgradeLevel = 0 ; //added upgrade level
    

    //hard coded values -

	private WheelFrictionCurve  forwardFriction,sidewaysFriction;
    private float thrust = -20000 , radius = 6, brakPower = 50000 , DownForceValue = 100f,smoothTime=0.09f , throttle ;// added throttle value

    [Header("DEBUG")]
    public float[] slip = new float[4];


    private void Awake() {

        upgradeLevel = PlayerPrefs.GetInt(carName + "upgrade");

        //check for upgrade -
        switch (upgradeLevel){
            case 0 : throttle = 0;
            break;
            case 1 : throttle = 0.1f;
            break;
            case 2 : throttle = 0.2f;
            break;
            case 3 : throttle = 0.3f;
            break;
        }


        if(SceneManager.GetActiveScene().name == "awakeScene")return;

        StartCoroutine(timedLoop());

    }
    private void Start()
    {

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void FixedUpdate() {

       // if(SceneManager.GetActiveScene().name == "awakeScene")return;
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        brake = Input.GetAxis("Jump");
        handbrake = Input.GetKey(KeyCode.LeftShift);
        addDownForce();
        animateWheels();
        steerVehicle();
        calculateEnginePower();
        shifter();
        adjustTraction();
        audiosource.pitch = (rigidbody.velocity.magnitude / maxVelocity)*2.8f;
    }

    private void calculateEnginePower(){
        wheelRPM();

        totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * (vertical + throttle);//add upgraded throttle
        float velocity  = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM,(1000+Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity , smoothTime);
        if(engineRPM > maxRPM + 500) engineRPM = maxRPM +  500 ;
        moveVehicle();

    }

    private void wheelRPM(){
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
 
        if(wheelsRPM < 0 && !reverse ){
            reverse = true;
            //manager.changeGear();
        }
        else if(wheelsRPM > 0 && reverse){
            reverse = false;
            //manager.changeGear();
        }
    }

    private bool checkGears(){
        if(KPH >= gearChangeSpeed[gearNum] ) return true;
        else return false;
    }

    private void shifter(){

        if(!isGrounded())return;
            //automatic
        if(gearChange == gearBox.automatic){
            if(engineRPM > maxRPM && gearNum < gears.Length-1 && !reverse && checkGears() ){
                gearNum ++;
                SoundManager.PlaySound(Sound.gearchange, transform.position);
                //manager.changeGear();
                return;
            }
            if (engineRPM < minRPM && gearNum > 0)
            {
                gearNum--;
                SoundManager.PlaySound(Sound.gearchange, transform.position);
                //manager.changeGear();
            }
        }
            //manual
        //else{
        //    if(Input.GetKeyDown(KeyCode.E)){
        //        gearNum ++;
        //        //manager.changeGear();
        //    }
        //}
        

    }
 
    private bool isGrounded(){
        if(wheels[0].isGrounded &&wheels[1].isGrounded &&wheels[2].isGrounded &&wheels[3].isGrounded )
            return true;
        else
            return false;
    }

    private void moveVehicle(){

        //  if(IM.boosting){
        //rigidbody.AddForce(transform.forward * 15000);

        //      //totalPower += 2000f;
        //  }
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].brakeTorque = brakeForce*brake;
        }

        if (drive == driveType.allWheelDrive){
            for (int i = 0; i < wheels.Length; i++){
                wheels[i].motorTorque = totalPower / 4;
            }
        }else if(drive == driveType.rearWheelDrive){
            for (int i = 2; i < wheels.Length; i++){
                wheels[i].motorTorque = (totalPower / 2);
            }
        }
        else{
            for (int i = 0 ; i < wheels.Length - 2; i++){
                wheels[i].motorTorque =  (totalPower / 2);
            }  
        }

        KPH = rigidbody.velocity.magnitude * 3.6f;

        //if(vertical <0 || KPH <= 1) Lights.SetActive(true); else Lights.SetActive(false);

    }

    private void steerVehicle(){


        //acerman steering formula
		//steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
        
        if (horizontal > 0 ) {
				//rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontal;
        } else if (horizontal < 0 ) {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontal;
			//transform.Rotate(Vector3.up * steerHelping);

        } else {
            wheels[0].steerAngle =0;
            wheels[1].steerAngle =0;
        }

    }

    private void animateWheels ()
	{
		Vector3 wheelPosition = Vector3.zero;
		Quaternion wheelRotation = Quaternion.identity;

		for (int i = 0; i < 4; i++) {
			wheels [i].GetWorldPose (out wheelPosition, out wheelRotation);
			wheelMesh [i].transform.position = wheelPosition;
			wheelMesh [i].transform.rotation = wheelRotation;
		}
	}

    private void addDownForce(){

        rigidbody.AddForce(-transform.up * DownForceValue * rigidbody.velocity.magnitude );

    }

    private float driftFactor;

    private void adjustTraction(){
            //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

        if (handbrake )
        {
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            sidewaysFriction.extremumValue =sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue,driftFactor * handBrakeFrictionMultiplier,ref velocity ,driftSmothFactor );

            for (int i = 0; i < 4; i++) {
                wheels [i].sidewaysFriction = sidewaysFriction;
                wheels [i].forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =  1.1f;
                //extra grip for the front wheels
            for (int i = 0; i < 2; i++) {
                wheels [i].sidewaysFriction = sidewaysFriction;
                wheels [i].forwardFriction = forwardFriction;
            }
            //rigidbody.AddForce(transform.forward * (KPH / 400) * 40000 );
            
		}
            //executed when handbrake is being held
        else{

			forwardFriction = wheels[0].forwardFriction;
			sidewaysFriction = wheels[0].sidewaysFriction;

			forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

			for (int i = 0; i < 4; i++) {
				wheels [i].forwardFriction = forwardFriction;
				wheels [i].sidewaysFriction = sidewaysFriction;

			}
        }

            //checks the amount of slip to control the drift
		for(int i = 2;i<4 ;i++){

            WheelHit wheelHit;

            wheels[i].GetGroundHit(out wheelHit);            

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -horizontal) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + horizontal )* Mathf.Abs(wheelHit.sidewaysSlip );
		}	
		
	}

	private IEnumerator timedLoop(){
		while(true){
			yield return new WaitForSeconds(.7f);
            radius = 6 + KPH / 20;
            
		}
	}


}
