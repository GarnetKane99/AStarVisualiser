using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLink : MonoBehaviour
{
    public Button link;

    private void Awake()
    {
        link.onClick.AddListener(() =>
        {
            Application.OpenURL("https://www.youtube.com/@GarnetKane");
        });

    }
}
