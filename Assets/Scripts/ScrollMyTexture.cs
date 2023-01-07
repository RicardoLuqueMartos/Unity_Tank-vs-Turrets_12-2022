using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMyTexture : MonoBehaviour
{
    public float scrollSpeedX = 0f;
    public float scrollSpeedY = 0f;

    private Renderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<Renderer>();


    }

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.mainTextureOffset = new Vector2(Time.realtimeSinceStartup * scrollSpeedX, Time.realtimeSinceStartup * scrollSpeedY);
    }
}
