using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ObjectDupe))]
[CanEditMultipleObjects]
public class ObjectDuplicate : Editor
{
    SerializedProperty Distance;
    SerializedProperty Direction;
    SerializedProperty Amount;
    SerializedProperty DeleteThisComponentOnDupe;
    SerializedProperty WorldSpace;
    void OnEnable()
    {
        Distance = serializedObject.FindProperty("dupDistance");
        Direction = serializedObject.FindProperty("Direction");
        Amount = serializedObject.FindProperty("amount");
        DeleteThisComponentOnDupe = serializedObject.FindProperty("OnDupe");
        WorldSpace = serializedObject.FindProperty("WorldSpace");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();//Updates information of the object the ObjectDupe is attached to.
        EditorGUILayout.PropertyField(DeleteThisComponentOnDupe,new GUIContent("DeleteOnDupe"));
        EditorGUILayout.PropertyField(WorldSpace, new GUIContent("WorldSpace"));
        EditorGUILayout.IntSlider(Distance, 0, 100, new GUIContent("Distance")); //Distance tweak
        EditorGUILayout.IntSlider(Amount, 0, 1000, new GUIContent("Amount"));
        EditorGUILayout.PropertyField(Direction); // Direction Selection
        ObjectDupe x = (ObjectDupe)target;// Picks the target
        if (GUILayout.Button("DupeIT!")) // Calls the Dupe function
        {
            for(int i = 1; i<Amount.intValue;i++)
            {
                Undo.RegisterCreatedObjectUndo(x.dupeObject(),"Duplicated Object");
            }
            if(Amount.intValue == 1)
            {
                Undo.RegisterCreatedObjectUndo(x.dupeObject(), "Duplicated Object");
            }
            if(DeleteThisComponentOnDupe.boolValue)
            {
                x.removeComponent();
            }
        }
        if (GUILayout.Button("Reset!"))
        {
            x.reset();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
