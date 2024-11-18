using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Scanner : MonoBehaviour
{
	[SerializeField] GameObject scannerPrefab;
	int time = 3;

	private void Update()
	{
		if (Input.GetKeyDown(KeySettingsManager.Instance.ScanKey))		
			StartCoroutine(Scan());
	}

	public IEnumerator Scan()
	{
		Debug.Log("test");
		for (int i = 0; i < time; i++)
		{
			GameObject newObject = Instantiate(scannerPrefab, transform.position, Quaternion.identity);
			yield return new WaitForSeconds(0.1f);
		}
	}	
}
