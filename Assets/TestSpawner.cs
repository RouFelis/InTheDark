using UnityEngine;
using Unity.Netcode;

public class TestSpawner : MonoBehaviour
{
    public GameObject TestObject;
    public Transform place;

    void Start()
    {
        NetworkObject test = Instantiate(TestObject , place.position , Quaternion.identity).GetComponent<NetworkObject>();
        test.Spawn();
    }





    // Update is called once per frame
    void Update()
    {
        
    }
}
