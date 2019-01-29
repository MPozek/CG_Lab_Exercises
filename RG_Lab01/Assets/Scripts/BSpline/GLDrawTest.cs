using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawTest : MonoBehaviour {

    public Material mat;

    public System.Action ToDraw;

    private void OnPostRender()
    {
        GL.PushMatrix();

        mat.SetPass(0);

        if (ToDraw != null)
            ToDraw();

        GL.PopMatrix();
    }
}
