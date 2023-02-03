using UnityEngine;
using System.Collections;


/// <summary>
/// Changes the FilterMode of supplied fonts 
/// This must be done in code as FilterMode is not exposed to fonts in the editor
/// </summary>
public class SetFontFilterMode : MonoBehaviour 
{
	public Font[] fontsToChange;
	public FilterMode filterMode;

	void OnEnable() 
	{
		for(int i = 0; i < fontsToChange.Length; i++)
		{
			fontsToChange[i].material.mainTexture.filterMode = filterMode;
		}
	}
}
