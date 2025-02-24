using UnityEngine;
using System.Collections.Generic;

public class Creatura : MonoBehaviour
{
    [Header("Random Seed")]
    public int seed = 0;

    private Vector3 p0 = new Vector3(0, 0, 0);  // Starting point of curve
    private Vector3 p3 = new Vector3(0, 3, 0);  // Ending point of curve

    [SerializeField, Range(3, 64)]
    private int curveResolution = 16;  // curve resolution
    [SerializeField, Range(3, 64)]
    private int radialResolution = 16;  // Revolution resolution

    [SerializeField]
    private int crabCount = 5;
    [SerializeField]
    private float crabSpacing = 7f;

    // Claw prefabs
    public GameObject crusherClawPrefab;
    public GameObject babyClawPrefab;
    public GameObject raveClawPrefab;

    // Eye prefabs
    public GameObject eyePrefab1;
    public GameObject eyePrefab2;
    public GameObject eyePrefab3;
    public GameObject eyePrefab4;

    // Parameters for fine-tuning body width
    [SerializeField, Range(0f, 1f)]
    private float minDirectionValue = 0.3f;
    [SerializeField, Range(0f, 1f)]
    private float maxDirectionValue = 0.7f;
    [SerializeField]
    private float minXDistance = 4f;
    [SerializeField]
    private float maxXDistance = 6f;
    [SerializeField]
    private float minY = 1f;
    [SerializeField]
    private float maxY = 2f;

    void Start()
    {
        GenerateCrabs();
    }

    void GenerateCrabs()
    {
        for (int i = 0; i < crabCount; i++)
        {
            GameObject crab = new GameObject($"Crab_{i + 1}");
            crab.transform.parent = transform;
            crab.transform.localPosition = new Vector3(i * crabSpacing, 0, 0);

            // Apply a unique seed for each crab
            int crabSeed = seed + i * 1000;  // Increased offset to reduce correlation
            GenerateSingleCrab(crab, crabSeed);
        }
    }

    void GenerateSingleCrab(GameObject crab, int crabSeed)
    {
        Random.InitState(crabSeed);

        // Generate materials for this crab
        Material mainBodyMaterial = CreateMainBodyMaterial(crabSeed);
        Material undersideMaterial = CreateUndersideMaterial(crabSeed);
        Material barnacleMaterial = CreateBarnacleMaterial(crabSeed);

        // jointed legs (50% chance)
        bool useJointedLegs = Random.value < 0.5f;

        // Body generation
        GameObject body = new GameObject("Body");
        body.transform.parent = crab.transform;
        body.transform.localPosition = Vector3.zero;

        GenerateBody(body, crabSeed, mainBodyMaterial, undersideMaterial);

        // Leg generation
        GenerateLegs(crab, crabSeed, mainBodyMaterial, barnacleMaterial, useJointedLegs);

        // Claw generation
        GenerateClaws(crab, crabSeed, mainBodyMaterial);

        // Eye generation
        GenerateEyes(crab, crabSeed, mainBodyMaterial);
    }

    void GenerateBody(GameObject parent, int bodySeed, Material mainBodyMaterial, Material undersideMaterial)
    {
        Random.InitState(bodySeed + 1);  // Offset

        float direction = Random.Range(minDirectionValue, maxDirectionValue);
        if (Random.value > 0.5f)
        {
            direction *= -1f;  // Randomly flip the direction
        }

        float xDistance = Random.Range(minXDistance, maxXDistance);

        Vector3 p1 = new Vector3(
            direction * xDistance,
            Random.Range(minY, maxY),
            0
        );
        Vector3 p2 = new Vector3(
            direction * xDistance,
            Random.Range(minY, maxY),
            0
        );

        Mesh bodyMesh = CreateRevolutionMesh(p0, p1, p2, p3);

        // Top Shell
        GameObject topShellObject = new GameObject("TopShell");
        topShellObject.transform.parent = parent.transform;
        topShellObject.transform.localPosition = Vector3.zero;

        MeshFilter topShellFilter = topShellObject.AddComponent<MeshFilter>();
        topShellFilter.mesh = bodyMesh;

        MeshRenderer topShellRenderer = topShellObject.AddComponent<MeshRenderer>();
        topShellRenderer.material = mainBodyMaterial;

        float shellScaleZ = Random.Range(0.5f, 0.7f);
        float shellScaleY = Random.Range(0.2f, 0.4f);
        float shellScaleX = Random.Range(0.89f, 0.9f);
        topShellObject.transform.localScale = new Vector3(shellScaleX, shellScaleY, shellScaleZ);

        // Underside
        GameObject undersideObject = new GameObject("Underside");
        undersideObject.transform.parent = parent.transform;
        undersideObject.transform.localPosition = new Vector3(0, -0.1f, 0);

        MeshFilter undersideFilter = undersideObject.AddComponent<MeshFilter>();
        undersideFilter.mesh = bodyMesh;

        MeshRenderer undersideRenderer = undersideObject.AddComponent<MeshRenderer>();
        undersideRenderer.material = undersideMaterial;

        undersideObject.transform.localScale = new Vector3(shellScaleX, shellScaleY, shellScaleZ);
    }

    void GenerateLegs(GameObject parent, int legSeed, Material mainBodyMaterial, Material barnacleMaterial, bool useJointedLegs)
    {
        Random.InitState(legSeed + 3);  // Offset

        // Decide whether each leg has barnacles (50% chance)
        bool[] legHasBarnacles = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            legHasBarnacles[i] = Random.value < 0.5f;
        }

        // Generate control points for legs
        Vector3 p1, p2;

        if (useJointedLegs)
        {
            // p1 and p2 in opposite directions for jointed legs
            float direction1 = Random.Range(1f, 2f);
            float direction2 = -Random.Range(1f, 2f);

            p1 = new Vector3(
                direction1,
                Random.Range(0.5f, 1f),
                0
            );
            p2 = new Vector3(
                direction2,
                Random.Range(2f, 3f),
                0
            );
        }
        else
        {
            // Normal legs
            float direction = Random.Range(1f, 2f);
            p1 = new Vector3(
                direction,
                Random.Range(0.5f, 1f),
                0
            );
            p2 = new Vector3(
                direction,
                Random.Range(2f, 3f),
                0
            );
        }

        Mesh legMesh = CreateRevolutionMesh(p0, p1, p2, p3);

        // Create legs with possible barnacles
        CreateLeg(parent, "Front_Left_Leg", 20, 120, -0.7f, 0, legMesh, mainBodyMaterial, barnacleMaterial, legHasBarnacles[0], legSeed + 1, useJointedLegs, p1, p2);
        CreateLeg(parent, "Front_Right_Leg", -20, -120, 0.7f, 0, legMesh, mainBodyMaterial, barnacleMaterial, legHasBarnacles[1], legSeed + 2, useJointedLegs, p1, p2);
        CreateLeg(parent, "Back_Left_Leg", 40, 120, -0.7f, 0.5f, legMesh, mainBodyMaterial, barnacleMaterial, legHasBarnacles[2], legSeed + 3, useJointedLegs, p1, p2);
        CreateLeg(parent, "Back_Right_Leg", -40, -120, 0.7f, 0.5f, legMesh, mainBodyMaterial, barnacleMaterial, legHasBarnacles[3], legSeed + 4, useJointedLegs, p1, p2);
    }

    void CreateLeg(GameObject parent, string name, float rotationY, float rotationZ, float translateX, float translateZ, Mesh legMesh, Material mainBodyMaterial, Material barnacleMaterial, bool hasBarnacles, int legSeed, bool useJointedLegs, Vector3 p1, Vector3 p2)
    {
        Random.InitState(legSeed);

        // Create a parent GameObject for the leg
        GameObject legObject = new GameObject(name);
        legObject.transform.parent = parent.transform;
        legObject.transform.localPosition = new Vector3(translateX, 0.2f, translateZ);
        legObject.transform.localRotation = Quaternion.Euler(0, rotationY, rotationZ);

        // Create a child GameObject for the leg mesh
        GameObject legMeshObject = new GameObject("LegMesh");
        legMeshObject.transform.parent = legObject.transform;
        legMeshObject.transform.localPosition = Vector3.zero;
        legMeshObject.transform.localRotation = Quaternion.identity;
        legMeshObject.transform.localScale = Vector3.one;

        MeshFilter legFilter = legMeshObject.AddComponent<MeshFilter>();
        legFilter.mesh = legMesh;

        MeshRenderer legRenderer = legMeshObject.AddComponent<MeshRenderer>();
        legRenderer.material = mainBodyMaterial;

        // Apply scaling to the leg mesh
        float scaleX = Random.Range(0.25f, 0.35f);
        float scaleY = Random.Range(0.7f, 0.9f);
        float scaleZ = Random.Range(0.25f, 0.35f);
        legMeshObject.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        // Add barnacles if applicable
        if (hasBarnacles)
        {
            // Decide how many barnacles to generate (1 to 3)
            int barnacleCount = Random.Range(1, 4);

            for (int i = 0; i < barnacleCount; i++)
            {
                // Choose a random t
                float t = Random.Range(0.1f, 0.9f);

                // Get the point on the curve
                Vector3 pointOnCurve = CalculateBezierPoint(t, p0, p1, p2, p3);

                Vector3 worldBarnaclePosition = legMeshObject.transform.TransformPoint(pointOnCurve);

                // Randomly select primitive
                PrimitiveType barnacleType = GetRandomBarnacleType();
                GameObject barnacle = GameObject.CreatePrimitive(barnacleType);
                barnacle.name = name + "_Barnacle_" + (i + 1);
                barnacle.transform.parent = legObject.transform;
                barnacle.transform.position = worldBarnaclePosition;
                barnacle.transform.localRotation = Random.rotation;
                float barnacleScale = Random.Range(0.1f, 0.2f);
                barnacle.transform.localScale = new Vector3(
                    barnacleScale * Random.Range(0.8f, 1.2f),
                    barnacleScale * Random.Range(0.8f, 1.2f),
                    barnacleScale * Random.Range(0.8f, 1.2f)
                );

                // Flatten
                Vector3 scale = barnacle.transform.localScale;
                scale.y *= 0.3f;
                barnacle.transform.localScale = scale;

                MeshRenderer barnacleRenderer = barnacle.GetComponent<MeshRenderer>();
                barnacleRenderer.material = barnacleMaterial;
            }
        }
    }

    PrimitiveType GetRandomBarnacleType()
    {
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0:
                return PrimitiveType.Sphere;
            case 1:
                return PrimitiveType.Capsule;
            case 2:
                return PrimitiveType.Cylinder;
            case 3:
                return PrimitiveType.Cube;
            default:
                return PrimitiveType.Cube;
        }
    }

    void GenerateClaws(GameObject parent, int clawSeed, Material mainBodyMaterial)
    {
        Random.InitState(clawSeed + 7);

        // List of claw prefabs
        GameObject[] clawPrefabs = new GameObject[] { crusherClawPrefab, babyClawPrefab, raveClawPrefab };

        // Randomly select a claw prefab for the left claw
        int leftClawIndex = Random.Range(0, clawPrefabs.Length);
        GameObject leftClawPrefab = clawPrefabs[leftClawIndex];

        // Randomly select a claw prefab for the right claw
        int rightClawIndex = Random.Range(0, clawPrefabs.Length);
        GameObject rightClawPrefab = clawPrefabs[rightClawIndex];

        // Left claw positions
        Vector3[] leftClawPositions = new Vector3[]
        {
            new Vector3(-0.6f, -0.1f, -0.72f),  // Crusher
            new Vector3(-0.7f, -0.2f, -0.67f),  // Baby
            new Vector3(-0.85f, 1.65f, -0.17f)  // Rave
        };

        Vector3 leftClawPosition = leftClawPositions[leftClawIndex];

        GameObject leftClaw = Instantiate(leftClawPrefab, parent.transform);
        leftClaw.name = "Left_Claw";
        leftClaw.transform.localPosition = leftClawPosition;
        leftClaw.transform.localRotation = Quaternion.identity;

        // Randomly scale the left claw between 1x and 1.3x
        float leftClawScaleFactor = Random.Range(1f, 1.3f);
        leftClaw.transform.localScale = Vector3.one * leftClawScaleFactor;

        // Set the material of the claw to the main body material
        SetClawMaterial(leftClaw, mainBodyMaterial);

        // Get the position for the right claw
        Vector3 rightClawPosition = leftClawPositions[rightClawIndex];
        rightClawPosition.x = -rightClawPosition.x;  // Mirror the x-position

        // Instantiate right claw
        GameObject rightClaw = Instantiate(rightClawPrefab, parent.transform);
        rightClaw.name = "Right_Claw";
        rightClaw.transform.localPosition = rightClawPosition;
        rightClaw.transform.localRotation = Quaternion.identity;

        // Randomly scale the right claw between 1x and 1.3x
        float rightClawScaleFactor = Random.Range(1f, 1.3f);
        Vector3 rightClawScale = Vector3.one * rightClawScaleFactor;
        rightClawScale.x *= -1f;  // Mirror x scale to flip the claw
        rightClaw.transform.localScale = rightClawScale;

        // Set the material of the claw to the main body material
        SetClawMaterial(rightClaw, mainBodyMaterial);
    }

    void SetClawMaterial(GameObject claw, Material material)
    {
        // Get all MeshRenderers in the claw
        MeshRenderer[] renderers = claw.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = material;
        }
    }

    void GenerateEyes(GameObject parent, int eyeSeed, Material mainBodyMaterial)
    {
        Random.InitState(eyeSeed + 8);

        // List of eye prefabs
        GameObject[] eyePrefabs = new GameObject[] { eyePrefab1, eyePrefab2, eyePrefab3, eyePrefab4 };

        // Randomly select an eye prefab
        int eyeIndex = Random.Range(0, eyePrefabs.Length);
        GameObject selectedEyePrefab = eyePrefabs[eyeIndex];

        // Eye positions corresponding to each prefab
        Vector3[] eyePositions = new Vector3[]
        {
            new Vector3(0f, 0.9f, -0.5f),  // Eye Prefab 1 position
            new Vector3(0f, 1.0f, -0.55f), // Eye Prefab 2 position
            new Vector3(0f, 1.1f, -0.6f),  // Eye Prefab 3 position
            new Vector3(0f, 1.2f, -0.65f)  // Eye Prefab 4 position
        };

        // Instantiate the eyes
        GameObject eyes = Instantiate(selectedEyePrefab, parent.transform);
        eyes.name = "Eyes";

        Vector3 eyePosition = eyePositions[eyeIndex];
        eyes.transform.localPosition = eyePosition;
        eyes.transform.localRotation = Quaternion.identity;
        eyes.transform.localScale = Vector3.one;

        // Replace the specific material in the eyes with the main body material
        SetEyeMaterial(eyes, mainBodyMaterial);
    }

    void SetEyeMaterial(GameObject eyes, Material material)
    {
        string materialToReplaceName = "EyeShellMaterial";

        MeshRenderer[] renderers = eyes.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains(materialToReplaceName))
                {
                    materials[i] = material;
                }
            }
            renderer.materials = materials;
        }
    }

    Mesh CreateRevolutionMesh(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[curveResolution * radialResolution];
        int[] triangles = new int[(curveResolution - 1) * (radialResolution - 1) * 6];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < curveResolution; i++)
        {
            float t = i / (float)(curveResolution - 1);
            Vector3 pointOnCurve = CalculateBezierPoint(t, start, control1, control2, end);

            for (int j = 0; j < radialResolution; j++)
            {
                float angle = j * Mathf.PI * 2 / (radialResolution - 1);
                float x = pointOnCurve.x * Mathf.Cos(angle);
                float z = pointOnCurve.x * Mathf.Sin(angle);
                vertices[vertIndex++] = new Vector3(x, pointOnCurve.y, z);
            }
        }

        for (int i = 0; i < curveResolution - 1; i++)
        {
            for (int j = 0; j < radialResolution - 1; j++)
            {
                int a = i * radialResolution + j;
                int b = i * radialResolution + (j + 1);
                int c = (i + 1) * radialResolution + j;
                int d = (i + 1) * radialResolution + (j + 1);

                triangles[triIndex++] = a;
                triangles[triIndex++] = c;
                triangles[triIndex++] = b;

                triangles[triIndex++] = b;
                triangles[triIndex++] = c;
                triangles[triIndex++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Material CreateMainBodyMaterial(int colorSeed)
    {
        Random.InitState(colorSeed + 4);

        Material material = new Material(Shader.Find("Standard"));
        float t = Random.Range(0f, 1f);
        Color color = Color.Lerp(new Color(0.6f, 0f, 0f), new Color(1f, 0.5f, 0f), t);
        material.color = color;

        return material;
    }

    Material CreateUndersideMaterial(int colorSeed)
    {
        Random.InitState(colorSeed + 5);

        Material material = new Material(Shader.Find("Standard"));
        float t = Random.Range(0f, 1f);
        Color color = Color.Lerp(new Color(0.95f, 0.9f, 0.8f), new Color(1f, 0.6f, 0.4f), t);
        material.color = color;

        return material;
    }

    Material CreateBarnacleMaterial(int colorSeed)
    {
        Random.InitState(colorSeed + 6);

        Material material = new Material(Shader.Find("Standard"));
        float t = Random.Range(0f, 1f);

        // Barnacle color ranges from light to grungy dark with a slight green tint
        Color lightColor = new Color(0.6f, 0.7f, 0.6f);
        Color darkColor = new Color(0.2f, 0.3f, 0.2f);
        material.color = Color.Lerp(lightColor, darkColor, t);

        return material;
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Bezier curve formula
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }
}
