using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FloatingObject))]
public class WaterFX : MonoBehaviour {

    /// <summary>
    /// Determines if water particles should be generated.
    /// </summary>
    [HideInInspector]
    public bool emit = true;

    public enum EmissionPriority
    {
        Random, HighestVelocity, HighestForce, RandomForceWeighted, RandomVelocityWeighted
    }
    [Tooltip("Determines which points of the mesh will emit water particles.")]
    public EmissionPriority emissionPriority = EmissionPriority.RandomForceWeighted;

    [Tooltip("Render queue of the WaterFX material.")]
    public int renderQueue = 2700;

    [Tooltip("Velocity object has to have to emit particles.")]
    public float sleepTresholdVelocity = 0.5f;
    private int maxParticles;
    [Tooltip("Time in seconds after which the particle will be destroyed")]
    public float particleLifetime = 3f;
    [Tooltip("Maximum particle size a particle can achieve.")]
    public float maxParticleSize = 4f;
    [Tooltip("How many particles will be emitted every cycle?")]
    public int emitPerCycle = 6;
    [Range(0, 0.5f)]
    [Tooltip("Particles will be emitted every emitTimeInterval seconds.")]
    public float emitTimeInterval = 0.08f;
    [Tooltip("How hight above the water will particles be emitted?")]
    public float surfaceElevation = 0.04f;
    [Tooltip("Determines how much velocity of the object will affect initial particle speed.")]
    public float initialVelocityModifier = 0.36f;

    public AnimationCurve alphaOverTime = null;
    public AnimationCurve sizeOverTime = null;
    public AnimationCurve velocityOverTime = null;

    [Tooltip("Limit initial alpha to this value.")]
    public float maxInitialAlpha = 1f;
    [Tooltip("Multiplies initial alpha by this value. Alpha cannot be higher than maxInitialAlpha.")]
    public float initialAlphaModifier = 1f;

    [Tooltip("Foam textures. At leas one has to be assigned. Textures will be picked randomly.")]
    public List<Texture2D> foamTextures = new List<Texture2D>();

    private float timeElapsed = 0f;
    private FloatingObject fo;
    private int frameCount = 0;

    public WaterParticle[] waterParticles;
    private GameObject particleContainer;
    private int pIndex;
    private int foamTexCount;

    private WaterLine line;
    private List<WaterLine> waterLineSorted;
    private Vector3 vel, localVel;
    private WaterParticle wp;

    void Start()
    {
        maxParticles = (int)((1f / emitTimeInterval) * emitPerCycle * particleLifetime);
        waterParticles = new WaterParticle[maxParticles];

        CheckCurves();

        pIndex = 0;

        GameObject masterContainer = GameObject.Find("WaterFX Particles");
        if(masterContainer == null)
        {
            masterContainer = new GameObject();
            masterContainer.name = "WaterFX Particles";
        }

        fo = GetComponent<FloatingObject>();
        particleContainer = new GameObject();
        particleContainer.transform.parent = masterContainer.transform;
        particleContainer.name = "Water Particles (" + fo.gameObject.name + ")";

        foamTexCount = foamTextures.Count;
        if (foamTexCount == 0) Debug.LogWarning("No foam textures assigned to object " + gameObject.name + ". Will not emit any particles");
    }

    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        frameCount++;

        // Check for frame skip
        if (emit && timeElapsed >= emitTimeInterval && fo.waterLines.Count > 0f && foamTexCount > 0)
        {
            frameCount = 0;
            timeElapsed = 0;

            int emitted = 0;
            int triedToEmit = 0;

            if (emissionPriority == EmissionPriority.HighestVelocity || emissionPriority == EmissionPriority.RandomVelocityWeighted)
            {
                waterLineSorted = fo.waterLines.OrderByDescending(p => p.tri.velocityMagTimesDot).ToList();
            }
            else
            {
                waterLineSorted = fo.waterLines.OrderByDescending(p => p.tri.dynamicForce.magnitude * p.tri.dotNormalVelocityNormal).ToList();
            }

            WaterLine fastestLine = waterLineSorted[0];

            // Check if object should emit
            if (fastestLine.tri.velocityMagnitude > sleepTresholdVelocity && fo.IsTouchingWater && fo.enabled)
            {
                int pointCount = fo.waterLines.Count;

                // Check if emit per frame valid
                if (emitPerCycle >= pointCount)
                    emitPerCycle = Mathf.Max(0, pointCount - 1);

                // If random weighted, take the first quater of the descending list
                if (emissionPriority == EmissionPriority.RandomVelocityWeighted || emissionPriority == EmissionPriority.RandomForceWeighted)
                {
                    int cnt = pointCount / 4;
                    if (cnt < emitPerCycle) cnt = emitPerCycle;
                    if (cnt < waterLineSorted.Count)
                    {
                        waterLineSorted = waterLineSorted.Take(cnt).ToList();
                    }                  
                }

                // Emit allowed number of particles
                while (emitted < emitPerCycle)
                {
                    // Controls
                    if (emitted >= pointCount) break;
                    if (triedToEmit >= pointCount) break;

                    // Get emit position
                    line = null;

                    // Get one point from water line
                    if (emissionPriority == EmissionPriority.Random)
                    {
                        line = fo.waterLines[Random.Range(0, fo.waterLines.Count)];
                    }
                    else if (emissionPriority == EmissionPriority.RandomVelocityWeighted)
                    {
                        line = waterLineSorted[Random.Range(0, waterLineSorted.Count)];
                    }
                    else if (emitted < waterLineSorted.Count)
                    {
                        line = waterLineSorted[emitted];
                    }

                    // Create new particle
                    if (line != null && line.tri.velocityMagnitude > sleepTresholdVelocity)
                    {
                        WaterParticle wp = new WaterParticle();

                        // Determine position
                        Vector3 p = line.p0;
                        p -= line.tri.velocity * Time.deltaTime;

                        // Setup particle
                        GameObject particleGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        Destroy(particleGo.GetComponent<MeshCollider>());
                        float scale = maxParticleSize;
                        particleGo.transform.localScale = new Vector3(scale, scale, scale);
                        particleGo.transform.rotation = Quaternion.Euler(90f, 0f, Random.Range(0f, 360f));
                        particleGo.transform.parent = particleContainer.transform;
                        MeshRenderer meshRenderer = particleGo.GetComponent<MeshRenderer>();
                        p.y = surfaceElevation + fo.WaterHeightFunction(p.x, p.z);
                        particleGo.transform.position = p;

                        // Mesh
                        Material matInstance = new Material(Shader.Find("WaterFX/WaterParticle"));
                        matInstance.SetTexture("_MainTex", foamTextures[Random.Range(0, foamTextures.Count)]);
                        meshRenderer.material = matInstance;
                        meshRenderer.material.renderQueue = renderQueue;
                        meshRenderer.enabled = true;

                        // Particle dynamics
                        vel = line.tri.normal * line.tri.velocityMagnitude;
                        localVel = fo.transform.InverseTransformVector(vel);
                        localVel.z = 0f;
                        vel = fo.transform.TransformVector(localVel);
                        vel.y = 0f;
                        wp.initialVelocity = vel * initialVelocityModifier;
                        wp.initialAlpha = Mathf.Clamp(line.tri.dynamicForce.magnitude * 0.033f * initialAlphaModifier, 0f, maxInitialAlpha);

                        // Init
                        wp.initialColor = Color.white;
                        wp.initialColor.a = wp.initialAlpha;
                        wp.initialScale = particleGo.transform.localScale;
                        wp.go = particleGo;
                        wp.t = particleGo.transform;
                        wp.mr = meshRenderer;
                        wp.timeToLive = particleLifetime;
                        wp.mat = meshRenderer.material;
                        wp.initTime = Time.time;

                        // Start from the beginning of the array, replacing the oldest particle
                        if (pIndex >= maxParticles - 1) pIndex = 0;

                        if (waterParticles[pIndex] != null) Destroy(waterParticles[pIndex].go);
                        waterParticles[pIndex] = wp;
                        pIndex++;
                        emitted++;
                    }

                    triedToEmit++;
                }
            }
        }

        // Update existing particles
        for (int i = waterParticles.Length - 1; i >= 0; i--)
        {           
            wp = waterParticles[i];
            if (wp != null)
            {
                float timeLived = Time.time - wp.initTime;
                if (timeLived >= wp.timeToLive)
                {
                    Destroy(wp.go);
                    waterParticles[i] = null;
                }
                else
                {
                    float livedPercent = timeLived / wp.timeToLive;

                    wp.t.position += Time.deltaTime * velocityOverTime.Evaluate(livedPercent) * wp.initialVelocity;
                    wp.t.localScale = wp.initialScale * sizeOverTime.Evaluate(livedPercent);

                    float currentAlpha = alphaOverTime.Evaluate(livedPercent);
                    if (currentAlpha < 0.05f && livedPercent > 0.4f)
                    {
                        Destroy(wp.go);
                        waterParticles[i] = null;
                    }
                    else
                    {
                        wp.mr.material.SetColor(
                            "_TintColor",
                            new Color(wp.initialColor.r, wp.initialColor.g, wp.initialColor.b, wp.initialColor.a * currentAlpha)
                            );
                    }    
                }
            }
        }

    }

    /// <summary>
    /// Generate curves if they are not set.
    /// </summary>
    void CheckCurves()
    {
        if (alphaOverTime.keys.Length < 2)
        {
            Keyframe[] ks = new Keyframe[] {
                    new Keyframe(0, 0.5f),
                    new Keyframe(0.05f, 1f),
                    new Keyframe(1f, 0f)
                    };
            alphaOverTime = new AnimationCurve(ks);
        }

        if (sizeOverTime.keys.Length < 2)
        {
            Keyframe[] ks = new Keyframe[] {
                    new Keyframe(0, 0.3f),
                    new Keyframe(1f, 1f),
                    };
            sizeOverTime = new AnimationCurve(ks);
        }

        if(velocityOverTime.keys.Length < 2)
        {
            Keyframe[] ks = new Keyframe[] {
                    new Keyframe(0f, 1f),
                    new Keyframe(0.2f, 0.5f),
                    new Keyframe(1f, 0f),
                    };
            velocityOverTime = new AnimationCurve(ks);
        }
    }
}
