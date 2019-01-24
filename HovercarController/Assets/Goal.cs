using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1<<other.gameObject.layer) & _targetLayerMask) != 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
