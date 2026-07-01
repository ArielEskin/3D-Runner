#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

public class PlayerSwipeInput : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    [SerializeField] private PlayerMovement playerMovement;
    
    [SerializeField] private float miniSwipeDistance = 50f;
    private Vector2 startTouch;
    private Vector2 endTouch; 
    private bool isSwiping;
    
    // ==========================================================================================================================================================
    
    void Update() // Verifies the game is in Touch mode and listens for input
    {
        if (GameManager.gameManager.isDead) return; 
        
        if (InputManager.instance.currentMode != InputMode.Touch)
        {
            return;
        }
        HandleTouch();
        HandleMouse();
    }

    private void HandleMouse() // Records the exact screen coordinates where a click starts and ends (for PC testing only)
    {
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

    private void HandleTouch() // Records the exact screen coordinates where a touch starts and ends
    {
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

    private void DetectSwipte() // Calculates the distance and direction of the swipe to trigger a jump, left move, or right move
    {
        Vector2 delta = endTouch - startTouch;

        if (delta.magnitude < miniSwipeDistance) { return; }
        
        float x = delta.x;
        float y = delta.y;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x > 0) { playerMovement.RightMove(); }
            else { playerMovement.LeftMove(); }
        }
        else { if (y > 0) { playerMovement.Jump(); } }
    }
}
