using UnityEngine;

public class ArrowSegmentClickArea : MonoBehaviour
{
    public Arrow Owner;

    private void OnMouseDown()
    {
        Arrow owner = Owner != null ? Owner : GetComponentInParent<Arrow>();
        if (owner != null)
        {
            owner.HandleClick();
        }
    }
}
