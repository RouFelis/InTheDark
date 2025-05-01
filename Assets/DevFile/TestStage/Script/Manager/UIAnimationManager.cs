using UnityEngine;
using System.Collections;

public class UIAnimationManager : MonoBehaviour
{
	[Header("AllDieAnime")]
	[SerializeField] private UIAnimation dieAnime;
	[SerializeField] private UIAnimation allDieCameraEffect1;
	[SerializeField] private UIAnimation allDieCameraEffect2;
	[SerializeField] private UIAnimation allDieCameraEffect3;

	[Header("DieAnime")]
	[SerializeField] private UIAnimation dieCameraEffect1;
	[SerializeField] private UIAnimation dieCameraEffect2;
	[SerializeField] private UIAnimation dieCameraEffect3;

	[Header("Close Object")]
	[SerializeField] private GameObject healthBar;

	#region AllDieAni
	public void AllDieAnimation()
	{
		healthBar.SetActive(false);
		dieAnime.gameObject.SetActive(true);
		allDieCameraEffect1.gameObject.SetActive(true);
		allDieCameraEffect2.gameObject.SetActive(true);
		allDieCameraEffect3.gameObject.SetActive(true);
		dieAnime.StartEffect();
		allDieCameraEffect1.StartEffect();
		allDieCameraEffect2.StartEffect();
		allDieCameraEffect3.StartEffect();
	}

	private void DisableAllDieUIAnimations()
	{
		dieAnime.gameObject.SetActive(false);
		allDieCameraEffect1.gameObject.SetActive(false);
		allDieCameraEffect2.gameObject.SetActive(false);
		allDieCameraEffect3.gameObject.SetActive(false);
	}


	#endregion


	#region DieAni


	private void DisableDieUIAnimations()
	{
		dieCameraEffect1.gameObject.SetActive(false);
		dieCameraEffect1.gameObject.SetActive(false);
		dieCameraEffect1.gameObject.SetActive(false);
		dieCameraEffect1.gameObject.SetActive(false);
	}

	public void DieAnimation()
	{
		healthBar.SetActive(false);
		dieCameraEffect1.gameObject.SetActive(true);
		dieCameraEffect2.gameObject.SetActive(true);
		dieCameraEffect3.gameObject.SetActive(true);
		dieCameraEffect1.StartEffect();
		dieCameraEffect2.StartEffect();
		dieCameraEffect3.StartEffect();
	}
	
	#endregion



	private void OnEnable()
	{
		allDieCameraEffect3.OnAnimationFinished += DisableAllDieUIAnimations;
		dieCameraEffect3.OnAnimationFinished += DisableDieUIAnimations;
	}

	private void OnDisable()
	{
		allDieCameraEffect3.OnAnimationFinished -= DisableAllDieUIAnimations;
		dieCameraEffect3.OnAnimationFinished -= DisableDieUIAnimations;
	}
}
