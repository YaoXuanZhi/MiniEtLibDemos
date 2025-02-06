using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FixAotDllsByAsset : MonoBehaviour
{
    public string text;

    void Start()
    {
        Stack<string> aaa = new Stack<string>();

        var button = GameObject.Find("").GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        LayerMask.NameToLayer("UI");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
        }
    }
}
