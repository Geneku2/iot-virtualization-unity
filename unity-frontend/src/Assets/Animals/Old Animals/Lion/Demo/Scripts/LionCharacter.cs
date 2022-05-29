using UnityEngine;
using System.Collections;

// TODO: Needs further inspection/analysis before refactoring
public class LionCharacter : MonoBehaviour {
	Animator lionAnimator;
	public bool jumpStart=false;
	public float groundCheckDistance = 0.6f;
	public float groundCheckOffset=0.01f;
	public bool isGrounded=true;
	public float jumpSpeed=1f;
	Rigidbody lionRigid;
	public float forwardSpeed;
	public float turnSpeed;
	public float walkMode=1f;
	public float jumpStartTime=0f;
	public float maxWalkSpeed=1f;
    public float animationSpeed;

    void Start () {
		lionAnimator = GetComponent<Animator> ();
		lionRigid=GetComponent<Rigidbody>();
        lionAnimator.SetFloat("Forward", 1f);
        lionAnimator.SetFloat("AnimationSpeed", 0f);
    }
	
	void FixedUpdate(){
		CheckGroundStatus ();
		jumpStartTime+=Time.deltaTime;
		maxWalkSpeed = Mathf.Lerp (maxWalkSpeed,walkMode,Time.deltaTime);
	}
	
	public void Attack(){
		lionAnimator.SetTrigger("Attack");
	}
	
	public void Hit(){
		lionAnimator.SetTrigger("Hit");
	}
	
	public void Death(){
		lionAnimator.SetBool("IsLived",false);
	}
	
	public void Rebirth(){
		lionAnimator.SetBool("IsLived",true);
	}
	
	public void StandUp(){
		lionAnimator.SetTrigger("StandUp");
	}
	
	public void SitDown(){
		lionAnimator.SetTrigger("SitDown");
	}

	public void LieDown(){
		lionAnimator.SetTrigger("LieDown");
	}

	public void Sleep(){
		lionAnimator.SetTrigger("Sleep");
	}

	public void WakeUp(){
		lionAnimator.SetTrigger("WakeUp");
	}

	public void Roar(){
		lionAnimator.SetTrigger("Roar");
	}

    public void Eat()
    {
        lionAnimator.SetFloat("Forward", 0f);
        lionAnimator.SetFloat("AnimationSpeed", 1f);
        lionAnimator.SetTrigger("Eat");
    }

    public void Gallop(){
		walkMode = 4f;
	}

	public void Canter(){
		walkMode = 3f;
	}

	public void Trot(){
		walkMode = 2f;
	}

	public void Walk(){
		walkMode = 1f;
	}
	
	public void Jump(){
		if (isGrounded) {
			lionAnimator.SetTrigger ("Jump");
			jumpStart = true;
			jumpStartTime=0f;
			isGrounded=false;
			lionAnimator.SetBool("IsGrounded",false);
		}
	}
	
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		isGrounded = Physics.Raycast (transform.position + (transform.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);
		
		if (jumpStart) {
			if(jumpStartTime>.25f){
				jumpStart=false;
				lionRigid.AddForce((transform.up+transform.forward*forwardSpeed)*jumpSpeed,ForceMode.Impulse);
				lionAnimator.applyRootMotion = false;
				lionAnimator.SetBool("IsGrounded",false);
			}
		}
		
		if (isGrounded && !jumpStart && jumpStartTime>.5f) {
			lionAnimator.applyRootMotion = true;
			lionAnimator.SetBool ("IsGrounded", true);
		} else {
			if(!jumpStart){
				lionAnimator.applyRootMotion = false;
				lionAnimator.SetBool ("IsGrounded", false);
			}
		}
	}
	
	public void Move(float speed){
		//lionAnimator.SetFloat ("Forward", forwardSpeed);
		//lionAnimator.SetFloat ("Turn", turnSpeed);
		lionAnimator.SetFloat("Forward", 1f);
        animationSpeed = speed;
        lionAnimator.SetFloat("AnimationSpeed", speed);
    }
}
