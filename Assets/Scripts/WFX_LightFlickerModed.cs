using UnityEngine;
using System.Collections;

/**
 *	Rapidly sets a light on/off.
 *	
 *	(c) 2015, Jean Moreno
**/

[RequireComponent(typeof(Light))]
public class WFX_LightFlickerModed : MonoBehaviour
{
	public float time = 0.05f;
	

	void Start ()
	{
		Invoke("Lighting", 0);
	}
	
	void Lighting()
	{	
		GetComponent<Light>().enabled = !GetComponent<Light>().enabled;

        Invoke("CutLight", time);
    }

	void CutLight()
	{
        GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
    }
}
