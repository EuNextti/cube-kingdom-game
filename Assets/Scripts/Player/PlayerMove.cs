using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    private float fallSpeed;
    private float turnSmoothVelocity;

    private Vector3 movement;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }
    }

    public void Move(CharacterController cc, float movementSpeed, Transform cam, Animator anim)
    {
        movement = ApplyMovement();

        ApplyGravity(cc);
        movement.y = fallSpeed;

        anim.SetBool("walking", movement.magnitude >= 0.1f);

        bool aiming = Input.GetMouseButton(1);

        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, aiming ? cam.eulerAngles.y : targetAngle, ref turnSmoothVelocity, aiming ? 0.05f : 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 direction = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            direction.Normalize();
            direction.y = movement.y;

            anim.SetFloat("moveY", aiming ? movement.z : 1);
            anim.SetFloat("moveX", aiming ? movement.x : 0);

            cc.Move(movementSpeed * Time.deltaTime * direction);
        }
        else
        {
            anim.SetFloat("moveY", movement.z);
            anim.SetFloat("moveX", movement.x);
        }
    }

    private Vector3 ApplyMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 position = new Vector3(inputX, 0, inputY).normalized;

        return position;
    }

    private void ApplyGravity(CharacterController cc)
    {
        if (cc.isGrounded)
        {
            fallSpeed = 0.0f;
        }
        else
        {
            fallSpeed += -9.81f * 3.0f * Time.deltaTime;
        }
    }
}
