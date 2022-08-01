using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public GameObject chatOb;
    private Coroutine textCo;
    public Animator ani;

    public void Chat(string name, string s)
    {
        chatText.text = $"{name} : {s}";
        if (textCo == null)
        {
            textCo = StartCoroutine(Co_Chat());
        }
        else
        {
            StopCoroutine(textCo);
            textCo = StartCoroutine(Co_Chat());
        }

        chatOb.SetActive(true);
    }

    IEnumerator Co_Chat()
    {
        yield return YieldInstructionCache.WaitForSeconds(2);
        chatOb.SetActive(false);
        textCo = null;
    }
}