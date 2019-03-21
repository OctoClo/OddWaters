using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panorama : MonoBehaviour
{
    [SerializeField]
    Sprite cursorCenter;
    [SerializeField]
    Sprite cursorLeft;
    [SerializeField]
    Sprite cursorRight;
    
    GameObject cursorBegin;
    Vector2 cursorOffset;
    Vector3 cursorScale;

    Renderer planeRenderer;
    Vector2 planeOffset;
    float dragSpeed;

    void Start()
    {
        cursorOffset = new Vector2(cursorCenter.texture.width / 2, cursorCenter.texture.height / 2);
        cursorScale = new Vector3(3, 3, 0);

        planeRenderer = GetComponent<MeshRenderer>();
        planeOffset = new Vector2(0, 0);
        dragSpeed = 0;
    }

    public void BeginDrag(Vector3 beginPos)
    {
        cursorBegin = new GameObject("CursorBegin");
        beginPos.y = 0;
        cursorBegin.transform.position = beginPos;
        cursorBegin.transform.rotation = Quaternion.Euler(90, 0, 0);
        cursorBegin.transform.localScale = cursorScale;
        SpriteRenderer renderer = cursorBegin.AddComponent<SpriteRenderer>();
        renderer.sprite = cursorCenter;
    }

    public void EndDrag()
    {
        dragSpeed = 0;
        Destroy(cursorBegin);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void UpdateSpeed(float speed)
    {
        dragSpeed = speed;
        if (dragSpeed == 0)
            Cursor.SetCursor(cursorCenter.texture, cursorOffset, CursorMode.Auto);
        else if (dragSpeed < 0)
            Cursor.SetCursor(cursorLeft.texture, cursorOffset, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorRight.texture, cursorOffset, CursorMode.Auto);
    }

    void Update()
    {
        planeOffset.x -= dragSpeed * Time.deltaTime;
        planeRenderer.material.SetTextureOffset("_MainTex", planeOffset);
    }
}
