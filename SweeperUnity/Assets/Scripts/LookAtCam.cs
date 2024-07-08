using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    static TextMode textMode = TextMode.Look90degrees;
    enum TextMode
    {
        LookExactlyAtCam,
        Look90degrees
    }
    public GameObject debugUp;
    public GameObject debugForward;


    void Update()
    {
        switch (ViewManager.instance._viewMode)
        {
            case ViewMode.Normal:
                transform.LookAt(ViewManager.instance.mainCamera.transform, ViewManager.instance.mainCamera.transform.up);
                transform.Rotate(ViewManager.instance.mainCamera.transform.up, 180);
                break;
            case ViewMode.VR:
                switch (textMode)
                {
                    case TextMode.LookExactlyAtCam:
                        //transform.LookAt((transform.position - ViewManager.instance.mainCamera.transform.position) + transform.position, Vector3.up);
                        transform.LookAt((transform.position - ViewManager.instance.mainCamera.transform.position) + transform.position, ViewManager.instance.mainCamera.transform.up);
                        break;
                    case TextMode.Look90degrees:

                        //trying to get it in local space but something is very off
                        Vector3 direction = (transform.parent.rotation * (transform.position - ViewManager.instance.mainCamera.transform.position));

                        transform.localRotation = Quaternion.LookRotation(GetClosestAxis(direction), GetClosestAxis(transform.parent.rotation * ViewManager.instance.mainCamera.transform.up));
                        debugUp.transform.localPosition = transform.parent.rotation * ViewManager.instance.mainCamera.transform.up;
                        debugForward.transform.localPosition = direction.normalized;
                        break;
                }
                break;
        }
    }

    Vector3 GetClosestAxis(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
        {
            if (direction.x >= 0)
                direction = new Vector3(1, 0, 0);
            else
                direction = new Vector3(-1, 0, 0);
        }
        else if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x) && Mathf.Abs(direction.y) >= Mathf.Abs(direction.z))
        {
            if (direction.y >= 0)
                direction = new Vector3(0, 1, 0);
            else
                direction = new Vector3(0, -1, 0);
        }
        else
        {
            if (direction.z >= 0)
                direction = new Vector3(0, 0, 1);
            else
                direction = new Vector3(0, 0, -1);
        }
        return direction;
    }
}
