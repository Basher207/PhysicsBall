using System;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace PhysicsBall
{
	/// <summary>
	/// Generates a flat mesh spiraling out from the center, getting less detailed over distance
	/// </summary>
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class MeshMaker : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;

		[SerializeField] private int radialDivisons = 50;
		[SerializeField] private int angularDivisions = 50;
		
		[SerializeField] private float startingExtents = 1f;
		[SerializeField] private float uvScale = 5;
		
		[SerializeField] private float extentsMultiPerDivision = 1f;
		
		[SerializeField] private Vector2 uvCenterOffset;

		public float UvScaling => uvScale;

		
		private void OnValidate()
		{
			meshFilter = GetComponent<MeshFilter>();
		}

		public void OnEnable()
		{
			Rebuild();
		}

		[ContextMenu("ReBuild")]
		void Rebuild()
		{
			//Destroy any old meshes already here.
			if (meshFilter.sharedMesh != null)
			{
				if (Application.isPlaying)
				{
					// This will destroy the object in game.
					Destroy(meshFilter.sharedMesh);
				}
				else
				{
					// This will destroy the object in the editor.
					DestroyImmediate(meshFilter.sharedMesh);
				}
			}

			meshFilter.sharedMesh = NewQuad(radialDivisons, angularDivisions, uvScale, uvCenterOffset ,extentsMultiPerDivision, startingExtents);
		}

		private static Mesh NewQuad(int radialDivisions, int angularDivisions, float uvScale, Vector2 uvCenterOffset, float increaseFromCenter, float extent = 1f)
		{
			int totalVertices = (radialDivisions + 1) * (angularDivisions + 1);
			Vector3[] verts = new Vector3[totalVertices];
			Vector2[] uvs = new Vector2[totalVertices];
			int[] tris = new int[radialDivisions * angularDivisions * 6];

			int triIndex = 0;

			// Loop through each radial division
			for (int r = 0; r <= radialDivisions; r++)
			{
				// Calculate the radius of this ring
				float radius = SumOfGeoSequence(extent, increaseFromCenter, r);

				// Loop through each segment in this ring
				for (int a = 0; a <= angularDivisions; a++)
				{
					// Calculate the angle of this segment
					float theta = (2 * Mathf.PI / angularDivisions) * a;

					// Convert to Cartesian coordinates
					float x = radius * Mathf.Cos(theta);
					float z = radius * Mathf.Sin(theta);

					// Store the vertex position
					int vertIndex = r * (angularDivisions + 1) + a;
					verts[vertIndex] = new Vector3(x, 0, z);
					uvs[vertIndex] = uvCenterOffset + (new Vector2(x, z) * uvScale);

					// For all but the last ring and segment
					if (r < radialDivisions && a < angularDivisions)
					{
						// Add the triangles for this segment
						tris[triIndex + 0] = vertIndex + angularDivisions + 2;
						tris[triIndex + 1] = vertIndex + angularDivisions + 1;
						tris[triIndex + 2] = vertIndex;

						tris[triIndex + 3] = vertIndex + 1;
						tris[triIndex + 4] = vertIndex + angularDivisions + 2;
						tris[triIndex + 5] = vertIndex;

						triIndex += 6;
					}
				}
			}

			Mesh newMesh = new Mesh();
			newMesh.indexFormat = IndexFormat.UInt32;

			newMesh.triangles = new int[0];
			newMesh.vertices = verts;
			newMesh.triangles = tris;
			newMesh.uv = uvs;

			//Due to the vertex shader moving vertices outside of the boundes
			//We are making them large enough so that it wouldn't get culled 
			newMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 9999999);


			return newMesh;
		}

		public static float SumOfGeoSequence(float firstTerm, float ratio, int lastTermIndex)
		{
			if (lastTermIndex < 0)
				ratio = 1f / ratio;  // Inverts the ratio for terms less than the center.
			if (Mathf.Abs(ratio - 1f) < Mathf.Epsilon)  // Handles the case where ratio is 1 to prevent division by zero.
				return lastTermIndex * firstTerm;
			return firstTerm * (1f - Mathf.Pow(ratio, lastTermIndex)) / (1f - ratio);
		}
	}
}