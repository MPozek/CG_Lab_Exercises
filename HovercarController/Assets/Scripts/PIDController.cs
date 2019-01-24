
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PIDController
{
	//Our PID coefficients for tuning the controller
	[FormerlySerializedAs("pCoeff"), SerializeField] private float _pCoeff = .8f;
    [FormerlySerializedAs("iCoeff"), SerializeField] private float _iCoeff = .0002f;
    [FormerlySerializedAs("dCoeff"), SerializeField] private float _dCoeff = .2f;
    [FormerlySerializedAs("minimum")] private float _minimum = -1;
    [FormerlySerializedAs("maximum")] private float _maximum = 1;
    
	private float _integral;
	private float _lastProportional;

	//We pass in the value we want and the value we currently have, the code
	//returns a number that moves us towards our goal
	public float Seek(float seekValue, float currentValue)
	{
		float deltaTime = Time.fixedDeltaTime;
		float proportional = seekValue - currentValue;

		float derivative = (proportional - _lastProportional) / deltaTime;
		_integral += proportional * deltaTime;
		_lastProportional = proportional;

		//This is the actual PID formula. This gives us the value that is returned
		float value = _pCoeff * proportional + _iCoeff * _integral + _dCoeff * derivative;
		value = Mathf.Clamp(value, _minimum, _maximum);

		return value;
	}
}