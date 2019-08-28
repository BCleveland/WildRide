using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicLevelObject : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI levelNameField = null;
	[SerializeField] Button playButton = null;

	public string levelName = "";
	
	public void InitializeDynamicLevelObjectLevel(string sceneName)
	{
		levelName = sceneName;
		levelNameField.text = levelName;

		playButton.onClick.AddListener(delegate { OnClickPlay(); });
	}

	public void OnClickPlay()
	{
		if (levelName != null && levelName.Trim() != "")
		{
			MenuController._instance.ChangeScenes(levelName);
		}
		else
		{
			print("Scene name null or whitespace in OnClickPlay.");
		}
	}
    
}
