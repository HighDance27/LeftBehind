using UnityEngine;

public class InvisibleWall : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = false;
    }
}
