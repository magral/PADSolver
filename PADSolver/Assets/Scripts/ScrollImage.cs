using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OrbType
{
	Undefined = -1,
	None = 0,
	Red = 1,
	Blue = 2,
	Green = 3,
	Light = 4,
	Dark = 5,
	Health = 6
}

public class ScrollImage : MonoBehaviour, IPointerClickHandler {

	[SerializeField]
	public Sprite noOrb;
	[SerializeField]
	public Sprite redOrb;
	[SerializeField]
	private Sprite blueOrb;
	[SerializeField]
	private Sprite greenOrb;
	[SerializeField]
	private Sprite lightOrb;
	[SerializeField]
	private Sprite darkOrb;
	[SerializeField]
	private Sprite healthOrb;
	[SerializeField]
	private OrbType currentOrb;

	private List<Sprite> orbSprites;
	private Image render;
	private int index;

	public OrbType State
	{
		get { return currentOrb; }
		set { currentOrb = value; }
	}

	public Sprite OrbSprite
	{
		get { return render.sprite; }
		set { render.sprite = value; }
	}

	void Awake()
	{
		currentOrb = OrbType.None;
		render = GetComponent<Image>();
		index = 0;
		orbSprites = new List<Sprite>();
	}

	void Start()
	{
		orbSprites.Add(noOrb);
		orbSprites.Add(redOrb);
		orbSprites.Add(blueOrb);
		orbSprites.Add(greenOrb);
		orbSprites.Add(lightOrb);
		orbSprites.Add(darkOrb);
		orbSprites.Add(healthOrb);
	}

	public void OnPointerClick(PointerEventData data)
	{
		if (data.button == PointerEventData.InputButton.Left) { NextImage(); }
		else if (data.button == PointerEventData.InputButton.Right) { PrevImage(); }
		else { SetOrb(OrbType.None); }
	}
	
	void NextImage()
	{
		index++;
		if(index >= orbSprites.Count)
		{
			index = 1;
		}
		currentOrb = (OrbType)index;
		render.sprite = orbSprites[index];
	}

	void PrevImage()
	{
		index--;
		if (index <= 0)
		{
			index = orbSprites.Count - 1;
		}
		currentOrb = (OrbType)index;
		render.sprite = orbSprites[index];
	}

	public void SetOrb(OrbType newImage)
	{
		currentOrb = newImage;
		index = (int)newImage;
		switch (newImage)
		{
			case OrbType.None:
			case OrbType.Undefined:
				render.sprite = noOrb;
				break;
			case OrbType.Red:
				render.sprite = redOrb;
				break;
			case OrbType.Blue:
				render.sprite = blueOrb;
				break;
			case OrbType.Green:
				render.sprite = greenOrb;
				break;
			case OrbType.Light:
				render.sprite = lightOrb;
				break;
			case OrbType.Dark:
				render.sprite = darkOrb;
				break;
			case OrbType.Health:
				render.sprite = healthOrb;
				break;
			default:
				throw new System.NotImplementedException();
		}
	}
}
