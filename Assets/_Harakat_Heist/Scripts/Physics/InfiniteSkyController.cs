using UnityEngine;

public class InfiniteSkyController : MonoBehaviour
{
    [Header("Sky Textures Reference")]
    public SpriteRenderer[] skyTiles = new SpriteRenderer[4]; // 2x2 grid layout
    
    [Header("Parallax Settings")]
    public Transform cameraTransform;
    public float scrollSpeedModifier = 0.05f; // Makes background look miles away

    private Vector2 textureSize;

    void Start()
    {
        // Automatically fetch main camera if not explicitly assigned
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Calculate the native pixel-to-unit boundaries of your sky webp sprite
        if (skyTiles[0] != null)
        {
            textureSize = skyTiles[0].sprite.bounds.size;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null || skyTiles[0] == null) return;

        // Move the entire background matrix slightly with the camera to simulate distance
        Vector3 backgroundPosition = cameraTransform.position * scrollSpeedModifier;
        backgroundPosition.z = transform.position.z; // Retain original layer depth
        transform.position = backgroundPosition;

        // Coordinate wrapping logic for endless looping loop
        foreach (SpriteRenderer tile in skyTiles)
        {
            Vector3 relativePos = cameraTransform.position - tile.transform.position;

            // Horizontal Teleport (Leapfrog Right/Left)
            if (Mathf.Abs(relativePos.x) >= textureSize.x)
            {
                float offsetValueX = Mathf.Sign(relativePos.x) * textureSize.x * 2f;
                tile.transform.position += new Vector3(offsetValueX, 0, 0);
            }

            // Vertical Teleport (Leapfrog Up/Down)
            if (Mathf.Abs(relativePos.y) >= textureSize.y)
            {
                float offsetValueY = Mathf.Sign(relativePos.y) * textureSize.y * 2f;
                tile.transform.position += new Vector3(0, offsetValueY, 0);
            }
        }
    }
}
