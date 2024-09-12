using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// [========================= LightManager =========================]

	public class LightManager : NetworkBehaviour
    {
		// [========================= Static =========================]

		private static LightManager _instance;

        public static LightManager Instance => _instance;

		// [========================= Field =========================]

		[SerializeField]
		private List<LightSource> _sources = new List<LightSource>();

		private List<Collider> _dirties = new List<Collider>();

		// [========================= Method =========================]

		static LightManager()
        {
            _instance = null;
		}

		private void Awake()
		{
            OnAwake();
		}

		private void OnAwake()
        {
			if (!_instance)
			{
				_instance = this;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
		}

		public override void OnNetworkSpawn()
		{
			if (IsServer)
			{
				UpdateManager.OnUpdate += OnUpdate;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsServer)
			{
				UpdateManager.OnUpdate -= OnUpdate;
			}
		}

		private void OnUpdate()
		{
			// TODO: �ų��� �ڵ� �ݺ��� �ð�

			// �ܼ��ϰ� 9��° ���̾��! �ϰ� 9�� �� �־��ְ� ���� �ַλ����� ����
			// �츮 Enemy ģ��... ���̾��ũ �� 512��...?
			var layerMask = 1 << 9;
			var ifNameToLayer = LayerMask.NameToLayer("Enemy");

			//Debug.Log($"layerMask: {layerMask}, ifNameToLayer: {ifNameToLayer}");

			_dirties.Clear();

			foreach (var source in _sources)
			{
				var angle = source.Angle;
				var distance = source.Distance;
				var targets = Physics.OverlapSphere(source.transform.position, distance, layerMask);

				foreach (var target in targets)
				{
					var direction = target.transform.position - source.transform.position;
					var isOccultation = Physics.Raycast(source.transform.position, direction, out var hit, distance);
					var isSight = Vector3.Angle(direction, source.transform.forward) < angle;

					Debug.Log($"{name} detected {target.name} => layer: {target.gameObject.layer}");

					if (hit.collider == target && isOccultation && isSight && !_dirties.Contains(target))
					{
						var pawn = target.GetComponent<EnemyPrototypePawn>();

						pawn?.OnLightInsighted(source);

						_dirties.Add(target);
					}
				}
			}

			foreach (var dirty in _dirties)
			{
				var pawn = dirty.GetComponent<EnemyPrototypePawn>();

				// �̸� �ٸ��� �ؾ� �ϴµ� ���� ���� �ȳ�.
				pawn?.OnLightInsighted();
			}
		}

		public void OnWorkLightSpanwed(LightSource light)
		{
			_sources.Add(light);
		}

		public void OnWorkLightDespawned(LightSource light)
		{
			_sources.Remove(light);
		}
	} 
}