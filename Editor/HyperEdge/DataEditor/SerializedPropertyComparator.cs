using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class SerializedPropertyComparator : IComparer<SerializedProperty>
{
    public int Compare(SerializedProperty x, SerializedProperty y)
    {
	return GenericCompare(x, y);
    }

    // TODO : look for a betetr way this probably generate ton of garbage + need to be extended manually
    int GenericCompare(SerializedProperty a, SerializedProperty b)
    {
	if (a.propertyType != b.propertyType)
	{
	    Debug.LogError("Couldn't compare 2 SerializedProeprty of different type");
	    return 0;
	}

	switch (a.propertyType)
	{
	    case SerializedPropertyType.AnimationCurve:
	    case SerializedPropertyType.Bounds:
	    case SerializedPropertyType.BoundsInt:
	    case SerializedPropertyType.Character:
	    case SerializedPropertyType.Color:
	    case SerializedPropertyType.ExposedReference:
	    case SerializedPropertyType.FixedBufferSize:
	    case SerializedPropertyType.Generic:
	    case SerializedPropertyType.Gradient:
	    case SerializedPropertyType.ObjectReference:
	    case SerializedPropertyType.Quaternion:
	    case SerializedPropertyType.Rect:
	    case SerializedPropertyType.RectInt:
	    case SerializedPropertyType.Vector2:
	    case SerializedPropertyType.Vector2Int:
	    case SerializedPropertyType.Vector3:
	    case SerializedPropertyType.Vector3Int:
	    case SerializedPropertyType.Vector4:
	        return 0;
	    case SerializedPropertyType.Boolean:
	        return a.boolValue.CompareTo(b.boolValue);
	    case SerializedPropertyType.Enum:
	        return a.enumValueIndex.CompareTo(b.enumValueIndex);
	    case SerializedPropertyType.Float:
	        return a.floatValue.CompareTo(b.floatValue);
	    case SerializedPropertyType.Integer:
	        return a.intValue.CompareTo(b.intValue);
	    case SerializedPropertyType.LayerMask: //really sueful to comapre layer mask int value?? 
	        return a.intValue.CompareTo(b.intValue);
	    case SerializedPropertyType.String:
	        return a.stringValue.CompareTo(b.stringValue);
	}

	return 0;
    }
}
}
