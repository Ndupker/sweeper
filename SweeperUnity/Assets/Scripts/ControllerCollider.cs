using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCollider : MonoBehaviour
{
    Collider _closestCollider = null;
    List<Collider> _currentCollisions = new List<Collider>();
    List<CellComponent> _currentCells = new List<CellComponent>();
    public Map _map = null;
    /*
    public OVRInput.Controller controllerType = OVRInput.Controller.LTouch;
    public OVRInput.RawButton uiButtonSelect = OVRInput.RawButton.LIndexTrigger;
    */
    VRUI _highlightedUI = null;
    bool _allowedToInteractWithMap = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!_allowedToInteractWithMap)
            return;
        CellComponent cell = other.GetComponent<CellComponent>();
        if (cell == null)
            return;
        if(_currentCollisions.Contains(other))
        {
            Debug.LogError("Cell collder being entered twice?");
            return;
        }
        _currentCollisions.Add(other);
        _currentCells.Add(cell);
        if (_closestCollider == null)
        {
            //select
            SetSelected(other, true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!_allowedToInteractWithMap)
            return;
        RemoveCollider(other);
    }

    void RemoveCollider(Collider other)
    {
        if (_currentCollisions.Contains(other))
        {
            if (_closestCollider == other)
            {
                //unselect
                SetSelected(_closestCollider, false);
            }
            int id = _currentCollisions.IndexOf(other);
            _currentCollisions.RemoveAt(id);
            _currentCells.RemoveAt(id);
        }
    }
    /*public static IEnumerator TriggerHaptics(OVRInput.Controller controllerType, float vibrationLevel, float time)
    {
        OVRInput.SetControllerVibration(vibrationLevel, vibrationLevel, controllerType);
        yield return new WaitForSeconds(time);
        OVRInput.SetControllerVibration(0f, 0f, controllerType);
    }*/
    void SetSelected(Collider collider, bool selected)
    {
        if (selected)
        {
            if (_closestCollider != collider)
            {
                //StartCoroutine(TriggerHaptics(controllerType, 0.2f, 0.05f));
            }
            _closestCollider = collider;
        }
        else if (_closestCollider == collider)
            _closestCollider = null;
        else
            return;

        int id = _currentCollisions.IndexOf(collider);
        _map.SetSelected(selected, _currentCells[id]._pos);
    }

    public CellComponent GetSelectedCell()
    {
        if (_closestCollider == null)
            return null;
        int id = _currentCollisions.IndexOf(_closestCollider);
        return _currentCells[id];
    }

    public void SetMapInteraction(bool enableInteraction)
    {
        _allowedToInteractWithMap = enableInteraction;
        if(!_allowedToInteractWithMap)
        {
            if (_closestCollider != null)
                SetSelected(_closestCollider, false);
            _currentCollisions.Clear();
            _currentCells.Clear();
            _closestCollider = null;
        }
    }

    private void Update()
    {
        if (_allowedToInteractWithMap)
        {
            float distance = float.MaxValue;
            if (_closestCollider != null)
            {
                if (!_currentCollisions.Contains(_closestCollider) || !_closestCollider.enabled)
                {
                    //unselect
                    SetSelected(_closestCollider, false);
                }
                else
                {
                    distance = Vector3.Magnitude(transform.position - _closestCollider.transform.position);
                }
            }
            for (int i = 0; i < _currentCollisions.Count; i++)
            {
                if (!_currentCollisions[i].enabled)
                {
                    RemoveCollider(_currentCollisions[i]);
                    i--;
                    continue;
                }
                if (Vector3.Magnitude(transform.position - _currentCollisions[i].transform.position) < distance)
                {
                    distance = Vector3.Magnitude(transform.position - _currentCollisions[i].transform.position);

                    if (_closestCollider != null)
                        //unselect
                        SetSelected(_closestCollider, false);
                    //select
                    SetSelected(_currentCollisions[i], true);
                }
            }
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            VRUI vrui = hit.collider.GetComponent<VRUI>();
            if (vrui != null)
            {
                if (vrui != _highlightedUI)
                {
                    if (_highlightedUI != null)
                    {
                        _highlightedUI.Highlighted = false;
                    }
                    _highlightedUI = vrui;
                    _highlightedUI.Highlighted = true;
                    //StartCoroutine(TriggerHaptics(controllerType, 0.2f, 0.05f));
                }
            }
            else if (_highlightedUI != null)
            {
                _highlightedUI.Highlighted = false;
                _highlightedUI = null;
            }
        }
        else if (_highlightedUI != null)
        {
            _highlightedUI.Highlighted = false;
            _highlightedUI = null;
        }

        /*if (OVRInput.GetDown(uiButtonSelect, controllerType))
        {
            if (_highlightedUI != null)
                _highlightedUI.Select();
        }*/
    }
}
