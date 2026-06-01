using System;
using UnityEngine;
using Unity.VisualScripting;

public class TextField : MonoBehaviour
{
    [SerializeField]
    public string itemDescription = "itemDescription";

    [TextArea]
    [SerializeField]
    public string itemDescriptiontext;
    private void Start()
    {
        Variables.Object(this).Set(itemDescription, itemDescriptiontext);
    }
}
