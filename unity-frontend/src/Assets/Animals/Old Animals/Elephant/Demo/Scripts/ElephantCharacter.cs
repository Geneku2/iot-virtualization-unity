using UnityEngine;
using System.Collections;
using System;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using System.Security.Cryptography;

// TODO: Needs further inspection/analysis before refactoring
public class ElephantCharacter : MonoBehaviour {
	public Animator elephantAnimator;
	public bool jumpStart=false;
	public float groundCheckDistance = 0.6f;
	public float groundCheckOffset=0.01f;
	public bool isGrounded=true;
	public float jumpSpeed=1f;
	Rigidbody elephantRigid;
	public float forwardSpeed = 1f;
    public float turnSpeed = 0f;
    public float animationSpeed = 0f;
	public float walkMode=1f;
	public float jumpStartTime=0f;
	public bool turnComplete = false;
    private float timer;
	private IEnumerator smoothSpeedCoroutine;
	
	void Start () {
		elephantAnimator = GetComponent<Animator> ();
		elephantAnimator.SetFloat("MovementSpeed", 0);
		elephantAnimator.SetFloat("Forward", 0);
		elephantAnimator.SetFloat("AnimationSpeed", 0);
		elephantRigid = GetComponent<Rigidbody>();
        //elephantAnimator.rootRotation.Set(1,0,0,0);

    }
	
	void FixedUpdate(){
		CheckGroundStatus ();
		//Move (forwardSpeed);
		jumpStartTime+=Time.deltaTime;
        //elephantAnimator.rootRotation.Set(1, 0, 0, 0);

    }
	
	public void Attack(){
		elephantAnimator.SetTrigger("Attack");
	}
	
	public void Hit(){
		elephantAnimator.SetTrigger("Hit");
	}
	
	public void Death(){
		elephantAnimator.SetBool("IsLived",false);
	}
	
	public void Rebirth(){
		elephantAnimator.SetBool("IsLived",true);
	}
	
	public void Eat()
    {
        elephantAnimator.SetTrigger("Eat");
		Stop();
	}

	//Added script, used to stop all motion and return elephant to idle state
	public void Stop()
	{
		elephantAnimator.SetFloat("Forward", 0f);
		animationSpeed = 0;
		elephantAnimator.SetFloat("AnimationSpeed", 0);
		elephantAnimator.SetFloat("Turn", 0);

		print("Stoppped at time: " + Time.time);
	}

	public void Test()
	{
		elephantAnimator.SetFloat("Forward", 0f);
		animationSpeed = 0;
		elephantAnimator.SetFloat("AnimationSpeed", 1);
		elephantAnimator.SetFloat("Turn", 1);
	}

	public void Trot(){
		walkMode = 2f;
	}
	
	public void Walk(){
		walkMode = 1f;
	}
	
	public void Jump(){
		if (isGrounded) {
			elephantAnimator.SetTrigger ("Jump");
			jumpStart = true;
			jumpStartTime=0f;
			isGrounded=false;
			elephantAnimator.SetBool("IsGrounded",false);
		}
	}
	
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		isGrounded = Physics.Raycast (transform.position + (transform.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);

		print(isGrounded);

		if (jumpStart) {
			if(jumpStartTime>.25f){
				jumpStart=false;
				elephantRigid.AddForce((transform.up+transform.forward*forwardSpeed)*jumpSpeed,ForceMode.Impulse);
				elephantAnimator.applyRootMotion = false;
				elephantAnimator.SetBool("IsGrounded",false);
			}
		}
		
		if (isGrounded && !jumpStart && jumpStartTime>.5f) {
			elephantAnimator.applyRootMotion = true;
			elephantAnimator.SetBool ("IsGrounded", true);
		} else {
			if(!jumpStart){
				elephantAnimator.applyRootMotion = false;
				elephantAnimator.SetBool ("IsGrounded", false);
			}
		}
	}
	
	//Careful! Use of this function will cause all speed smoothing to reset
	public void Move(float speed){

		print("Set MovementSpeed to " + speed);

		if (smoothSpeedCoroutine != null)
		{
			StopCoroutine(smoothSpeedCoroutine);
		}

		//TODO: Add constants instead of numbers after sufficient testing
		float originalSpeed = elephantAnimator.GetFloat("AnimationSpeed"); ;
		float forward = speed;

		//Would cause unexplained turning if forward was set below 0 and 1
		//TODO: Figure out why
		//TODO: Figure out better way to do movements
		forward = Mathf.Clamp(forward, 0, 2);

		//May cause problem later
		elephantAnimator.SetFloat("MovementSpeed", 1);
        elephantAnimator.SetFloat("Forward", forward);

		if (speed != originalSpeed)
		{
			//start smoothing
			float multiplier = (speed - animationSpeed) / Mathf.Abs(speed - animationSpeed);
			smoothSpeedCoroutine = smoothMoveSpeed(speed, multiplier);
			StartCoroutine(smoothSpeedCoroutine);
		}
	}

	//This is a rather risky way to smooth movement. If anything safer is thought of, this shall be replaced
	IEnumerator smoothMoveSpeed(float destinationSpeed, float multiplier)
	{
		for (; ; )
		{
			//Use constants to replace this after sufficient testing
			float deltaSpeedPerSecond = 0.35f;
			float timeRefSegment = 0.1f;

			if (approximately(animationSpeed, destinationSpeed, 0.2f))
			{
				if(destinationSpeed == 0)
				{
					Stop();
				}

				yield break;
			}

			print("Accelerating/Decelerating! Current Speed: " + animationSpeed + " DestinationSpeed: " + destinationSpeed);
			//Add or minus about deltaSpeedPerSecond
			//multiplier is either -1 or +1
			animationSpeed += deltaSpeedPerSecond / (1/timeRefSegment) * multiplier;

			//update speed
			elephantAnimator.SetFloat("AnimationSpeed", animationSpeed);

			yield return new WaitForSeconds(timeRefSegment);
		}
	}
	//Custom implemented file to testing float point affinity
	private bool approximately(float a, float b, float tolerance)
	{
		return (tolerance >= Mathf.Abs(a - b));
	}

	
    public void TurnForTime(float turnDirection, float time, float speed)
	{
		turnComplete = false;
		elephantAnimator.SetFloat("Turn", turnDirection * 0.1f);
		Move(speed);
		StartCoroutine(TurnWithTime(turnDirection, time, 0, 0));
	}

	//turnDirection only 1 or -1 (right or left)
	//Turn for degree, with a forward speed of speed
	public void TurnAndMove(float turnDirection, float degree, float speed) {
		turnComplete = false;
        elephantAnimator.SetFloat("Turn", turnDirection * 0.1f);
		Move(speed);
		StartCoroutine(TurnWithDegree(turnDirection, degree, transform.forward, 0));
	}

	IEnumerator TurnWithTime(float turnDirection, float time, float timeRef, float turnSpeedRef)
	{
		timeRef = Time.time;

		for (; ; )
		{
			if (Time.time - timeRef >= time)
			{
				elephantAnimator.SetFloat("Turn", 0);

				yield break;
			}

			//Smooth last turn
			if (Time.time - timeRef <= 1 && turnSpeedRef >= 0.2f)
			{
				turnSpeedRef += 0.8f * Time.deltaTime * (-turnDirection);
			}

			if (Math.Abs(turnSpeedRef) < 1)
			{
				turnSpeedRef += 0.4f * Time.deltaTime * turnDirection;
			}

			elephantAnimator.SetFloat("Turn", turnSpeedRef);

			if (elephantAnimator.GetFloat("Forward") == 0)
			{
				elephantAnimator.SetFloat("AnimationSpeed", turnSpeedRef);
			}

			yield return null;
		}
	}

	//Each frame update for turning motion
	IEnumerator TurnWithDegree(float turnDirection, float degree, Vector3 startPos, float turnSpeedRef)
	{
		float degreeDifference;

		for (; ; )
		{
			degreeDifference = degree - Vector3.Angle(startPos, transform.forward);

			//TODO: Use constants after sufficient testing
			if (degreeDifference <= 30)
			{
				
				if (turnSpeedRef <= 0.7f)
				{
					turnSpeedRef = 0.7f;
				}

				if (elephantAnimator.GetFloat("Forward") == 0)
				{
					elephantAnimator.SetFloat("AnimationSpeed", turnSpeedRef);
				}

				turnSpeed = degreeDifference / 30 * turnDirection * turnSpeedRef;

				elephantAnimator.SetFloat("Turn", turnSpeed);
			} else
			{
				if (Math.Abs(turnSpeed) < 1)
				{
					turnSpeedRef += 0.4f * Time.deltaTime * turnDirection;
				}

				if (elephantAnimator.GetFloat("Forward") == 0)
				{
					elephantAnimator.SetFloat("AnimationSpeed", turnSpeedRef);
				}

				elephantAnimator.SetFloat("Turn", turnSpeedRef);
			}

			if (degreeDifference <= 0.01f)
			{
				turnComplete = true;
				elephantAnimator.SetFloat("Turn", 0);
				yield break;
			}

			yield return new WaitForSeconds(0.02f);
		}
	}

}
