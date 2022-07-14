using UnityEngine;
using UnityEngine.UI;

public class CodeContainer : MonoBehaviour
{
    [Header("Prefab located objects")]
    public Image codeUsing;
    public Text codeBox;
    public GameObject deviceAContainer;
    public Image deviceAImage;
    public Text deviceANameBox;
    public Text deviceADateBox;
    public GameObject deviceBContainer;
    public Image deviceBImage;
    public Text deviceBNameBox;
    public Text deviceBDateBox;
    public GameObject deviceBBackBox;

    [Header("Sprites")]
    public Sprite imageOn;
    public Sprite imageOff;
    public Sprite dotOn;
    public Sprite dotOff;

    public string AIdentifier;
    public string BIdentifier;

    public float currentHeight;
    public bool isActive;
}