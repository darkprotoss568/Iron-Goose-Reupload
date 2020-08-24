using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class ObjectSwap {
    [MenuItem("GameObject/SwapObjects %l")] // Ctrl + l to swap objects
    private static void SwapObjects()
    {
        if (!Selection.activeTransform) return;
        if (Selection.gameObjects.Length == 2)
        {
            Undo.RegisterCompleteObjectUndo(Selection.gameObjects[0], "SwapObjects");
            Vector3 x = Selection.gameObjects[0].transform.position;
            Selection.gameObjects[0].transform.position = Selection.gameObjects[1].transform.position;
            Undo.RegisterCompleteObjectUndo(Selection.gameObjects[1], "SwapObjectsB");
            Selection.gameObjects[1].transform.position = x;
        }
        else
            return;
    }
}
