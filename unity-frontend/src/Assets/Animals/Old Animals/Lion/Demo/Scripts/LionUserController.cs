using UnityEngine;
using System.Collections;
// TODO: Needs further inspection/analysis before refactoring
public class LionUserController : MonoBehaviour
{
    LionCharacter lionCharacter;
    float timer;
    private float[] real_speed = new float[16];

    void Start()
    {
        lionCharacter = GetComponent<LionCharacter>();
        initializeSpeedData();
    }

    void Update()
    {

        // don't fall
        /*if (transform.position.x >= 450 || transform.position.z >= 450 || transform.position.x <= 50 || transform.position.z <= 50)
        {
            transform.Rotate(0F, 180f, 0F);
        }*/
    }

    public void moveTo(float distance, float time)
    {
        float speed = distance / time;
        //float anim_speed = interpolation(speed);
        float anim_speed = fpsBasedSpeed(speed);
        lionCharacter.Move(anim_speed);
        //Debug.Log("wtf" + anim_speed);
        //StartCoroutine(delayedStop(time));
    }

    float interpolation(float speed)
    {
        //Debug.Log("Speed" + speed);
        int i = 0;
        bool speeding = true;
        while (i < 16)
        {
            if (real_speed[i] > speed)
            {
                speeding = false;
                break;
            }
            i++;
        }
        if (speeding)
        {
            return 1.5f + (speed - real_speed[14]) / (real_speed[14] - real_speed[13]) * 0.1f;
        }
        i -= 1;
        return i * 0.1f + (speed - real_speed[i]) / (real_speed[i + 1] - real_speed[i]) * 0.1f;
    }

    float fpsBasedSpeed(float speed)
    {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.86461 / 30 * fps);
    }

    void initializeSpeedData()
    {
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


/*
public class LionUserController : MonoBehaviour {
	LionCharacter lionCharacter;
	
	void Start () {
		lionCharacter = GetComponent < LionCharacter> ();
	}
	
	void Update () {	
		if (Input.GetButtonDown ("Fire1")) {
			lionCharacter.Attack();
		}
		if (Input.GetButtonDown ("Jump")) {
			lionCharacter.Jump();
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			lionCharacter.Hit();
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			lionCharacter.Death();
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			lionCharacter.Rebirth();
		}		
		if (Input.GetKeyDown (KeyCode.U)) {
			lionCharacter.StandUp();
		}		
		if (Input.GetKeyDown (KeyCode.J)) {
			lionCharacter.SitDown();
		}	
		if (Input.GetKeyDown (KeyCode.M)) {
			lionCharacter.LieDown();
		}		
		if (Input.GetKeyDown (KeyCode.N)) {
			lionCharacter.Sleep();
		}	
		if (Input.GetKeyDown (KeyCode.Y)) {
			lionCharacter.WakeUp();
		}	
		if (Input.GetKeyDown (KeyCode.R)) {
			lionCharacter.Roar();
		}		
		if (Input.GetKeyDown (KeyCode.G)) {
			lionCharacter.Gallop();
		}	
		if (Input.GetKeyDown (KeyCode.C)) {
			lionCharacter.Canter();
		}	
		if (Input.GetKeyDown (KeyCode.T)) {
			lionCharacter.Trot();
		}	
		if (Input.GetKeyUp (KeyCode.X)) {
			lionCharacter.Walk();
		}	

		lionCharacter.forwardSpeed=lionCharacter.maxWalkSpeed*Input.GetAxis ("Vertical");
		lionCharacter.turnSpeed= Input.GetAxis ("Horizontal");
	}

}
*/