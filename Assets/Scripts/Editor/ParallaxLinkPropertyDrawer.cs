using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ParallaxLink))]
public class ParallaxLinkPropertyDrawer : PropertyDrawer
{
    (Rect, Rect) CalculateRect(Rect position)
    {
        float bcgSize = 100f;
        float depSize = 10f;
        Rect bcgR = new Rect(position.x, position.y, bcgSize, position.height);
        Rect depR = new Rect(position.x + bcgSize + depSize, position.y, position.width - (bcgSize + depSize), position.height);
        return (bcgR, depR);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        label.text = property.FindPropertyRelative("background").name.ToString();
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        (Rect background, Rect dependent) = CalculateRect(position);

        EditorGUI.PropertyField(background, property.FindPropertyRelative("background"), GUIContent.none, true);
        EditorGUI.PropertyField(dependent, property.FindPropertyRelative("linked"), GUIContent.none, true);
        
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();

//        base.OnGUI(position, property, label);
    }
}