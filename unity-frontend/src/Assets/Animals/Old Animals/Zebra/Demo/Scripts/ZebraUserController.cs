using UnityEngine;
using System.Collections;

// TODO: Needs further inspection/analysis before refactoring
public class ZebraUserController : MonoBehaviour
{
    ZebraCharacter zebraCharacter;
    float timer;

    void Start()
    {
        zebraCharacter = GetComponent<ZebraCharacter>();
        timer = Random.value * 10.0F;
    }

    void Update()
    {
    }

    public void moveTo(float distance, float time)
    {
        float speed = distance / time;
        float anim_speed = fpsBasedSpeed(speed);
        zebraCharacter.Move(anim_speed);
        //Debug.Log("wtf" + anim_speed);
        //StartCoroutine(delayedStop(time));
    }

    float fpsBasedSpeed(float speed)
    {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.936472 / 30 * fps);
    }
}

/*if (Input.GetButtonDown ("Fire1")) {
          zebraCharacter.Attack();
      }
      if (Input.GetButtonDown ("Jump")) {
          zebraCharacter.Jump();
      }
      if (Input.GetKeyDown (KeyCode.H)) {
          zebraCharacter.Hit();
      }
      if (Input.GetKeyDown (KeyCode.K)) {
          zebraCharacter.Death();
      }
      if (Input.GetKeyDown (KeyCode.L)) {
          zebraCharacter.Rebirth();
      }		

      if (Input.GetKeyDown (KeyCode.E)) {
          zebraCharacter.EatStart();
      }	
      if (Input.GetKeyUp (KeyCode.E)) {
          zebraCharacter.EatEnd();
      }	

      if (Input.GetKeyDown (KeyCode.J)) {
          zebraCharacter.SitDown();
      }	
      if (Input.GetKeyDown (KeyCode.N)) {
          zebraCharacter.Sleep();
      }	
      if (Input.GetKeyDown (KeyCode.U)) {
          zebraCharacter.StandUp();
      }	

      if (Input.GetKeyDown (KeyCode.G)) {
          zebraCharacter.Gallop();
      }	
      if (Input.GetKeyDown (KeyCode.C)) {
          zebraCharacter.Canter();
      }	
      if (Input.GetKeyDown (KeyCode.T)) {
          zebraCharacter.Trot();
      }	
      if (Input.GetKeyUp (KeyCode.X)) {
          zebraCharacter.Walk();
      }	*/

/*if ((!firstinit) || (tarPos == transform.position))
{
    firstinit = true;
    float randf = Random.value * 10.0F;
    nextPos = transform.position+Random.insideUnitSphere * randf;
    while (nextPos.x >= 450 || nextPos.z >= 450)
    {
        nextPos = new Vector3(Random.value * 400, 30, Random.value * 400);
    }
    float terrainHeight = Terrain.activeTerrain.SampleHeight(nextPos) + 5;
    Debug.Log("HEIGHT:"+terrainHeight);
    tarPos = new Vector3(nextPos.x, terrainHeight, nextPos.z);
    //tarPos = new Vector3(nextPos.x, 100, nextPos.z);


}*/
