using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NWH;
using System.Linq;

public partial class FloatingObject : MonoBehaviour
{
    public bool DEBUG = false;

    /// <summary>
    /// Target rigidbody that all forces are applied to.
    /// </summary>
    [SerializeField]
    public Rigidbody rb = null;

    [SerializeField]
    private Transform t = null;

    // Meshes
    public Mesh originalMesh = null;

    public Mesh dummyMesh = null;

    [SerializeField]
    private int[] dummyMeshTris;

    [SerializeField]
    private Vector3[] dummyMeshVerts;

    [SerializeField]
    private int dummyMeshTriCount;

    [SerializeField]
    private int dummyMeshVertCount;

    [SerializeField]
    private bool previewDummyMesh = false;

    [SerializeField]
    private bool simplifyMesh = false;

    [SerializeField]
    private bool convexMesh = false;

    /// <summary>
    /// List containing all underwater triangles and their simulation data.
    /// </summary>
    public BuoyTri[] underwaterTris;
    public int underwaterTriCount;

    /// <summary>
    /// List containing triangle edges that are at the surface of the water. 
    /// Together they represent the water line of the object.
    /// </summary>
    public List<WaterLine> waterLines = new List<WaterLine>();

    [SerializeField]
    private float simplificationRatio = 0.1f;

    private bool isTouchingWater = false;

    /// <summary>
    /// Multiplier for the forces resulting from the velocity component parallel to the triangle normals.
    /// </summary>
    [SerializeField]
    public float dynamicForceFactor = 1f;

    /// <summary>
    /// Density of the fluid object is in. Affects buoyant force.
    /// </summary>
    [SerializeField]
    public float fluidDensity = 1030f;

    /// <summary>
    /// Delegate function for retrieving water height.
    /// </summary>
    public GetWaterHeightAtPoint WaterHeightFunction = null;
    public delegate float GetWaterHeightAtPoint(float x, float z);

    /// <summary>
    /// Game object representing water.
    /// </summary>
    public GameObject water;

    // Do not recalculate mesh data and forces if object is stationary
    public bool simulate = true;
    public float sleepPositionTreshold = 0.001f;
    public float sleepAngleTreshold = 0.003f;
    public bool sleep = false;
    private Vector3 rbSleepPosition;
    private Vector3 rbSleepUp;
    private WaterFX waterEffects = null;

    private Vector3 meshSize;
    [SerializeField]
    private float meshVolume;
    [SerializeField]
    private float materialDensity;
    [SerializeField]
    private float materialMass;
    [SerializeField]
    public bool sleepEnabled = true;

    private Vector3[] objVerticesGlobal;
    private float[] distancesToSurface;

    // Avoid GC
    private VertexData vd0, vd1, vd2;
    private Vector3 H, M, L, I_M, I_L, J_M, J_H;
    private float h_H, h_M, h_L;
    private int M_index;
    private VertexData[] sortedData = new VertexData[3];
    private float waterY;
    private Vector3 globalPos;

    List<GameObject> wgos = new List<GameObject>();

    private void Start()
    {
        sleep = false;

        // Check if the values have alread been set and assign them if not.
        if (originalMesh == null)
        {
            originalMesh = GetComponent<MeshFilter>().sharedMesh;
        }

        if (dummyMeshTriCount == 0)
        {
            dummyMesh = originalMesh;

            // Get mesh vars to prevent garbage
            dummyMeshTris = dummyMesh.triangles;
            dummyMeshVerts = dummyMesh.vertices;
            dummyMeshTriCount = dummyMeshTris.Length;
            dummyMeshVertCount = dummyMeshVerts.Length;
        }

        underwaterTris = new BuoyTri[dummyMeshTriCount];
        for(int i = 0; i < underwaterTris.Length; i++)
        {
            underwaterTris[i] = new BuoyTri();
        }


        if (rb == null) rb = GetComponent<Rigidbody>();
        if (t == null) t = transform;

        meshSize = GetComponent<MeshRenderer>().bounds.size;

        wgos = GameObject.FindGameObjectsWithTag("Water")
            .OrderBy(x => Vector3.Distance(this.transform.position, x.transform.position)).ToList();

        // No water objects found, dont simulate
        if (wgos.Count == 0)
        {
            simulate = false;
        }
        // Water object found
        else
        {
            water = wgos[0];

            // Try to get Water Interface
            WaterInterface wi = null;
            if (wi = water.GetComponent<WaterInterface>())
            {
                WaterHeightFunction = wi.GetWaterHeightAtLocation;
            }
            // No water interface, use transform position for water height
            else
            {
                WaterHeightFunction = GetWaterHeightFlatSurface;
            }
        }

        // Check for water effects script
        waterEffects = GetComponent<WaterFX>();

        // Initial position
        rbSleepPosition = rb.position;
        rbSleepUp = rb.transform.up;

        // Check for dummy meshes with too large complexity
        if (dummyMeshVertCount > 1000)
            Debug.LogWarning("No need for this large a dummy mesh (>1000 vertices). Use mesh simplification for better performance. Object: " + gameObject.name);

        // Pre-generate to avoid garbage
        objVerticesGlobal = new Vector3[dummyMeshVertCount];
        distancesToSurface = new float[dummyMeshVertCount];

        // Avoid get_Item() overhead
        vd0 = new VertexData();
        vd1 = new VertexData();
        vd2 = new VertexData();
    }

    void FixedUpdate()
    {
        if(water != null)
        {
            // Do not try to simulate if mesh is out of range of water.
            float distanceToWater = (transform.position.y - WaterHeightFunction(transform.position.x, transform.position.z)) * 0.6f;

            if (distanceToWater < meshSize.x || distanceToWater < meshSize.y || distanceToWater < meshSize.z)
                simulate = true;
            else
                simulate = false;

            // If simulation enabled
            if (simulate)
            {
                // Determine if object should go into sleep
                float distanceDiff = Vector3.Distance(rb.position, rbSleepPosition);
                float angleDiff = Vector3.Angle(rb.transform.up, rbSleepUp);

                if (sleepEnabled)
                {
                    if ((distanceDiff < sleepPositionTreshold && angleDiff < sleepAngleTreshold))
                    {
                        sleep = true;

                        // Deactivate water effects
                        if (waterEffects != null)
                            waterEffects.emit = false;
                    }
                    else
                    {
                        sleep = false;
                        rbSleepPosition = rb.position;
                        rbSleepUp = rb.transform.up;
                        if (waterEffects != null) waterEffects.emit = true;
                    }
                }
                else
                {
                    sleep = false;
                }

                if (!sleep)
                {
                    underwaterTris = GenerateSplitMesh();
                }

                // Check if any triangles under water
                // TODO - might be problematic as Length is always > 0
                if (underwaterTriCount > 0)
                    isTouchingWater = true;
                else
                    isTouchingWater = false;

                // Apply triangle forces
                for(int i = 0; i < underwaterTriCount; i++)
                {
                    if (underwaterTris[i].distanceToSurface < 0)
                    {
                        rb.AddForceAtPosition(underwaterTris[i].resultantForce, underwaterTris[i].center);
                    }
                }
            }
            // Not simulated, assume not in water
            else
            {
                isTouchingWater = false;
            }
        }      
    }

    /// <summary>
    /// Generate simplified mesh from original mesh if simplificatio and/or convex is set to true.
    /// </summary>
    public Mesh ManipulateMesh(Mesh dummyMesh)
    {
        if (simplifyMesh)
        {
            dummyMesh = GenerateSimplifiedMesh(dummyMesh, simplificationRatio);
        }

        if (convexMesh)
        {
            dummyMesh = GenerateConvexMesh(dummyMesh);
        }
        dummyMeshTris = dummyMesh.triangles;
        dummyMeshVerts = dummyMesh.vertices;
        dummyMeshTriCount = dummyMeshTris.Length;
        dummyMeshVertCount = dummyMeshVerts.Length;
        return dummyMesh;
    }

    /// <summary>
    /// Positive for above water, negative for under. Level = 0
    /// </summary>
    /// <param name="p">Position from which distance to surface is calculated.</param>
    /// <returns></returns>
    public float GetDistanceToSurface(Vector3 p, GetWaterHeightAtPoint waterHeight)
    {
        return p.y - waterHeight(p.x, p.z);
    }

    private float GetWaterHeightFlatSurface(float x, float z)
    {        
        return water.transform.position.y;
    }

    /// <summary>
    /// Checks if the point is in the water in relation to floating object.
    /// </summary>
    /// <param name="point">Point in world coordinates.</param>
    /// <returns></returns>
    public bool PointInWater(Vector3 point)
    {
        if(GetDistanceToSurface(point, this.WaterHeightFunction) < 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Generate mesh from vertices and triangles.
    /// </summary>
    /// <param name="vertices">Array of vertices.</param>
    /// <param name="triangles">Array of triangles (indices).</param>
    /// <returns></returns>
    public Mesh GenerateMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh m = new Mesh();
        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateBounds();
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Reduces poly count of the mesh while trying to preserve features.
    /// </summary>
    /// <param name="om">Mesh to simplify.</param>
    /// <param name="ratio">Percent of the triangles the new mesh will have</param>
    /// <returns></returns>
    private Mesh GenerateSimplifiedMesh(Mesh om, float ratio)
    {
        int originalTriCount = om.triangles.Length;
        var verts = om.vertices;
        var normals = om.normals;

        MeshDecimate meshDecimate = new MeshDecimate();
        meshDecimate.ratio = ratio;
        meshDecimate.PreCalculate(om);
        meshDecimate.Calculate(om);

        Mesh sm = new Mesh();
        sm.name = "SimplifiedMeshPreview";
        sm.vertices = meshDecimate.finalVertices;
        sm.triangles = meshDecimate.finalTriangles;
        sm.normals = meshDecimate.finalNormals;

        return sm;
    }

    /// <summary>
    /// Generates mesh that consists of only triangles that are under water. Those tris that are
    /// half under water are split into triangles until the mesh corresponds to the submerged part 
    /// of the object.
    /// </summary>
    /// <param name="originalVerts"></param>
    /// <param name="originalTris"></param>
    /// <returns></returns>
    private BuoyTri[] GenerateSplitMesh()
    {
        waterLines.Clear();
        underwaterTriCount = 0;

        // Check if custom water height function set or just defaulted to GetWaterHeightFlatSurface
        if (WaterHeightFunction == GetWaterHeightFlatSurface)
        {
            waterY = water.transform.position.y;
            for (int j = 0; j < dummyMeshVertCount; j++)
            {
                globalPos = t.TransformPoint(dummyMeshVerts[j]);
                objVerticesGlobal[j] = globalPos;
                // Avoid function calls to speed things up
                distancesToSurface[j] = globalPos.y - waterY;
            }
        }
        // Custom function set
        else
        {
            for (int j = 0; j < dummyMeshVertCount; j++)
            {
                globalPos = t.TransformPoint(dummyMeshVerts[j]);
                objVerticesGlobal[j] = globalPos;
                distancesToSurface[j] = GetDistanceToSurface(globalPos, WaterHeightFunction);
            }
        }

        // Go through all the triangles
        underwaterTriCount = 0;
        for (int i = 0; i < dummyMeshTriCount; i += 3)
        {
            vd0.index = 0;
            vd0.dist = distancesToSurface[dummyMeshTris[i]];
            vd0.pos = objVerticesGlobal[dummyMeshTris[i]];

            vd1.index = 1;
            vd1.dist = distancesToSurface[dummyMeshTris[i + 1]];
            vd1.pos = objVerticesGlobal[dummyMeshTris[i + 1]];

            vd2.index = 2;
            vd2.dist = distancesToSurface[dummyMeshTris[i + 2]];
            vd2.pos = objVerticesGlobal[dummyMeshTris[i + 2]];

            // All vertices are underwater
            if (vd0.dist < 0f && vd1.dist < 0f && vd2.dist < 0f)
            {
                underwaterTris[underwaterTriCount++].Set(vd0.pos, vd1.pos, vd2.pos, this, 3);
            }
            // 1 or 2 vertices are below the water (cut triangles)
            else
            {
                // v0 > v1
                if (vd0.dist > vd1.dist)
                {
                    // v0 > v2
                    if (vd0.dist > vd2.dist)
                    {
                        // v1 > v2                  
                        if (vd1.dist > vd2.dist)
                        {
                            sortedData[0] = vd0;
                            sortedData[1] = vd1;
                            sortedData[2] = vd2;
                        }
                        // v2 > v1
                        else
                        {
                            sortedData[0] = vd0;
                            sortedData[1] = vd2;
                            sortedData[2] = vd1;
                        }
                    }
                    // v2 > v0
                    else
                    {
                        sortedData[0] = vd2;
                        sortedData[1] = vd0;
                        sortedData[2] = vd1;
                    }
                }
                // v0 < v1
                else
                {
                    // v0 < v2
                    if (vd0.dist < vd2.dist)
                    {
                        // v1 < v2
                        if (vd1.dist < vd2.dist)
                        {
                            sortedData[0] = vd2;
                            sortedData[1] = vd1;
                            sortedData[2] = vd0;
                        }
                        // v2 < v1
                        else
                        {
                            sortedData[0] = vd1;
                            sortedData[1] = vd2;
                            sortedData[2] = vd0;
                        }
                    }
                    // v2 < v0
                    else
                    {
                        sortedData[0] = vd1;
                        sortedData[1] = vd0;
                        sortedData[2] = vd2;
                    }
                }

                // Two verts below water
                if (sortedData[0].dist > 0f && sortedData[1].dist < 0f && sortedData[2].dist < 0f)
                {
                    TwoUnderWater(sortedData);
                }

                // One vert below water
                else if (sortedData[0].dist > 0f && sortedData[1].dist > 0f && sortedData[2].dist < 0f)
                {
                    OneUnderWater(sortedData);
                }
            }
        }

        return underwaterTris;
    }

    /// <summary>
    /// Two vertices of triangle are under water. Split the triangle into three parts.
    /// </summary>
    private void TwoUnderWater(VertexData[] vertexData)
    {
        vd0 = vertexData[0];
        vd1 = vertexData[1];
        vd2 = vertexData[2];

        // H is always at position 0
        H = vd0.pos;

        // Find the index of M
        M_index = vd0.index - 1;
        if (M_index < 0)
        {
            M_index = 2;
        }

        // Heights to the water
        h_H = vd0.dist;
        h_M = 0f;
        h_L = 0f;

        if (vd1.index == M_index)
        {
            M = vd1.pos;
            L = vd2.pos;

            h_M = vd1.dist;
            h_L = vd2.dist;
        }
        else
        {
            M = vd2.pos;
            L = vd1.pos;

            h_M = vd2.dist;
            h_L = vd1.dist;
        }

        I_M = ((-h_M / (h_H - h_M)) * (H - M)) + M;
        I_L = ((-h_L / (h_H - h_L)) * (H - L)) + L;

        // Add new waterline
        waterLines.Add(new WaterLine(I_L, I_M, underwaterTris[underwaterTriCount]));

        // Generate tris
        underwaterTris[underwaterTriCount++].Set(M, I_M, I_L, this, 2);
        underwaterTris[underwaterTriCount++].Set(M, I_L, L, this, 2);
    }

    /// <summary>
    /// One vertex is under water. Split the triangle into two parts.
    /// </summary>
    private void OneUnderWater(VertexData[] vertexData)
    {
        vd0 = vertexData[0];
        vd1 = vertexData[1];
        vd2 = vertexData[2];

        L = vd2.pos;

        // Find the index of H
        int H_index = vd2.index + 1;
        if (H_index > 2)
        {
            H_index = 0;
        }

        // Get heights to water
        float h_L = vd2.dist;
        float h_H = 0f;
        float h_M = 0f;

        if (vd1.index == H_index)
        {
            H = vd1.pos;
            M = vd0.pos;

            h_H = vd1.dist;
            h_M = vd0.dist;
        }
        else
        {
            H = vd0.pos;
            M = vd1.pos;

            h_H = vd0.dist;
            h_M = vd1.dist;
        }

        J_M = (-h_L / (h_M - h_L)) * (M - L) + L;
        J_H = -h_L / (h_H - h_L) * (H - L) + L;

        // Add new waterline
        waterLines.Add(new WaterLine(J_M, J_H, underwaterTris[underwaterTriCount]));

        // Generate tris
        underwaterTris[underwaterTriCount++].Set(L, J_H, J_M, this, 1);
    }

    /// <summary>
    /// Calculate signed volume of a triangle given by its vertices.
    /// </summary>
    public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    /// <summary>
    /// Calculates volume of the given mesh.
    /// </summary>
    public float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        Vector3 scale = transform.lossyScale;
        return Mathf.Abs(volume) * scale.x * scale.y * scale.z;
    }

    /// <summary>
    /// Generates convex mesh.
    /// </summary>
    public Mesh GenerateConvexMesh(Mesh mesh)
    {
        IEnumerable<Vector3> stars = mesh.vertices;
        Mesh m = new Mesh();

        m.name = "ConvexMesh";
        List<int> triangles = new List<int>();

        var vertices = stars.Select(x => new Vertex(x)).ToList();

        var result = MIConvexHull.ConvexHull.Create(vertices);
        m.vertices = result.Points.Select(x => x.ToVec()).ToArray();
        var xxx = result.Points.ToList();

        foreach (var face in result.Faces)
        {
            triangles.Add(xxx.IndexOf(face.Vertices[0]));
            triangles.Add(xxx.IndexOf(face.Vertices[1]));
            triangles.Add(xxx.IndexOf(face.Vertices[2]));
        }

        m.triangles = triangles.ToArray();
        m.RecalculateNormals();

        return m;
    }

    /// <summary>
    /// Generates mesh that has all of its vertices placed in a 3D grid. Slow.
    /// </summary>
    /// <param name="m">Original mesh.</param>
    /// <param name="resolution">Number of "buckets" vertices will be placed into. Final number of 
    /// points is resolution*resolution*resolution.</param>
    public static Mesh GenerateRasterizedMesh(Mesh m, int resolution)
    {
        return MeshUtils.RasterizeMesh(m, resolution);
    }

    private void OnDrawGizmos()
    {
        if (DEBUG)
        {
            for (int i = 0; i < underwaterTriCount; i++)
            {
                BuoyTri tri = underwaterTris[i];

                // Center
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(tri.center, 0.01f);

                // Dynamic forces 
                Gizmos.color = Color.red;
                Gizmos.DrawLine(tri.center, tri.center - tri.normal * tri.dynamicForce.magnitude * 0.15f);

                // Buoyant forces
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tri.center, tri.center + Vector3.up * tri.buoyantForce.magnitude * 0.2f);

                // Lines
                Gizmos.color = Color.white;
                Gizmos.DrawLine(tri.p1, tri.p2);
                Gizmos.DrawLine(tri.p2, tri.p3);
                Gizmos.DrawLine(tri.p3, tri.p1);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < waterLines.Count; i++)
            {
                Vector3 p0 = waterLines[i].p0;
                Vector3 p1 = waterLines[i].p1;
                Gizmos.DrawLine(p0, p1);
            }
        }
    }
}

