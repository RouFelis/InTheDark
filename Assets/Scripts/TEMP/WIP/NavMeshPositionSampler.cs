using UnityEngine;
using UnityEngine.AI;

public class NavMeshPositionSampler : MonoBehaviour
{
    private static NavMeshPositionSampler instance;

    public static NavMeshPositionSampler Instance => instance;

    static NavMeshPositionSampler()
    {
        instance = default;
    }

	private void Awake()
	{
		instance = this;
        DontDestroyOnLoad(gameObject);
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public bool SamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask)
    {
        return NavMesh.SamplePosition(sourcePosition, out hit, maxDistance, areaMask);
	}
}
