using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostingController : MonoBehaviour
{
    public float MaxBoostGauge = 100f;

    public float CurrentBoostPower { get; private set; }
    public float CurrentBoostGauge { get; private set; }

    public float BoostSpentPerSecond = 20f;
    public float BoostGaugeRecoveryRate = 10f;

    public float BoostPower = 1.5f;

    private PlayerInput _input;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        CurrentBoostGauge = MaxBoostGauge;
    }

    private void Update()
    {
        if (_input.IsBoosting)
        {
            if (CurrentBoostGauge > 0f)
            {
                CurrentBoostGauge = Mathf.Max(0f, CurrentBoostGauge - Time.deltaTime * BoostSpentPerSecond);
            }

            CurrentBoostPower = CurrentBoostGauge > 0f? BoostPower : 0f;
        }
        else
        {
            CurrentBoostPower = 0f;
            // recovery
            CurrentBoostGauge = Mathf.Min(MaxBoostGauge, CurrentBoostGauge + BoostGaugeRecoveryRate * Time.deltaTime);
        }
    }
}
