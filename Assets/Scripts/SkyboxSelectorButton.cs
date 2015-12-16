using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class SkyboxSelectorButton : MonoBehaviour
{
    public int thisSkyboxIndex;
    public GameObject skyboxes;

	void Start ()
    {
        GetComponent<Button>().onClick.AddListener(chooseSkybox);
    }

    void chooseSkybox()
    {
        for (int skyboxIndex = 0; skyboxIndex < skyboxes.transform.childCount; skyboxIndex++)
        {
            bool enable = false;
            if (skyboxIndex == thisSkyboxIndex)
            {
                enable = true;
            }

            skyboxes.transform.GetChild(skyboxIndex).gameObject.SetActive(enable);
        }
    }
}
