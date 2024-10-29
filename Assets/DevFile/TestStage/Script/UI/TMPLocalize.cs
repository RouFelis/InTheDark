using UnityEngine;
using TMPro;


public class TMPLocalize : MonoBehaviour
{
    TMP_Text tmp;
    [SerializeField] string tmpContents;

    void Start()
    {
        tmp = this.GetComponent<TMP_Text>();
        tmp.text = tmpContents;
    } 
}
