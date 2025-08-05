using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using DunGen;
using System.Collections.Generic;

public class SceneObjectManager : NetworkBehaviour
{
    private BoxCollider region;

    private void Awake()
    {
        region = GetComponent<BoxCollider>();
        region.isTrigger = true;
    }

	void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

   
    //�� ��ε� �� ������Ʈ ����.
    private void OnSceneUnloaded(Scene unloadedScene)
    {
        if (!IsServer)
            return;

        Vector3 center = region.transform.TransformPoint(region.center);
        Vector3 halfSize = region.size * 0.5f;
        Quaternion rotation = region.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, halfSize, rotation, ~0);  // ��� ���̾�

        // ���� ����
        var dungeon = FindAnyObjectByType<Dungeon>();
        if (dungeon != null)
            Destroy(dungeon.gameObject);

        foreach (var col in hits)
        {
			var go = col.gameObject;

            // �ڱ� �ڽ� �Ǵ� Tag�� Manager�� ��� �ǳʶ�
            if (go == this.gameObject || go.CompareTag("Manager"))
                continue;

            var netObj = go.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                if (NetworkManager.Singleton.IsServer)
                    netObj.Despawn();
            }
            else
            {
                Destroy(go);
            }
        }
    }

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		var b = GetComponent<BoxCollider>().bounds;
		Gizmos.color = new Color(1, 0, 0, 0.25f);
		Gizmos.DrawCube(b.center, b.size);
	}
#endif
}
