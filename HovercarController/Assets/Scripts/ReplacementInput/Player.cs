using UnityEngine;

namespace ReplacementInput
{
    public class Player
    {
        public bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }

        public float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        public bool GetButton(string buttonName)
        {
            return Input.GetButton(buttonName);
        }
    }
}
