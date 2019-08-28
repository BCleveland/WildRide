using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	[Tooltip("The currently active submenu.")]
	[SerializeField] private string m_currentSubMenu = null;

	[Tooltip("The submenu that will be lauched first on startup.")]
	[SerializeField] private string m_launchMenu = null;

	[Tooltip("All submenus childed to this menu root.")]
	[SerializeField] public List<GameObject> SubMenus = null;

	private void Start()
	{
		foreach (var item in SubMenus)
		{
			item.gameObject.SetActive(false);
		}
		TurnOnSubMenu(m_launchMenu);
	}

	/// <summary>
	/// Switches active submenu to named menu.
	/// </summary>
	/// <param name="name">Name of submenu requested</param>
	public void TurnOnSubMenu(string name)
	{
		if (SubMenus.Exists(a => a.name == name))
		{
			SubMenus.Find(a => a.name == m_currentSubMenu).SetActive(false);
			SubMenus.Find(a => a.name == name).SetActive(true);
			m_currentSubMenu = name;
		}
		else
		{
			Debug.Log("Misspelled Menu name at Menu.TurnOnSubMenu - " + this.gameObject.name 
				+ ". Please check spelling of " + name + "(name) and the menu you are attempting to request.");
		}
	}
}
