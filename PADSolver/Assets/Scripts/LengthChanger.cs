using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LengthChanger : MonoBehaviour {

	[SerializeField]
	private Text _numberTextBox;
	[SerializeField]
	private Slider _slider;

	void Update()
	{
		_numberTextBox.text = ((int)_slider.value).ToString();
	}

}
