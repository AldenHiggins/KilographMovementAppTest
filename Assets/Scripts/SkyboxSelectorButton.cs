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

    public void chooseSkybox()
    {
        for (int skyboxIndex = 0; skyboxIndex < skyboxes.transform.childCount; skyboxIndex++)
        {
            bool enable = false;
            if (skyboxIndex == thisSkyboxIndex)
            {
                enable = true;
                // Access the text of this skybox
                transform.parent.GetChild(skyboxIndex).transform.GetChild(0).gameObject.GetComponent<Text>().color = new Color(0.098f, 0.388f, 0.596f);
                transform.parent.GetChild(skyboxIndex).transform.GetChild(0).gameObject.GetComponent<Outline>().effectColor = new Color(255.0f, 255.0f, 0.0f);
            }
            else
            {
                transform.parent.GetChild(skyboxIndex).transform.GetChild(0).gameObject.GetComponent<Text>().color = new Color(255.0f, 255.0f, 255.0f);
                transform.parent.GetChild(skyboxIndex).transform.GetChild(0).gameObject.GetComponent<Outline>().effectColor = new Color(0.0f, 0.0f, 0.0f);
            }

            
            skyboxes.transform.GetChild(skyboxIndex).gameObject.SetActive(enable);
        }
    }
}
