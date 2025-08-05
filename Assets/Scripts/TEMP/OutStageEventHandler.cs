using System;
using System.Collections;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

namespace InTheDark.Prototypes
{
    public class OutStageEventHandler : MonoBehaviour
    {
        public static event Action<bool> OnStageExit;

        public string ObjectName;

        private void Awake()
        {
			DontDestroyOnLoad(gameObject);

			StartCoroutine(AddEvent());
        }

		private void OnDestroy()
		{
			var gameObject = GameObject.Find(ObjectName);

			if (gameObject)
			{
				var stage = gameObject.GetComponent<OutStage>();

				if (stage)
				{
					stage.outDoorAction -= OnPLayerStageExit;
				}
			}
		}

		private IEnumerator AddEvent()
        {
            var isRunning = true;

            while (isRunning)
            {
                var gameObject = GameObject.Find(ObjectName);

                if (gameObject)
                {
                    var stage = gameObject.GetComponent<OutStage>();

					if (stage)
					{
                        stage.outDoorAction += OnPLayerStageExit;

                        Debug.Log("¿¬°á È®¤·;¤Ó¤¤!!!!!!!!!!!!!!!!11");

                        isRunning = !isRunning;
					}
					else
					{
						yield return null;
					}
				}
                else
                {
                    yield return null;
                }
            }
        }

        private void OnPLayerStageExit(bool value)
        {
			OnStageExit?.Invoke(value);
		}
    } 
}