using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Input/Normal")]
public class InputManager : InputMiddleware
{
    public float jumpPressedRemember;
    private float jumpPressedRememberValue;

    public override bool Process(InputState input)
    {
        input.moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        jumpPressedRememberValue -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
            jumpPressedRememberValue = jumpPressedRemember;

        if (jumpPressedRememberValue > 0)
        {
            input.canJump = true;
            return true;
        }
        return false;
    }
}
