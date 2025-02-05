using UnityEngine;

public class EnemyAnimsTest : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		var down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        var left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		var right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        var attack = Input.GetKeyDown(KeyCode.Space) && !_animator.GetCurrentAnimatorStateInfo(1).IsName("Attack");

        var move = up || down || left || right;

        //Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

        if (attack)
        {
            _animator.SetTrigger("OnAttack");
		}

		_animator.SetBool("IsWalking", move);
	}
}
