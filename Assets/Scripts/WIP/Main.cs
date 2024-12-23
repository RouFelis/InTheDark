//using InTheDark.Prototypes.Programs;

//using System;
//using System.Collections.Generic;

//using Unity.Netcode;

//using UnityEngine;

//namespace InTheDark.Prototypes.Programs
//{
//	[Serializable]
//	public class Entity : IDisposable
//	{
//		private Entity()
//		{

//		}

//		public static Entity CreateInstance()
//		{
//			var instance = new Entity();

//			return instance;
//		}

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public class Health : IDisposable
//	{
//		[SerializeField]
//		private float _value;

//		public float Value
//		{
//			get => _value;

//			set => _value = value;
//		}

//		private Health()
//		{

//		}

//		private Health(float value)
//		{
//			_value = value;
//		}

//		public static Health CreateInstance(float value)
//		{
//			var instance = new Health(value);

//			return instance;
//		}

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public class Attack : IDisposable
//	{
//		// 임시
//		public class Arguments : IDisposable
//		{
//			public float Value;

//			public virtual void Dispose()
//			{
//				// throw new NotImplementedException();
//			}
//		}

//		[SerializeField]
//		private Entity _owner;

//		[SerializeField]
//		private float _value;

//		public float Value
//		{
//			get => _value;

//			set => _value = value;
//		}

//		private Attack()
//		{

//		}

//		private Attack(float value)
//		{
//			_value = value;
//		}

//		public static Attack CreateInstance(float value)
//		{
//			var instance = new Attack(value);

//			return instance;
//		}

//		public static Attack CreateInstance(Entity entity, float value)
//		{
//			var instance = new Attack(value);

//			return instance;
//		}

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}

//		public virtual void Execute()
//		{
//			// throw new NotImplementedException();
//		}

//		public virtual void Execute(Entity target)
//		{

//		}

//		// 임시?
//		public virtual void Execute(Damage target)
//		{
//			using var args = new Arguments()
//			{
//				Value = _value
//			};

//			target.Execute(args);
//		}
//	}

//	// Attackable <-> Attack??
//	// Damageable <-> Damage??

//	[Serializable]
//	public class Damage : IDisposable
//	{
//		[SerializeField]
//		private int _index;

//		public int Index
//		{
//			get => _index;

//			set => _index = value;
//		}

//		public float Value => Indicator.Instance.Values[_index];

//		private Damage()
//		{

//		}

//		private Damage(int index)
//		{
//			_index = index;
//		}

//		public static Damage CreateInstance(int index)
//		{
//			var instance = new Damage(index);

//			return instance;
//		}

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}

//		public virtual void Execute()
//		{
//			// throw new NotImplementedException();
//		}

//		public virtual void Execute(Attack.Arguments args)
//		{
//			Indicator.Instance.Values[_index] += args.Value;
//		}
//	}

//	// 실제 구현시엔 NetworkBehaviour로 구현
//	[Serializable]
//	public class Indicator : IDisposable
//	{
//		private static Indicator _instance;

//		[SerializeField]
//		private List<float> _values = new List<float>();

//		private Stack<int> _pool = new Stack<int>();

//		public static Indicator Instance
//		{
//			get => _instance;
//		}

//		public List<float> Values
//		{
//			get => _values;
//		}

//		static Indicator()
//		{
//			_instance = default;
//		}

//		private Indicator()
//		{

//		}

//		public static Indicator CreateInstance()
//		{
//			if (_instance == null)
//			{
//				var instance = new Indicator();

//				_instance = instance;
//			}

//			return _instance;
//		}

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}

//		public void Add(float value, out int index)
//		{
//			if (_pool.Count > 0)
//			{
//				index = _pool.Pop();

//				_values[index] = value;
//			}
//			else
//			{
//				index = _values.Count;

//				_values.Add(value);
//			}
//		}

//		public bool Remove(int index)
//		{
//			var isActive = !_pool.Contains(index);
//			var isContain = _values.Count < 0 && _values.Count > index;

//			var isRemoved = isActive && isContain;

//			if (isRemoved)
//			{
//				_pool.Push(index);
//			}

//			return isRemoved;
//		}
//	}

//	[Serializable]
//	public struct EntityRuntimeData : IEquatable<EntityRuntimeData>, IDisposable, INetworkSerializable
//	{
//		public bool Equals(EntityRuntimeData other)
//		{
//			// throw new NotImplementedException();

//			return default;
//		}

//		public void Dispose()
//		{
//			// throw new NotImplementedException();
//		}

//		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public struct ModuleRuntimeData : IEquatable<ModuleRuntimeData>, IDisposable, INetworkSerializable
//	{
//		public bool Equals(ModuleRuntimeData other)
//		{
//			// throw new NotImplementedException();

//			return default;
//		}

//		public void Dispose()
//		{
//			// throw new NotImplementedException();
//		}

//		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public class HealthRuntimeData : IDisposable
//	{
//		public float Value;

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public class AttackRuntimeData : IDisposable
//	{
//		public float Value;

//		public virtual void Dispose()
//		{
//			// throw new NotImplementedException();
//		}
//	}

//	[Serializable]
//	public class HealthSourceData
//	{
//		[SerializeField]
//		private float _value;

//		public float Value => _value;
//	}

//	[Serializable]
//	public class AttackSourceData
//	{
//		[SerializeField]
//		private float _value;

//		public float Value => _value;
//	}
//}

//namespace InTheDark.Prototypes
//{
//	public sealed class Main : MonoBehaviour
//    {
//        [SerializeField]
//        private int[] _attackPicker;

//		[SerializeField]
//		private int[] _damagePicker;

//		//

//		[SerializeField]
//		private int _count;

//		[SerializeField]
//		private Entity[] _entityModules;

//		[SerializeField]
//		private Health[] _healthModules;

//		[SerializeField]
//		private Attack[] _attackModules;

//		[SerializeField]
//		private Damage[] _damageModules;

//		[SerializeField]
//		private Indicator _indicator;

//		private void Awake()
//		{
//			var indicator = Indicator.CreateInstance();

//			_entityModules = new Entity[_count];
//			_healthModules = new Health[_count];
//			_attackModules = new Attack[_count];
//			_damageModules = new Damage[_count];

//			_indicator = indicator;

//			for (var i = 0; i < _count; i++)
//			{
//				_indicator.Add(0.0F, out var index);

//				var entity = Entity.CreateInstance();
//				var health = Health.CreateInstance(1870.0F);
//				var attack = Attack.CreateInstance(120.0F);
//				var damage = Damage.CreateInstance(index);

//				_entityModules[i] = entity;
//				_healthModules[i] = health;
//				_attackModules[i] = attack;
//				_damageModules[i] = damage;
//			}
//		}

//		private void OnDestroy()
//		{
//			for (var i = 0; i < _count; i++)
//			{
//				var entity = _entityModules[i];
//				var health = _healthModules[i];
//				var attack = _attackModules[i];
//				var damage = _damageModules[i];

//				entity?.Dispose();
//				health?.Dispose();
//				attack?.Dispose();
//				damage?.Dispose();
//			}
//		}

//		private void Update()
//        {
//            var key = KeyCode.Space;
//            var isKeyDown = Input.GetKeyDown(key);

//            if (isKeyDown)
//            {
//                foreach (var attackIndex in _attackPicker)
//                {
//					foreach (var damageIndex in _damagePicker)
//					{
//						var attack = _attackModules[attackIndex];
//						var damage = _damageModules[damageIndex];

//						attack.Execute(damage);
//					}
//				}

//				for (var i = 0; i < _entityModules.Length; i++)
//				{
//					var entity = _entityModules[i];
//					var health = _healthModules[i];
//					var damage = _damageModules[i];

//					var current = health.Value - damage.Value;

//					var message = $"[{entity}] Health: {current} / {health.Value}";

//					Debug.Log(message);
//				}
//			}
//		}
//	}
//}

using InTheDark.LoremIpsum;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	public sealed class Main : MonoBehaviour
	{
		private void Awake()
		{
			OnAwake();
		}

		private void OnAwake()
		{

		}
	}
}