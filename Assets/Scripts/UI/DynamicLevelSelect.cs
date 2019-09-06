using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DynamicLevelSelect : MonoBehaviour
{
	[SerializeField]
	List<string> scenes = null;

	[SerializeField] DynamicLevelObject dynamicPrefab = null;
	[SerializeField] private Transform scrollViewContent = null;
	
	float scrollViewContentHeight = 0;
	float dynamicPrefabHeight = 100;

    // Start is called before the first frame update
    void Start()
    {
        
		if (scrollViewContent == null || 
			scenes.Count == 0 || 
			scenes == null || 
			dynamicPrefab == null)
		{
			print("One or more values were not set in Dynamic Level Loader on " + this.name);
			return;
		}

		scrollViewContentHeight = scrollViewContent.localScale.y;

		float endHeight = 0;

		foreach (var scene in scenes)
		{
			DynamicLevelObject newObject = Instantiate(dynamicPrefab, Vector3.zero, Quaternion.identity, scrollViewContent);
			newObject.InitializeDynamicLevelObjectLevel(scene);
			newObject.GetComponent<RectTransform>().localPosition = new Vector3(0, endHeight, 0);
			endHeight -= dynamicPrefabHeight;
		}
		
		if (Mathf.Abs(endHeight) < scrollViewContentHeight) endHeight = scrollViewContentHeight;
		scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollViewContent.GetComponent<RectTransform>().sizeDelta.x, Mathf.Abs(endHeight));

    }


}
