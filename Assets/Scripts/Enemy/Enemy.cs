using BehaviorDesigner.Runtime;

using UnityEngine;

namespace InTheDark
{
	/// <summary>
	/// 
	/// </summary>
	public interface IEnemy : IEntity
    {

	}

	/// <summary>
	/// 
	/// </summary>
	public class Enemy : Entity, IEnemy
	{

	} 
}