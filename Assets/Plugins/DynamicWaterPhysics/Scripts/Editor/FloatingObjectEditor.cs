using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWH;
using UnityEditor;

[CustomEditor(typeof(FloatingObject))]
[ExecuteInEditMode]
[CanEditMultipleObjects]
[System.Serializable]
public class FloatingObjectEditor : Editor
{
    public string originalMeshName;
    public float prevMaterialDensity;
    public bool showAdvanced = false;

    private FloatingObject fo;

    private SerializedProperty targetRigidbody;
    private SerializedProperty materialDensity;
    private SerializedProperty fluidDensity;
    private SerializedProperty dynamicForceFactor;
    private SerializedProperty doConvexifyMesh;
    private SerializedProperty doSimplifyMesh;
    private SerializedProperty simplificationRatio;
    private SerializedProperty debug;
    private SerializedProperty reuseForces;
    private SerializedProperty reusePositionTreshold;
    private SerializedProperty reuseAngleTreshold;

    private void OnEnable()
    {
        targetRigidbody = serializedObject.FindProperty("rb");
        materialDensity = serializedObject.FindProperty("materialDensity");
        fluidDensity = serializedObject.FindProperty("fluidDensity");
        dynamicForceFactor = serializedObject.FindProperty("dynamicForceFactor");
        doConvexifyMesh = serializedObject.FindProperty("convexMesh");
        doSimplifyMesh = serializedObject.FindProperty("simplifyMesh");
        simplificationRatio = serializedObject.FindProperty("simplificationRatio");
        debug = serializedObject.FindProperty("DEBUG");
        reuseForces = serializedObject.FindProperty("sleepEnabled");
        reusePositionTreshold = serializedObject.FindProperty("sleepPositionTreshold");
        reuseAngleTreshold = serializedObject.FindProperty("sleepAngleTreshold");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        FloatingObject fo = (FloatingObject)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(targetRigidbody, 
            new GUIContent("Target Rigidbody", "Rigidbody on which the forces will be applied. Must be self or parent object's rigidbody"));
        EditorGUILayout.PropertyField(materialDensity, 
            new GUIContent("Material Density", 
            "Density of the material object is made of. Anything higher than fluid density will sink. Check manual for proper use. Set to 0 to ignore."));
        EditorGUILayout.PropertyField(fluidDensity,
            new GUIContent("Fluid Density",
            "Density of the fluid this object is in. Higher density will make object tent to float more easily. Affects only buoyancy. Check 'Dynamic Force Factor' for dynamic forces"));
        EditorGUILayout.PropertyField(dynamicForceFactor,
            new GUIContent("Dynamic Force Factor",
            "Forces acting upon the object based on it's velocity."));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dummy Mesh", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(doConvexifyMesh, new GUIContent("Convexify Mesh",
            "Generate a convex mesh. This must be used if the mesh is not closed (missing one of its surfaces, e.g. only bottom of the hull has triangles)."));
        EditorGUILayout.PropertyField(doSimplifyMesh, new GUIContent("Simplify Mesh",
            "Generate a simplified mesh. 15-30 triangles is recommended for simple objects, and up to 100 for ships and similar and more complex objects. Simulation is O(n) where n is number of triangles."));
        if (fo.DoSimplifyMesh)
        {
            EditorGUILayout.PropertyField(simplificationRatio);
        }

        EditorGUILayout.Space();

        if (fo.TargetRigidbody == null) fo.TargetRigidbody = fo.GetComponent<Rigidbody>();

        if (!fo.PreviewDummyMesh || fo.OriginalMesh == null)
        {
            if (fo.GetComponent<MeshFilter>() != null)
            {
                fo.OriginalMesh = fo.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        // Check if dummy mesh set
        if (fo.DummyMesh == null)
        {
            fo.DummyMesh = fo.originalMesh;
        }

        string buttonText = fo.PreviewDummyMesh ? "End Preview" : "Preview";
        if (GUILayout.Button(buttonText))
        {
            foreach (FloatingObject t in targets)
            {
                // Preview dummy mesh button
                t.PreviewDummyMesh = !t.PreviewDummyMesh;

                if (t.PreviewDummyMesh)
                {
                    t.GetComponent<MeshFilter>().mesh = t.DummyMesh;
                }
                else
                {
                    t.GetComponent<MeshFilter>().mesh = t.OriginalMesh;
                }
            }
        }


        if (fo.MaterialDensity != prevMaterialDensity)
        {
            fo.MeshVolume = fo.VolumeOfMesh(fo.DummyMesh);
            fo.MaterialMass = fo.MeshVolume * fo.MaterialDensity;
        }

        if (GUILayout.Button("Update Dummy Mesh"))
        {
            foreach(FloatingObject t in targets)
            {
                // Update dummy mesh button
                if (t.OriginalMesh == null) t.OriginalMesh = t.GetComponent<MeshFilter>().sharedMesh;
                t.DummyMesh = t.ManipulateMesh(t.OriginalMesh);
            }
        }

        // Tri count
        if (fo.DummyMesh != null && fo.OriginalMesh != null)
        {
            EditorGUILayout.LabelField("Original: " + (fo.OriginalMesh.triangles.Length / 3) + " -> Dummy: " + (fo.DummyMesh.triangles.Length / 3) + " triangles");
        }

        if (showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Additional Options"))
        {
            EditorGUILayout.PropertyField(debug);
            EditorGUILayout.PropertyField(reuseForces);
            EditorGUILayout.PropertyField(reusePositionTreshold);
            EditorGUILayout.PropertyField(reuseAngleTreshold);
        }


        if (fo.GetComponent<Rigidbody>() != null && fo.DummyMesh != null)
        {
            if (fo.TargetRigidbody == fo.GetComponent<Rigidbody>())
            {
                if (fo.MaterialDensity > 0)
                {
                    fo.TargetRigidbody.mass = fo.MaterialMass;
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        prevMaterialDensity = fo.MaterialDensity;

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(fo);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
