using UnityEngine;
using UnityEngine.AI;

public class EQSItemInstance : MonoBehaviour
{
	[SerializeField]
	private Color _color;

	public void SetColor(Color color)
	{
		_color = color;
	}

	public void OnDrawGizmosSelected()
	{
		
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = _color;
		Gizmos.DrawSphere(transform.position, 0.25F);
	}
}
