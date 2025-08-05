using System.Collections;
using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
	#region DieProperty
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
	[SerializeField] private RectTransform healthBar;
	[SerializeField] private RectTransform compass;



	[Header("ReviveAnime")]
	[SerializeField] private UIAnimation reviveAnimation;



	#endregion


	/*	#region AllDieAni
		public void AllDieAnimation()
		{
			healthBar.localScale = Vector3.zero;
			dieAnime.gameObject.SetActive(true);
			allDieCameraEffect1.gameObject.SetActive(true);
			allDieCameraEffect2.gameObject.SetActive(true);
			allDieCameraEffect3.gameObject.SetActive(true);
			dieAnime.StartEffect();
			allDieCameraEffect1.StartEffect();
			allDieCameraEffect2.StartEffect();
			allDieCameraEffect3.StartEffect();
			Debug.Log("ALL DIE");
		}

		private void DisableAllDieUIAnimations()
		{
			dieAnime.gameObject.SetActive(false);
			allDieCameraEffect1.gameObject.SetActive(false);
			allDieCameraEffect2.gameObject.SetActive(false);
			allDieCameraEffect3.gameObject.SetActive(false);
		}


		#endregion*/


	#region DieAni


	/*private void DisableDieUIAnimations()
	{
		dieCameraEffect1.gameObject.SetActive(false);
		dieCameraEffect2.gameObject.SetActive(false);
		dieCameraEffect3.gameObject.SetActive(false);
	}


	public void DieAnimation()
	{
		healthBar.localScale = Vector3.zero;
		dieCameraEffect1.gameObject.SetActive(true);
		dieCameraEffect2.gameObject.SetActive(true);
		dieCameraEffect3.gameObject.SetActive(true);
		dieCameraEffect1.StartEffect();
		dieCameraEffect2.StartEffect();
		dieCameraEffect3.StartEffect();
		Debug.Log("DIE");
	}*/
	private void DisableDieUIAnimations()
	{
		allDieCameraEffect1.gameObject.SetActive(false);
		allDieCameraEffect2.gameObject.SetActive(false);
		allDieCameraEffect3.gameObject.SetActive(false);
	}


	public void DieAnimation()
	{
		healthBar.localScale = Vector3.zero;
		compass.localScale = Vector3.zero;
		allDieCameraEffect1.gameObject.SetActive(true);
		allDieCameraEffect2.gameObject.SetActive(true);
		allDieCameraEffect3.gameObject.SetActive(true);
		allDieCameraEffect1.StartEffect();
		allDieCameraEffect2.StartEffect();
		allDieCameraEffect3.StartEffect();
		Debug.Log("DIE");
	}

	private void AllDieAnimation(bool oldValue, bool newValue)
	{		
		if(newValue)
			StartCoroutine(AllDieAnimationCo());
	}

	public IEnumerator AllDieAnimationCo()
	{
		yield return new WaitForSeconds(1.618f);

		dieAnime.gameObject.SetActive(true);
		dieAnime.StartEffect();

		yield return new WaitForSeconds(2f);

		dieAnime.gameObject.SetActive(false);
	}


	#endregion


	#region

	public void ReviveAnimation()
	{
		reviveAnimation.StartEffect();
		StartCoroutine(healthbarOn());
	}


	private IEnumerator healthbarOn()
	{
		yield return new WaitForSeconds(1f);
		healthBar.localScale = Vector3.one;
		compass.localScale = Vector3.one;
	}
	#endregion


	private void OnEnable()
	{
		//allDieCameraEffect3.OnAnimationFinished += DisableAllDieUIAnimations;
		allDieCameraEffect3.OnAnimationFinished += DisableDieUIAnimations;
		PlayersManager.Instance.allPlayersDead.OnValueChanged += AllDieAnimation;
	}

	private void OnDisable()
	{
		//allDieCameraEffect3.OnAnimationFinished -= DisableAllDieUIAnimations;
		allDieCameraEffect3.OnAnimationFinished -= DisableDieUIAnimations;
		PlayersManager.Instance.allPlayersDead.OnValueChanged -= AllDieAnimation;
	}
}
