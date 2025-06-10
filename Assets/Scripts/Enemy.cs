using BehaviorDesigner.Runtime;

using UnityEngine;

namespace InTheDark
{
	/// <summary>
	/// 
	/// </summary>
	public interface IEnemy : IEntity
    {
		/// <summary>
		/// 
		/// </summary>
		public BehaviorTree CurrentBehavior
		{
			get;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Enemy : Entity, IEnemy
	{
		/// <summary>
		/// 
		/// </summary>
		[SerializeField]
		private BehaviorTree _currentBehavior;

		/// <summary>
		/// 
		/// </summary>
		public BehaviorTree CurrentBehavior
		{
			get
			{
				var value = _currentBehavior;

				return value;
			}

			protected set
			{
				_currentBehavior = value;
			}
		}
	} 
}