using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlaneCreation : MonoBehaviour
{
    const float minRaiseAmount = 0.005f;
    const float maxRaiseAmount = 1f;
    const float minBlurAmount = 1;
    const float maxBlurAmount = 10;

    [Header("General settings")]
    public int vertexSize = 200;
    public float size = 0.5f;
    [Range(minRaiseAmount, maxRaiseAmount)] public float raiseAmount = 0.05f;
    [Range(minBlurAmount, maxBlurAmount)] public int blurAmount = 4;
    public Material mat;

    [Header("UI")]
    public Slider raiseAmountSlider;
    public Slider blurSlider;

    GameObject meshObj;
    MeshFilter meshFilter;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    int triangleIndex = 0;

    void Start()
    {
        CreatePlane();
        AssignSliders();
    }

    private void Update()
    {
        if(Input.GetMouseButton(1))
            RaiseGround(true);
        if (Input.GetMouseButton(0))
            RaiseGround(false);
    }

    void RaiseGround(bool isUpward)
    {
        if (IsPointerOverUIObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, int.MaxValue))
        {
            RaiseGroundMesh(hit.point, raiseAmount * ((isUpward) ? 1 : -1), blurAmount);
        }
    }

    void RaiseGroundMesh(Vector3 position, float amount, int blur)
    {
        int x, y;
        SnapOnMesh(position, out x, out y);
        BlurredRaise(x, y, blur, amount);

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void BlurredRaise(int xPos, int yPos, int blur, float amount)
    {
        int vertexIndexMain = yPos * vertexSize + xPos;
        vertices[vertexIndexMain].y -= amount;
        float modifier = amount / blur;

        for (int y = yPos - blur; y < yPos + blur; y++) 
        {
            for (int x = xPos - blur; x < xPos + blur; x++)
            {
                int vertexIndex = y * vertexSize + x;
                if (y < 0 || x < 0 || y > vertexSize || x > vertexSize || vertexIndexMain == vertexIndex)
                    continue;

                int blurDegree = Mathf.Max(Mathf.Abs(x - xPos), Mathf.Abs(y - yPos));
                vertices[vertexIndex].y -= amount - modifier * blurDegree;
            }
        }
    }

    void SnapOnMesh(Vector3 position, out int x, out int y)
    {
        x = vertexSize - Mathf.CeilToInt(position.x / size);
        y = Mathf.CeilToInt(position.z / size);
    }

    #region MeshCreation
    void CreatePlane()
    {
        mesh = CreateMesh();
        meshObj = new GameObject();
        meshObj.transform.localRotation = Quaternion.Euler(0, 0, 180);
        meshObj.transform.position = new Vector3(vertexSize * size, 0, 0);
        meshObj.AddComponent<MeshRenderer>().material = mat;
        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshObj.AddComponent<MeshCollider>();
    }

    Mesh CreateMesh()
    {
        vertices = new Vector3[vertexSize * vertexSize];
        triangles = new int[(vertexSize - 1) * (vertexSize - 1) * 6];
        triangleIndex = 0;

        for (int y = 0; y < vertexSize; y++)
        {
            for (int x = 0; x < vertexSize; x++)
            {
                int vertexIndex = y * vertexSize + x;
                vertices[vertexIndex] = new Vector3(x, 0, y) * size;

                if (x < vertexSize - 1 && y < vertexSize - 1)
                {
                    WriteTriangle(vertexIndex, vertexIndex + vertexSize + 1, vertexIndex + vertexSize);
                    WriteTriangle(vertexIndex, vertexIndex + 1, vertexIndex + vertexSize + 1);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void WriteTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }
    #endregion

    #region UI
    void AssignSliders()
    {
        raiseAmountSlider.minValue = minRaiseAmount;
        raiseAmountSlider.maxValue = maxRaiseAmount;

        blurSlider.minValue = minBlurAmount;
        blurSlider.maxValue = maxBlurAmount;

        raiseAmountSlider.value = raiseAmount;
        blurSlider.value = blurAmount;

        if (raiseAmountSlider != null)
            raiseAmountSlider.onValueChanged.AddListener(delegate { raiseAmount = raiseAmountSlider.value; });

        if (blurSlider != null)
            blurSlider.onValueChanged.AddListener(delegate { blurAmount = Mathf.RoundToInt(blurSlider.value); });
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    } 
    #endregion
}
