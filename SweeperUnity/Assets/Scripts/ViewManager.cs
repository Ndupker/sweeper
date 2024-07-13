using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ViewMode
{
    Normal,
    VR
}
public class ViewManager : MonoBehaviour
{
    static ViewManager _viewManager;
    public static ViewManager instance
    {
        get{return _viewManager;}

    }

    public ViewMode _viewMode;
    public GameObject _defaultGO;
    public Camera _defaultCam;
    public GameObject _VRGO;
    public Camera _VRCam;
    public GameObject _VRWorld;

    public Camera mainCamera
    {
        get
        {
            switch (_viewMode)
            {
                case ViewMode.Normal:
                    return _defaultCam;
                case ViewMode.VR:
                    return _VRCam;
            }
            return null;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _viewManager = this;
        _defaultGO.SetActive(_viewMode == ViewMode.Normal);
        if(_VRGO != null)
            _VRGO.SetActive(_viewMode == ViewMode.VR);
        if (_VRWorld != null)
            _VRWorld.SetActive(_viewMode == ViewMode.VR);
    } 
}
