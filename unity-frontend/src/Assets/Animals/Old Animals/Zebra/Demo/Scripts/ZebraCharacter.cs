using UnityEngine;
using System.Collections;

// TODO: Needs further inspection/analysis before refactoring
public class ZebraCharacter : MonoBehaviour {
	Animator zebraAnimator;
	public bool jumpStart=false;
	public float groundCheckDistance = 0.6f;
	public float groundCheckOffset=0.01f;
	public bool isGrounded=true;
	public float jumpSpeed=1f;
	Rigidbody zebraRigid;
	public float forwardSpeed;
	public float turnSpeed;
	public float walkMode=1f;
	public float jumpStartTime=0f;
	public float maxWalkSpeed=1f;
    private float animationSpeed;


    void Start () {
		zebraAnimator = GetComponent<Animator> ();
		zebraRigid=GetComponent<Rigidbody>();
        zebraAnimator.SetFloat("Forward", 1f);
        zebraAnimator.SetFloat("AnimationSpeed", 0f);
    }
	
	void FixedUpdate(){
		CheckGroundStatus ();
		jumpStartTime+=Time.deltaTime;
		maxWalkSpeed = Mathf.Lerp (maxWalkSpeed,walkMode,Time.deltaTime);
	}
	
	public void Attack(){
		zebraAnimator.SetTrigger("Attack");
	}
	
	public void Hit(){
		zebraAnimator.SetTrigger("Hit");
	}
	
	public void Death(){
		zebraAnimator.SetBool("IsLived",false);
	}
	
	public void Rebirth(){
		zebraAnimator.SetBool("IsLived",true);
	}
	
	public void EatStart()
    {
        zebraAnimator.SetFloat("Forward", 0f);
        zebraAnimator.SetFloat("AnimationSpeed", 1f);
        zebraAnimator.SetBool("IsEating",true);
	}
	public void EatEnd()
    {
        zebraAnimator.SetFloat("Forward", 1f);
        zebraAnimator.SetBool("IsEating",false);
	}
	
	
	public void SitDown(){
		zebraAnimator.SetTrigger("SitDown");
	}
	
	public void Sleep(){
		zebraAnimator.SetTrigger("Sleep");
	}
	
	public void StandUp(){
		zebraAnimator.SetTrigger("StandUp");
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
			zebraAnimator.SetTrigger ("Jump");
			jumpStart = true;
			jumpStartTime=0f;
			isGrounded=false;
			zebraAnimator.SetBool("IsGrounded",false);
		}
	}
	
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		isGrounded = Physics.Raycast (transform.position + (transform.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);
		
		if (jumpStart) {
			if(jumpStartTime>.1f){
				jumpStart=false;
				zebraRigid.AddForce((transform.up+transform.forward*forwardSpeed*.3f)*jumpSpeed,ForceMode.Impulse);
				zebraAnimator.applyRootMotion = false;
				zebraAnimator.SetBool("IsGrounded",false);
			}
		}
		
		if (isGrounded && !jumpStart && jumpStartTime>.5f) {
			zebraAnimator.applyRootMotion = true;
			zebraAnimator.SetBool ("IsGrounded", true);
		} else {
			if(!jumpStart){
				zebraAnimator.applyRootMotion = false;
				zebraAnimator.SetBool ("IsGrounded", false);
			}
		}
	}
	
	public void Move(float speed)
    {
		EatEnd();
        animationSpeed = speed;
        zebraAnimator.SetFloat("AnimationSpeed", speed);
    }

}
