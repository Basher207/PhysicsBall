using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBall
{
    public static class Extentions
    {
        public static float RoundToDecimalPlace(this float number, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10.0f, decimalPlaces);
            return Mathf.Round(number * multiplier) / multiplier;
        }
        public static string RoundAndFormat(this float number, int decimalPlaces)
        {
            float roundedNumber = number.RoundToDecimalPlace(decimalPlaces);
            return roundedNumber.ToString("0.00");
        }
    }
}