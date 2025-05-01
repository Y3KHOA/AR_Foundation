using UnityEngine;
using UnityEngine.UI;

namespace UISwitcher {
	public class Example : MonoBehaviour {
		[Header("Case 1: Switcher True/False")]
		[SerializeField] private UISwitcher switcher1;
		[SerializeField] private Text valueText1;

		[Header("Case 2: Switcher True/False/Null")]
		[SerializeField] private UISwitcher switcher2;
		[SerializeField] private Text valueText2;

		[Header("Case 3: Switcher Custom Colors")]
		[SerializeField] private UISwitcher switcher3;
		[SerializeField] private Text onText;
		[SerializeField] private Text offText;

		[Header("Case 4: Different Styles")]
		[SerializeField] private UISwitcher switcher4;
		[SerializeField] private Text valueText4;

		private void Awake() {
			//You can use UnityEvents
			switcher1.onValueChanged.AddListener(OnValueChanged1);
			switcher2.onValueChangedNullable.AddListener(OnValueChanged2);

			//You can also use event Action
			switcher3.OnValueChanged += OnValueChanged3;
			switcher4.OnValueChangedNullable += OnValueChanged4;

			//You can Set and get isOn value
			valueText1.text = switcher1.isOn.ToString();

			//You can also Set and get isOnNullable value
			switcher4.isOnNullable = true;

			valueText2.text = switcher2.isOnNullable.ToString();
		}

		private void OnValueChanged1(bool isOn) {
			valueText1.text = isOn.ToString();

			if (switcher1.isOn)
				valueText1.color = Color.green;
			else
				valueText1.color = Color.red;
		}

		private void OnValueChanged2(bool? isOn) {
			if (isOn.HasValue)
				valueText2.text = isOn.Value.ToString();
			else
				valueText2.text = "Null";
		}

		private void OnValueChanged3(bool isOn) {
			onText.enabled = isOn;
			offText.enabled = !isOn;
		}

		private void OnValueChanged4(bool? obj) {
			// at first you need to check if isOn has value. 
			if (obj.HasValue) 
			//if property HasValue is true. get value using property -> Value
				valueText4.text = obj.Value ? "On" : "Off";
			else
			//if has value is false current state is not defined 
				valueText4.text = "Not defined";
		}
	}
}