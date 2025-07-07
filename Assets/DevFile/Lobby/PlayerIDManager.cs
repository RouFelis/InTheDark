using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerIDManager : MonoBehaviour
{
    private string playerName = "";

    public string PlayerName { get =>playerName; set =>playerName = value ; }

    [SerializeField] private TMP_InputField playerIDinput;


    public void PlayerIDSetter()
	{
		if (!string.IsNullOrEmpty(playerIDinput.text))
		{
			PlayerName = playerIDinput.text;
		}
    }
}
