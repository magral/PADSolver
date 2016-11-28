using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OrbImage
{
	Blue = 0,
	Red = 1,
	Light = 2,
	Dark = 3,
	Green = 4,
	Health = 5
}
public class ScrollImage : MonoBehaviour, IPointerClickHandler {

	[SerializeField]
	private Sprite redOrb;
	[SerializeField]
	private Sprite blueOrb;
	[SerializeField]
	private Sprite lightOrb;
	[SerializeField]
	private Sprite darkOrb;
	[SerializeField]
	private Sprite greenOrb;
	[SerializeField]
	private Sprite healthOrb;
	[SerializeField]
	private OrbImage currentOrb;

	private List<Sprite> orbSprites;
	private Image render;
	private int index;

	void Awake()
	{
		currentOrb = OrbImage.Blue;
		render = GetComponent<Image>();
		index = 0;
		orbSprites = new List<Sprite>();
	}

	void Start()
	{
		orbSprites.Add(blueOrb);
		orbSprites.Add(redOrb);
		orbSprites.Add(lightOrb);
		orbSprites.Add(darkOrb);
		orbSprites.Add(greenOrb);
		orbSprites.Add(healthOrb);
	}

	public void OnPointerClick(PointerEventData data)
	{
		NextImage();
		
	}
	
	void NextImage()
	{
		index++;
		if(index >= orbSprites.Count)
		{
			index = 0;
		}
		currentOrb = (OrbImage)index;
		render.sprite = orbSprites[index];
	}

}
