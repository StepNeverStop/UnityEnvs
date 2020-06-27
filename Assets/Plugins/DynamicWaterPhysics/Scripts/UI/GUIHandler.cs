using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIHandler : MonoBehaviour {

    public Changer changer;
    public Text speedText;
    public Text rudderText;
    public bool reset = false;

    private void Update()
    {
        Rigidbody shipRb = Changer.shipGo.GetComponent<Rigidbody>();
        if (shipRb != null)
        {
            speedText.text = "SPEED: " + string.Format("{0:0.0}", shipRb.velocity.magnitude * 1.95f) + "kts";
            rudderText.text = "RUDDER: " + string.Format("{0:0.0}", shipRb.GetComponent<AdvancedShipController>().rudders[0].Angle) + "°";
        }
    }

    public void ResetScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
