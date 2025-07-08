using Unity.Netcode;

using UnityEngine;

namespace InTheDark
{
	/// <summary>
	/// 
	/// </summary>
	public interface IEntity : IDamageable
	{
		/// <summary>
		/// 
		/// </summary>
		public bool IsAlive
		{
			get; set;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IDamageable
	{
		/// <summary>
		/// 
		/// </summary>
		public float CurrentHealth
		{
			get; set;
		}

		/// <summary>
		/// 
		/// </summary>
		public float MaxHealth
		{
			get; set;
		}
	}

	public static class EntityExtensionMethods
	{

	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class Entity : NetworkBehaviour, IEntity
	{
		/// <summary>
		/// 
		/// </summary>
		[SerializeField] 
        private NetworkVariable<float> _currentHealth = new();

		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
		private NetworkVariable<float> _maxHealth = new();

		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
        private NetworkVariable<bool> _isAlive = new();

		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
        private Animator _animator;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
		private Collider _collider;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
        private Renderer _renderer;

		/// <summary>
		/// 
		/// </summary>
		public float CurrentHealth
        {
            get
            {
                var value = _currentHealth.Value;

                return value;
            }

            set
			{
				value = Mathf.Clamp(value, 0.0F, _maxHealth.Value);

				if (_currentHealth.Value != value)
                {
					_currentHealth.Value = value;
				}
            }
        }

		/// <summary>
		/// 
		/// </summary>
		public float MaxHealth
		{
			get
			{
				var value = _maxHealth.Value;

				return value;
			}

			set
			{
				value = Mathf.Max(0.0F, value);

				if (_maxHealth.Value != value)
				{
					_maxHealth.Value = value;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsAlive
        {
			get
			{
				var value = _isAlive.Value;

				return value;
			}

			set
			{
				if (_isAlive.Value != value)
				{
					_isAlive.Value = value;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Animator Animator
        {
            get
            {
                var value = _animator;

                return value;
            }

			protected set
			{
				_animator = value;
			}
        }

		/// <summary>
		/// 
		/// </summary>
		public Collider Collider
		{
			get
			{
				var value = _collider;

				return value;
			}

			protected set
			{
				_collider = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Renderer Renderer
        {
            get
            {
                var value = _renderer;

                return value;
			}

			protected set
			{
				_renderer = value;
			}
        }

		/// <summary>
		/// 
		/// </summary>
		protected NetworkVariable<float> NetworkCurrentHealth
		{
			get
			{
				var value = _currentHealth;

				return value;
			}

			private set
			{
				_currentHealth = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected NetworkVariable<float> NetworkMaxHealth
		{
			get
			{
				var value = _maxHealth;

				return value;
			}

			private set
			{
				_maxHealth = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected NetworkVariable<bool> NetworkIsAlive
		{
			get
			{
				var value = _isAlive;

				return value;
			}

			private set
			{
				_isAlive = value;
			}
		}

		protected virtual void Awake()
		{

		}

		protected virtual void Start()
		{

		}

		public void OnAttacked()
		{

		}
	}
}