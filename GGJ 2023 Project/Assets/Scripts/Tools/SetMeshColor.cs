using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SetMeshColor : MonoBehaviour
{
    [SerializeField] private string _colorName = "_Color";
    [SerializeField] private Color _color = Color.white;

    private MeshRenderer _renderer;

    void Awake ()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    void OnEnable ()
    {
        SetColor();
    }

    public void SetColor ()
    {
        _renderer.material.SetColor(_colorName, _color);
    }

    public void SetColor(string p_colorName, Color p_color)
    {
        _renderer.material.SetColor(p_colorName, p_color);
    }
}