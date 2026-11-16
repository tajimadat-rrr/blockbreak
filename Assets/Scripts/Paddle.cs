using UnityEngine;
using UnityEngine.InputSystem;



public class Paddle : MonoBehaviour
{
    [SerializeField] private float minX = -2.9f;
    [SerializeField] private float maxX = 2.9f;

    private bool canMove = false;  //動いてよい状態か

    // Update is called once per frame
    void Update()
    {
        if (canMove  == false)
        {
            return;
        }


        Vector2 mousePosition = Mouse.current.position.ReadValue();
        //マウス画面上の座標を取得

        //画面→Unitｙ座標に変換
        Vector3 WorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //Debug.Log(WorldPosition);

        //動かせる範囲を決める
        float limiitedX = Mathf.Clamp(WorldPosition.x, minX, maxX);
        transform.position = new Vector3(limiitedX, transform.position.y, 0);
    }


    public void StartMove()
    {
        canMove = true;
    }
}
