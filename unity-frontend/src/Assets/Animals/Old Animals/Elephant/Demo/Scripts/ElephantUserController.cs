using UnityEngine;
using System.Collections;

// TODO: Needs further inspection/analysis before refactoring
public class ElephantUserController : MonoBehaviour
{
    ElephantCharacter elephantCharacter;
    //float timer;
    //Vector3 initialPos;
    private bool startMove = false;
    private float[] real_speed = new float[16];
    private float startTime;

    void Start()
    {
        elephantCharacter = GetComponent<ElephantCharacter>();
        //timer = Random.value * 10.0F;
        //timer = 0;
        //initialPos = transform.position;
        initializeSpeedData();
        //elephantCharacter.Move(1f);

    }

    void Update()
    {
        /*
        timer += Time.deltaTime;
        elephantCharacter.Move(1f);
        if (!startMove) {
            if ((transform.position - initialPos).magnitude > 1f)
            {
                startMove = true;
                initialPos = transform.position;
            }
            timer = 0;
        }
        if (Mathf.Abs(timer - 1f) < Time.deltaTime / 2) {
            Debug.Log((transform.position - initialPos).magnitude / timer);
        }
        /*
        if (timer > 0)
        {
            elephantCharacter.Move(0.1f);
            //elephantCharacter.Turn();
        }
        else
        {   
            float direction = Random.value < 0.5f ? 1 : -1;
            elephantCharacter.Turn(direction, 1 );
            timer = Random.value * 10.0F;
            //elephantCharacter.forwardSpeed = 1f;
            //elephantCharacter.Turn();
            //transform.Rotate(0F, Random.value * 90.0F, 0F);
        }
        // don't fall
        /*if (transform.position.x >= 450 || transform.position.z >= 450 || transform.position.x <= 50 || transform.position.z <= 50)
        {
            transform.Rotate(0F, 180f, 0F);
        }*/
    }

    public void moveTo(float distance, float time)
    {
        //float angular_diff = transform.eulerAngles.y - yRotation;            
        //Debug.Log(time + " " + Time.deltaTime);
        float speed = distance / time;
        //float anim_speed = interpolation(speed);
        float anim_speed = fpsBasedSpeed(speed);


        elephantCharacter.Move(anim_speed);
            //Debug.Log("wtf" + anim_speed);
            //StartCoroutine(delayedStop(time));
    }
    /*
    IEnumerator delayedStop(float time)
    {
        yield return new WaitForSeconds(time);
        //elephantCharacter.Move(0f);
        //transform.position = new Vector3(destiny.x, transform.position.y, destiny.z);
    }
    */

    float fpsBasedSpeed(float speed) {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.76837 / 30 * fps);
    }

    float interpolation(float speed) {
        //Debug.Log("Speed" + speed);
        int i = 0;
        bool speeding = true;
        while (i < 16) {
            if (real_speed[i] > speed) {
                speeding = false;
                break;
            }
            i++;
        }
        if (speeding) {
            return 1.5f + (speed - real_speed[14]) / (real_speed[14] - real_speed[13]) * 0.1f;
        }
        i-=1;
        return i * 0.1f + (speed - real_speed[i]) / (real_speed[i+1] - real_speed[i]) * 0.1f;
    }

    void initializeSpeedData() {
        real_speed[0] = 0f;
        real_speed[1] = 0.064f;
        real_speed[2] = 0.286f;
        real_speed[3] = 0.533f;
        real_speed[4] = 0.760f;
        real_speed[5] = 1.013f;
        real_speed[6] = 1.239f;
        real_speed[7] = 1.484f;
        real_speed[8] = 1.720f;
        real_speed[9] = 1.961f;
        real_speed[10] = 2.186f;
        real_speed[11] = 2.415f;
        real_speed[12] = 2.670f;
        real_speed[13] = 2.907f;
        real_speed[14] = 3.201f;
        real_speed[15] = 3.384f;
    }
}


/*public class ElephantUserController : MonoBehaviour {
	ElephantCharacter elephantCharacter;
	
	void Start () {
		elephantCharacter = GetComponent <ElephantCharacter> ();
	}
	
	void Update () {	
		if (Input.GetButtonDown ("Fire1")) {
			elephantCharacter.Attack();
		}
		if (Input.GetButtonDown ("Jump")) {
			elephantCharacter.Jump();
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			elephantCharacter.Hit();
		}
		
		if (Input.GetKeyDown (KeyCode.K)) {
			elephantCharacter.Death();
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			elephantCharacter.Rebirth();
		}		
	
		if (Input.GetKeyDown (KeyCode.E)) {
			elephantCharacter.Eat();
		}		
		
		if (Input.GetKeyDown (KeyCode.T)) {
			elephantCharacter.Trot();
		}	
		if (Input.GetKeyUp (KeyCode.T)) {
			elephantCharacter.Walk();
		}	
		
		elephantCharacter.forwardSpeed=elephantCharacter.walkMode*Input.GetAxis ("Vertical");
		elephantCharacter.turnSpeed= Input.GetAxis ("Horizontal");
	}
}*/
