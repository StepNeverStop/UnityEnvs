using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Changer : MonoBehaviour {

    [SerializeField]
    public List<GameObject> ships = new List<GameObject>();
    [SerializeField]
    public List<ListWrapper> cameras = new List<ListWrapper>();

    public int activeShipID;
    public int activeCameraID;

    public static GameObject shipGo;
    public static GameObject cameraGo;

	void Start () {
        if(ships.Count == 0)
        {
            ships = GameObject.FindGameObjectsWithTag("Ship").ToList();
        }

        for(int i = 0; i < ships.Count; i++)
        {
            ListWrapper cs = new ListWrapper();
            foreach (Transform child in ships[i].transform)
            {
                if (child.CompareTag("ShipCamera") || child.CompareTag("MainCamera"))
                {
                    cs.cameras.Add(child.gameObject);
                }                
            }
            cameras.Add(cs);
        }

        cameraGo = cameras[activeShipID].cameras[activeCameraID];
        shipGo = ships[activeShipID];

        DisableAllCamerasExceptActive();
        DisableAllShipsExceptActive();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeCamera();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeShip();
        }
    }

    public void ChangeShip()
    {
        // Check if ship exists and change
        activeShipID++;
        if (ships.Count == activeShipID)
        {
            activeShipID = 0;
        }

        // Check if ship contains camera
        if (cameras[activeShipID].cameras.Count == activeCameraID)
            activeCameraID = 0;

        // Update game object
        shipGo = ships[activeShipID];

        DisableAllCamerasExceptActive();
        DisableAllShipsExceptActive();
    }

    public void ChangeCamera()
    {
        activeCameraID++;
        if (cameras[activeShipID].cameras.Count == activeCameraID)
        {
            activeCameraID = 0;
        }
        cameraGo = cameras[activeShipID].cameras[activeCameraID];
        DisableAllCamerasExceptActive();
    }

    public void DisableAllCamerasExceptActive()
    {
        for(int i = 0; i < cameras.Count; i++)
        {
            for(int j = 0; j < cameras[i].cameras.Count; j++)
            {
                cameras[i].cameras[j].gameObject.SetActive(false);
                cameras[i].cameras[j].gameObject.tag = "ShipCamera";
            }
        }

        cameras[activeShipID].cameras[activeCameraID].gameObject.SetActive(true);
        cameras[activeShipID].cameras[activeCameraID].gameObject.tag = "MainCamera";
    }

    public void DisableAllShipsExceptActive()
    {
        for (int i = 0; i < ships.Count; i++)
        {
            ships[i].GetComponent<AdvancedShipController>().enabled = false;
            for (int j = 0; j < cameras[i].cameras.Count; j++) if(j != activeCameraID && i != activeShipID)
            {
                cameras[i].cameras[j].SetActive(false);
                cameras[i].cameras[j].tag = "ShipCamera";
            }
        }
        ships[activeShipID].GetComponent<AdvancedShipController>().enabled = true;
    }

    // Workaround to enable nested list serialization
    [System.Serializable]
    public class ListWrapper
    {
        [SerializeField]
        public List<GameObject> cameras = new List<GameObject>();
    }
}
