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
		orbSprites.Add(blueOrb);
		orbSprites.Add(lightOrb);
		orbSprites.Add(darkOrb);
		orbSprites.Add(greenOrb);
		orbSprites.Add(healthOrb);
	}

	public void OnPointerClick(PointerEventData data)
	{
		Debug.Log("clicked");
		NextImage();
	}
	void OnMouseDown()
	{
		Debug.Log("mouse down");
		RaycastHit2D hit;
		hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);

		if (hit.collider.gameObject == this.gameObject)
		{
			Debug.Log("hit");
			NextImage();
		}
	}
	
	void NextImage()
	{
		index++;
		render.sprite = orbSprites[index];
	}

}
