using UnityEngine;

public class Foller : MonoBehaviour
{
    public Player player;
    public CharacterController characterController;

    public float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.transform);

		//      if (Input.GetButton("Horizontal"))
		//      {
		//          transform.position += new Vector3(speed * Input.GetAxis("Horizontal"), 0.0F, 0.0F);
		//      }

		//if (Input.GetButton("Vertical"))
		//{
		//	transform.position += new Vector3(0.0F, 0.0F, speed * Input.GetAxis("Vertical"));
		//}

		//transform.position += new Vector3(speed * Time.deltaTime * Input.GetAxis("Horizontal"), 0.0F, speed * Time.deltaTime * Input.GetAxis("Vertical"));

        characterController.Move(new Vector3(speed * Time.deltaTime * Input.GetAxis("Horizontal"), 0.0F, speed * Time.deltaTime * Input.GetAxis("Vertical")));


	}
}
