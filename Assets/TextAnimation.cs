using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    public string[] strings;
    public float delay;
    public bool loop = true;
    public float randomness = 0.1f;

    
    private TextMeshProUGUI textMesh;
    private float timer;
    private int index = 0;

    
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            timer = -Random.Range(0f, randomness);
            if(!loop && index >= strings.Length-1)
            {
                return;
            }
            index++;
            if (index >= strings.Length)
            {
                index = 0;
            }
            textMesh.text = strings[index];
            
        }
    }
}
