using System;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public class EnemyPrototype : EnemyPrototypePawn
	{
        protected override void OnUpdate()
        {
            base.OnUpdate();

            Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.magenta);
        }
    }
}