using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRUI : MonoBehaviour
{
    public Material _defaultMat;
    public Material _highlightedMat;
    public UnityEvent _event;
    public bool _debugTrigger = false;
    Renderer _rend;
    bool _highlighted = false;
    public bool Highlighted
    {
        get  { return _highlighted; }
        set { _highlighted = value;
            _rend.sharedMaterial = _highlighted ? _highlightedMat : _defaultMat;
        }
    }
    public void Select()
    {
        _event.Invoke();
    }


    // Start is called before the first frame update
    void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        if(_debugTrigger)
        {
            _debugTrigger = false;
            Select();
        }
    }
}
