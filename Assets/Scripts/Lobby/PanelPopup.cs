using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelPopup : MonoBehaviour
{
    public GameObject panel;

    public void InvertActive()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
