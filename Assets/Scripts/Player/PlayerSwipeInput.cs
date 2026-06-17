using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

public class PlayerSwipeInput : MonoBehaviour
{
    // ============== References ==============
    [SerializeField] private PlayerMovement playerMovement;
    
    // ============= Swipe Distance count ============ 
    [SerializeField] private float miniSwipeDistance = 50f;
    
    // =========== touch ==========
    private Vector2 startTouch; // where is the touch start
    private Vector2 endTouch; // where is the touch end
    
    private bool isSwiping; // track the swipe
    
    // Update is called once per frame
    void Update()
    {
        //check if the player is dead if yes stop the input
        if (GameManager.gameManager.isDead) return; 
        
        if (InputManager.instance.currentMode != InputMode.Touch)
        {
            return; // disable swipe input
        }
        
        HandleTouch();
        HandleMouse();
    }

    private void HandleMouse()
    {
        // check if there is a mouse 
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startTouch = Mouse.current.position.ReadValue();
            isSwiping = true;
        }
        
        if (Mouse.current.leftButton.wasReleasedThisFrame &&  isSwiping)
        {
            endTouch = Mouse.current.position.ReadValue();
            DetectSwipte();
            isSwiping = false;
        }
    }

    private void HandleTouch()
    {
        // check if finger pressing the screen
        if (Touchscreen.current == null) return;

        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            startTouch = Touchscreen.current.primaryTouch.position.ReadValue();
            isSwiping = true;
            
        }

        if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame && isSwiping)
        {
            endTouch = Touchscreen.current.primaryTouch.position.ReadValue();
            DetectSwipte();
            isSwiping = false;
        }
        
    }

    private void DetectSwipte()
    {
        Vector2 delta = endTouch - startTouch; // Difference between start and end position

        if (delta.magnitude < miniSwipeDistance) // if the swipe is to small ignore it
        {
            return;
        }
        
        float x = delta.x;
        float y = delta.y;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x > 0)
            {
                playerMovement.RightMove(); //player move right
            }
            else
            {
                playerMovement.LeftMove(); //player move left
            }
        }
        else
        {
            if (y > 0)
            {
                playerMovement.Jump(); //player jump
            }
        }

    }
}
