using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using Unity.Collections;


public class Quest5 : QuestBase
{
    [Header("UI")]
    [SerializeField] private TMP_Text displayText;

    [Header("USB")]
    [SerializeField] private Transform USBSpawnTransfrom;
    [SerializeField] private GameObject USBPrefab;

    [Header("Screen Settings")]
    private const int MaxDisplayLines = 11;

    private List<string> history = new List<string>(); // 서버만 보관

    private List<string> hackingEffectLines = new List<string>
{
    "> Initiating purge protocol...",
    "> [▓░░░░░░░░░░] 10% - Allocating memory...",
    "> [▓▓░░░░░░░░░] 20% - Access granted...",
    "> [▓▓▓░░░░░░░░] 30% - Unpacking scripts...",
    "> [▓▓▓▓░░░░░░░] 40% - Injecting commands...",
    "> [▓▓▓▓▓░░░░░░] 50% - Killing processes...",
    "> [▓▓▓▓▓▓░░░░░] 60% - Deleting binaries...",
    "> [▓▓▓▓▓▓▓░░░░] 70% - Removing traces...",
    "> [▓▓▓▓▓▓▓▓░░░] 80% - Overwriting headers...",
    "> [▓▓▓▓▓▓▓▓▓░░] 90% - Finalizing wipe...",
    "> [▓▓▓▓▓▓▓▓▓▓] 100% - Target eliminated.",

    "> Verifying zeroed sectors...",
    "> Confirming irreversible deletion...",
    "> System reports: no recoverable data found.",
    "> Remaining bytes: 0x00000000",
    "> Purge success code: 0xC0FFEE",
    "> Logging event to hidden syslog...",
    "> Cleaning execution logs...",
    "> Memory cache: flushed.",
    "> Backdoor status: NOT INSTALLED",
    "> Residual signatures: ERASED",
    "> Uninstall timestamp: SYNCHRONIZED",
    "> Registry anomalies: NONE DETECTED",
    "> Kernel response time: NORMAL",
    "> Antivirus flag: BYPASSED",
    "> User detection: NONE",
    "> Root process terminated.",
    "> Awaiting further instructions...",
    "> Session closing in 3... 2... 1...",
    "> Complete!"
};

    private ulong spawnedObjectId;

    [SerializeField] private float totalEffectDuration = 6.0f;
    [SerializeField] private float minDelayFactor = 0.5f;  // 속도 랜덤화 최소 계수
    [SerializeField] private float maxDelayFactor = 1.5f;  // 속도 랜덤화 최대 계수

    [SerializeField] private LayerMask completeLayer;


    public void StartQuest()
	{
        StartCoroutine(PlayHackingEffectAndRefresh());
	}



    private IEnumerator PlayHackingEffectAndRefresh()
    {
        int lineCount = hackingEffectLines.Count;
        float baseDelay = totalEffectDuration / lineCount;
        float usedTime = 0f;

        // 서버에 오브젝트 생성 요청
        SpawnObjectServerRpc();

        for (int i = 0; i < lineCount; i++)
        {
            string line = hackingEffectLines[i];
            history.Add(line);
            displayText.text = BuildDisplayText();

            float randomFactor = Random.Range(minDelayFactor, maxDelayFactor);
            float delay = baseDelay * randomFactor;

            if (usedTime + delay > totalEffectDuration)
            {
                delay = totalEffectDuration - usedTime;
            }

            usedTime += delay;
            yield return new WaitForSeconds(delay);
        }

        if (usedTime < totalEffectDuration)
        {
            yield return new WaitForSeconds(totalEffectDuration - usedTime);
        }

        yield return new WaitForSeconds(0.5f);

        // 서버에 레이어 설정 요청
        SetLayerServerRpc(spawnedObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject obj = Instantiate(USBPrefab, USBSpawnTransfrom.position, USBSpawnTransfrom.rotation);

        NetworkObject networkObj = obj.GetComponent<NetworkObject>();
        if (networkObj != null && !networkObj.IsSpawned)
        {
            networkObj.Spawn();

            // 생성된 오브젝트의 ID를 클라이언트에 전달
            SetSpawnedObjectIdClientRpc(networkObj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void SetSpawnedObjectIdClientRpc(ulong objectId)
    {
        spawnedObjectId = objectId;
    }

    [ClientRpc]
    private void SetLayerClientRpc(ulong objectId, int targetLayerMask)
    {
        int layerIndex = Mathf.RoundToInt(Mathf.Log(targetLayerMask, 2));

        if (layerIndex < 0 || layerIndex > 31)
        {
            Debug.LogError($"Invalid layer index: {layerIndex}");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject netObj))
        {
            netObj.gameObject.layer = layerIndex;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLayerServerRpc(ulong objectId)
    {
        int layerIndex = Mathf.RoundToInt(Mathf.Log(completeLayer.value, 2));

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject netObj))
        {
            netObj.gameObject.layer = layerIndex;
            SetLayerClientRpc(objectId, completeLayer.value);
        }
    }

    private string BuildDisplayText()
    {
        List<string> lines = new List<string>(history);

        // Max line 수를 넘을 경우, 위에서부터 제거
        while (lines.Count > MaxDisplayLines)
            lines.RemoveAt(0);

        return string.Join("\n", lines);
    }

}
