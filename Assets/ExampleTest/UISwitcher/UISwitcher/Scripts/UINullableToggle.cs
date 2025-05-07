using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace UISwitcher {

	[Serializable]
	public class ValueChangedEvent : UnityEvent<bool> {
	}

	[Serializable]
	public class NullableValueChangedEvent : UnityEvent<bool?> {
	}
	public class UINullableToggle : Selectable, IPointerClickHandler {
		public event Action<bool> OnValueChanged;
		public event Action<bool?> OnValueChangedNullable;

		public ValueChangedEvent onValueChanged = new();
		public NullableValueChangedEvent onValueChangedNullable = new();

		[SerializeField] private int m_isOnNullable;
		[SerializeField] private bool m_nullValueEnabled;

		public bool isOn {
			get {
				if (m_isOnNullable < 0) return false;
				return m_isOnNullable != 0;
			}
			set => Set(value);
		}

		public bool? isOnNullable {
			get {
				if (m_isOnNullable < 0) return null;
				if (m_isOnNullable == 0) return false;
				return true;
			}
			set => Set(value);
		}

		public void SetWithoutNotify(bool? value) =>
				Set(value, false);

		private void Set(bool? value, bool notify = true) {
			if (m_isOnNullable == NullableBoolToInt(value))
				return;

			if (!value.HasValue)
				m_isOnNullable = -1;
			else
				m_isOnNullable = value.Value ? 1 : 0;

			if (notify)
				ValueChangedNotify(value);

			OnChanged(value);
		}

		public virtual void OnChanged( /*(int value*/) =>
				OnChanged(IntToNullableBool(m_isOnNullable));

		virtual protected void OnChanged(bool? value) {}

		private void ValueChangedNotify(bool? value) {
			if (value.HasValue) {
				OnValueChanged?.Invoke(value.Value);
				onValueChanged?.Invoke(value.Value);
			}

			OnValueChangedNullable?.Invoke(value);
			onValueChangedNullable?.Invoke(value);
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (m_nullValueEnabled)
				MoveToNextValue();
			else
				MoveToBetweenTrueFalse();
		}

		private void MoveToBetweenTrueFalse() {
			if (!IsActive() || !IsInteractable())
				return;
			isOnNullable = isOnNullable switch {
					null => true,
					true => false,
					_ => true
			};
		}

		private void MoveToNextValue() {
			if (!IsActive() || !IsInteractable())
				return;

			isOnNullable = isOnNullable switch {
					null => true,
					true => false,
					_ => null
			};
		}

		private int NullableBoolToInt(bool? value) {
			if (!value.HasValue)
				return -1;
			return value.Value ? 1 : 0;
		}

		private bool? IntToNullableBool(int value) {
			if (value > 0) return true;
			if (value == 0) return false;
			return null;
		}
	}
}