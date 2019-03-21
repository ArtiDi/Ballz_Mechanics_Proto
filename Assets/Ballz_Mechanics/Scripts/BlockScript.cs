using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEditor;
[ExecuteAlways]
public class BlockScript : MonoBehaviour
{
    public int HealthPoints = 1;
    public int HealthPointsProperty
    {
        get { return HealthPoints; }
        set
        {
            HealthPoints = value;
            if (HealthPoints <= 0)
            {
                DestroyBlock();
                MainManagerScript.Instance.CheckWin();
                return;
            }
            UpdateText();
        }
    }
    [SerializeField]
    private TextMeshPro HealthPointsText;
    public void UpdateText()
    {
        try
        {
            HealthPointsText.text = HealthPoints.ToString();
        }
        catch (Exception e)
        {

        }
    }
    private void OnEnable()
    {
        HealthPointsText = GetComponentInChildren<TextMeshPro>();
        HealthPointsText.text = HealthPoints.ToString();
    }
    void Start()
    {
        HealthPointsText.text = HealthPoints.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            HealthPointsProperty--;
        }
    }
    void DestroyBlock()
    {
        this.gameObject.SetActive(false);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BlockScript))]
public class BlockScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var script = (BlockScript)target;
        
        if (GUILayout.Button("Update Text"))
        {
            script.UpdateText();
            Canvas.ForceUpdateCanvases();
        }
    }
}
#endif
