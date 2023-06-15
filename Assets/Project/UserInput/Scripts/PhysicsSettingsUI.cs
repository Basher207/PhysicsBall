using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace PhysicsBall
{
    /// <summary>
    /// Simple script for passing variables from UI
    /// </summary>
    public class PhysicsSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider airResistanceSlider;
        [SerializeField] private Slider frictionDragSlider;
        [SerializeField] private Slider propagateWaveSlider;
        [SerializeField] private Slider timeScaleSlider;

        [SerializeField] private TextMeshProUGUI equationText;
        
        [SerializeField] private BPhysicsSimulator bPhysicsSimulator;
        [SerializeField] private BPhysicsSurface bPhysicsSurface;

        [SerializeField] private float maxAirResistance = 100f;
        [SerializeField] private float maxFrictionDrag = 100f;
        [SerializeField] private float maxPropagationSpeed = 100f;

        private void Awake()
        {
            airResistanceSlider.onValueChanged.AddListener(OnAirResistanceChange);
            frictionDragSlider.onValueChanged.AddListener(OnFrictionDragChange);
            propagateWaveSlider.onValueChanged.AddListener(OnPropagateWaveSliderChange);
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleSliderChange);

            OnAirResistanceChange(airResistanceSlider.value);
            OnFrictionDragChange(propagateWaveSlider.value);
            OnPropagateWaveSliderChange(propagateWaveSlider.value);
            OnTimeScaleSliderChange(timeScaleSlider.value);
        }

        private void Update()
        {
            if (Mathf.Abs(bPhysicsSurface.WavePropagationOffset) < 0.01f)
            {
                equationText.text = $"Y = 0.3*SIN(3*SQRT((X-5)2+(Z-5)2  )) + 0.5*COS(X+Z)";
            }
            else
            {
                string propagationOffset = bPhysicsSurface.WavePropagationOffset.RoundAndFormat(2);
                equationText.text = $"Y = 0.3*SIN({propagationOffset}+3*SQRT((X-5)2+(Z-5)2  )) + 0.5*COS(X+Z)";
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnAirResistanceChange(float normalisedValue)
        {
            bPhysicsSimulator.AirDrag = maxAirResistance * airResistanceSlider.value;
        }

        private void OnFrictionDragChange(float normalisedValue)
        {
            bPhysicsSimulator.FrictionDrag = maxFrictionDrag * frictionDragSlider.value;
        }

        private void OnPropagateWaveSliderChange(float normalisedValue)
        {
            bPhysicsSurface.SetWavePropagationSpeed(normalisedValue * maxPropagationSpeed);
        }

        private void OnTimeScaleSliderChange(float normalisedValue)
        {
            if (Mathf.Abs(1f - normalisedValue) < 0.1f)
            {
                normalisedValue = 1f;
                timeScaleSlider.value = normalisedValue;
            }

            if (normalisedValue > 1f)
            {
                Time.timeScale = Mathf.Lerp(1f, 20f, normalisedValue - 1f);
            }
            else
            {
                Time.timeScale = normalisedValue;
            }
        }
    }
}