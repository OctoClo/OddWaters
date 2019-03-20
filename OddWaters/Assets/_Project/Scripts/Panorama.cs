using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panorama : MonoBehaviour
{
    [SerializeField]
    GameObject panorama;
    [SerializeField]
    Sprite cursorCenter;
    [SerializeField]
    Sprite cursorLeft;
    [SerializeField]
    Sprite cursorRight;

    Rigidbody rigidBody;
    GameObject cursorBegin;
    Vector2 cursorOffset;

    void Start()
    {
        rigidBody = panorama.GetComponent<Rigidbody>();
        cursorOffset = new Vector2(cursorCenter.texture.width / 2, cursorCenter.texture.height / 2);
    }

    public void BeginDrag(Vector3 beginPos)
    {
        cursorBegin = new GameObject("CursorBegin");
        cursorBegin.transform.position = beginPos;
        SpriteRenderer renderer = cursorBegin.AddComponent<SpriteRenderer>();
        renderer.sprite = cursorCenter;
    }

    public void EndDrag()
    {
        Destroy(cursorBegin);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        rigidBody.velocity = Vector3.zero;
    }

    public void UpdateSpeed(Vector3 speed)
    {
        rigidBody.velocity = new Vector3(speed.x, 0, 0);
        if (speed.x == 0)
            Cursor.SetCursor(cursorCenter.texture, cursorOffset, CursorMode.Auto);
        else if (speed.x < 0)
            Cursor.SetCursor(cursorLeft.texture, cursorOffset, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorRight.texture, cursorOffset, CursorMode.Auto);
    }
}
