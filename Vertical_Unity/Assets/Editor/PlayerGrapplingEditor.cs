using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerGrapplingHandler))]
public class PlayerGrapplingEditor : Editor
{
    /*public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerGrapplingHandler script = (PlayerGrapplingHandler)target;

        GUILayout.Space(10);

        script.useRealisticMomentum = EditorGUILayout.Toggle("Use Realistic Momentum", script.useRealisticMomentum);

        if (script.useRealisticMomentum)
        {
            script.tractionForce = EditorGUILayout.FloatField("Traction Force", script.tractionForce);
            script.tractionAirDensity = EditorGUILayout.FloatField("Traction Air Density", script.tractionAirDensity);
        }
        else
        {
            script.maxTractionSpeed = EditorGUILayout.FloatField("Max Traction Speed", script.maxTractionSpeed);
            script.tractionAcceleration = EditorGUILayout.FloatField("Traction Acceleration", script.tractionAcceleration);
        }

    }*/
}
