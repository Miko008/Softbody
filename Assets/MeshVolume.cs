using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshVolume : MonoBehaviour
{
    public static MeshVolume Obj;

    public bool doSave = true;
    public int saveInterval = 500;
    int saveCounter;


    public float originalVolume, presentVolume;
    Mesh mesh;
    MeshCollider collider;
    

    public float springForce = 20f;
	public float damping = 8f;
    public float timeCorrection = 5f;
    public float brushSize = 1f;

	Vector3[] originalVertices, displacedVertices, vertexVelocities;

	float uniformScale = 1f;

    void Start()
    {
        Obj = this;
        SaveSystem.Init(saveInterval, "save_");
        saveCounter = 0;

        mesh = GetComponent<MeshFilter>().sharedMesh;
        collider = GetComponent<MeshCollider>();
        originalVolume = VolumeOfMesh(mesh);
        Debug.Log("The volume of the mesh is " + originalVolume + " cube units.");


		originalVertices = mesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
		vertexVelocities = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        if (saveCounter == saveInterval && doSave)
        {
            SaveSystem.Save();
            saveCounter = 0;
        }
        presentVolume = VolumeOfMesh(mesh);
		uniformScale = transform.localScale.x;
        float changedVolume = presentVolume/originalVolume;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i, changedVolume);
		}
		mesh.vertices = displacedVertices;
		mesh.RecalculateNormals();
        collider.sharedMesh = mesh;
        SaveSystem.AddRow(displacedVertices);
        saveCounter++;
    }

    string verticesPositions()
    {
        string positions = "";
        foreach (Vector3 vert in mesh.vertices)
        {
            positions += vert.ToString() + ";";
        }
        return positions + "\n";
    }

    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }


    float VolumeOfMesh(Mesh mesh)
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

        return Mathf.Abs(volume);
    }


	void UpdateVertex (int i, float changedVolume) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * springForce * Mathf.Pow(changedVolume,3) * Time.deltaTime / timeCorrection;
		velocity *= 1f - (damping/(1f+Mathf.Pow(changedVolume-1f,2))) * Time.deltaTime / timeCorrection ;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * ((Time.deltaTime / timeCorrection) / uniformScale);
	}

	public void AddDeformingForce (Vector3 point, float force) {
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force);
		}
	}

	void AddForceToVertex (int i, Vector3 point, float force) {
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + Mathf.Pow(pointToVertex.sqrMagnitude, 2) / brushSize);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}

    public void ResetMesh()
    {
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			vertexVelocities[i] = Vector3.zero;
            displacedVertices[i] = originalVertices[i];
		}
		mesh.vertices = displacedVertices;
		mesh.RecalculateNormals();
        collider.sharedMesh = mesh;
        SaveSystem.ResetRows();
        saveCounter = 0;
        if(Time.timeScale == 0)
            Time.timeScale = 1;
    }
}
