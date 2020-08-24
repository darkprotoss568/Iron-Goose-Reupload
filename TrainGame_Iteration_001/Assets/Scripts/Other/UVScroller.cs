using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScroller : MonoBehaviour
{
	public GameObject meshGO;

	public Vector2 scrollSpeed = new Vector2(0, 0);

	private Vector2 currScroll = new Vector2(0, 0);

	private MeshRenderer mr;

	void Start()
	{
		mr = meshGO.GetComponent<MeshRenderer>();
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		currScroll += (scrollSpeed * Time.deltaTime);
		mr.material.SetTextureOffset("_MainTex", currScroll);
	}
}
