using System.Linq;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    public static EnemyUI instance;

    [SerializeField] Transform objectToFollow;
    [SerializeField] Renderer objectToFollowRend;

    [SerializeField] RectTransform uiObject;
    [SerializeField] Vector3 offset;

    private Camera cam;
    public bool allowObjectToMoveOffscreen = true;

    private void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        if (objectToFollow && objectToFollowRend)
        {
            uiObject.gameObject.SetActive(true);
            // calculate screen position of object to follow
            // takes world position and changes that to screen position
            Vector3 screenPos = cam.WorldToScreenPoint(objectToFollow.position);
            // get rect transform of the renderer
            Rect rect = GetScreenRect(objectToFollowRend, cam);
            // calculate the position, anchor one point of the object instead of the middle
            Vector3 pos = screenPos + offset + new Vector3(rect.width / 2f, rect.height / 2f);
            pos = AllowPositionOverride(pos);
            uiObject.position = pos;
        }
        else
        {
            uiObject.gameObject.SetActive(false);
        }
    }

    Vector3 AllowPositionOverride(Vector3 pos)
    {
        // if allowing it to go offscreen, leave it as is
        if (allowObjectToMoveOffscreen) return pos;
        // see if object is outside of screen bounds
        bool isOutsideX = (pos.x + uiObject.sizeDelta.x / 2f) >= Screen.width || (pos.x - uiObject.sizeDelta.x / 2f) < 0;
        bool isOutsideY = (pos.y + uiObject.sizeDelta.x / 2f) >= Screen.height || (pos.y - uiObject.sizeDelta.x / 2f) < 0;
        // set new position
        Vector3 newPos = pos;
        // if outside, set new position
        // otherwise, leave it as is
        newPos.x = isOutsideX ? uiObject.position.x : pos.x;
        newPos.y = isOutsideY ? uiObject.position.y : pos.y;
        // return new position
        return newPos;
    }

    public static Rect GetScreenRect(Renderer renderer, Camera cam)
    {
        // get bounds of renderer object
        Bounds bounds = renderer.bounds;
        // vector3 array to contain each corner
        Vector3[] corners =
        {
            cam.WorldToScreenPoint(bounds.min),
            cam.WorldToScreenPoint(bounds.max),

            cam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)),
            cam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z)),

            cam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z)),
            cam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)),

            cam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)),
            cam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)),
        };

        float minX = corners.Min(v => v.x);
        float maxX = corners.Max(v => v.x);
        float minY = corners.Min(v => v.y);
        float maxY = corners.Max(v => v.y);
        float minZ = corners.Min(v => v.z);
        float maxZ = corners.Max(v => v.z);

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
