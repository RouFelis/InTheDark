using UnityEngine;

public class LizardAnimatorTester : MonoBehaviour
{
    [SerializeField]
    private Animator _anim;

    [SerializeField]
    private KeyCode _attack = KeyCode.A;

    [SerializeField]
    private KeyCode _dead = KeyCode.D;

    [SerializeField]
    private KeyCode _idle = KeyCode.I;

    [SerializeField]
    private KeyCode _walk = KeyCode.W;

	private void Awake()
	{
        if (!_anim)
        {
			_anim = GetComponent<Animator>();
		}
	}

	private void Update()
    {
        if (Input.GetKeyDown(_attack) && _anim)
        {
            _anim.SetBool("Attacking", true);
			_anim.SetBool("Dead", false);
			_anim.SetBool("Walking", false);
		}
        else if (Input.GetKeyDown(_dead) && _anim)
        {
			_anim.SetBool("Attacking", false);
			_anim.SetBool("Dead", true);
			_anim.SetBool("Walking", false);
		}
		else if (Input.GetKeyDown(_walk) && _anim)
		{
			_anim.SetBool("Attacking", false);
			_anim.SetBool("Dead", false);
			_anim.SetBool("Walking", true);
		}
		else if (Input.GetKeyDown(_idle) && _anim)
        {
			_anim.SetBool("Attacking", false);
			_anim.SetBool("Dead", false);
			_anim.SetBool("Walking", false);
		}
    }
}
