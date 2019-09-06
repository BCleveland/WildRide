using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	[Tooltip("All menu roots.")]
	[SerializeField] List<Menu> m_Menus = null;
	[Tooltip("The currently active menu.")]
	[SerializeField] string m_currentMenu = null;
	[Tooltip("The menu root that will be lauched first on startup.")]
	[SerializeField] string m_launchMenu = null;

	public static MenuController _instance = null;

	private void Start()
	{
		foreach (var item in m_Menus)
		{
			item.gameObject.SetActive(false);
		}
		ChangeMenu(m_launchMenu);
		Time.timeScale = 1.0f;

		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			DestroyImmediate(this);
		}
	}


	public void ChangeMenu(string menu)
	{
		if (m_Menus.Exists(a => a.name == m_launchMenu))
		{
			m_Menus.Find(a => a.name == m_currentMenu).gameObject.SetActive(false);
			m_Menus.Find(a => a.name == menu).gameObject.SetActive(true);
			m_currentMenu = menu;
		}
		else
		{
			Debug.Log("Misspelled Menu name at Menu.ActivateMenu - " + this.gameObject.name
				+ ". Please check spelling of " + menu + "(menu) and the menu you are attempting to request.");
		}
	}

	public void ChangeScenes(string scene)
	{
		SceneManager.LoadScene(scene);
	}
}
