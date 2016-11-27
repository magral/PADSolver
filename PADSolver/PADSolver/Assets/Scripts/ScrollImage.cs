using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OrbImage
{
	Red,
	Green,
	Blue,
	Light,
	Dark,
	Health
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
		render = GetComponent<Image>();
		index = 0;
		orbSprites = new List<Sprite>();
		
	}

	void Start()
	{
		orbSprites.Add(redOrb);
		orbSprites.Add(lightOrb);
		orbSprites.Add(darkOrb);
		orbSprites.Add(greenOrb);
		orbSprites.Add(healthOrb);
		orbSprites.Add(blueOrb);
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
		render.sprite = orbSprites[index];
	}

}
