﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GorillaNetworking;
using Utilla.Models;

namespace Utilla
{
	public class GamemodeSelector : MonoBehaviour
	{
		const int PageSize = 4;

		ModeSelectButton[] modeSelectButtons = Array.Empty<ModeSelectButton>();

		static Material buttonMaterial = Resources.Load<Material>("objects/treeroom/materials/plastic");
		Text gamemodesText;

		int page;

		public void Initialize(Transform parent, Transform buttonParent, Transform gamemodesList)
		{
			transform.parent = parent;

			var buttons = Enumerable.Range(0, PageSize).Select(x => buttonParent.GetChild(x));
			modeSelectButtons = buttons.Select(x => x.GetComponent<ModeSelectButton>()).ToArray();

			gamemodesText = gamemodesList.gameObject.GetComponent<Text>();
            gamemodesText.lineSpacing = 1.06f * 1.2f;
            gamemodesText.transform.localScale *= 0.85f;
            gamemodesText.transform.position += gamemodesText.transform.right * 0.05f;
            gamemodesText.horizontalOverflow = HorizontalWrapMode.Overflow;

			CreatePageButtons(buttons.First().gameObject);

			ShowPage(0);
		}

		void CreatePageButtons(GameObject templateButton)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.SetActive(false);
			MeshFilter meshFilter = cube.GetComponent<MeshFilter>();

			GameObject CreatePageButton(string text, Action onPressed)
			{
				GameObject button = GameObject.Instantiate(templateButton);
				button.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
				button.GetComponent<Renderer>().material = buttonMaterial;
				button.transform.parent = templateButton.transform.parent;
				button.transform.localRotation = templateButton.transform.localRotation;
				button.transform.localScale = Vector3.one * 0.1427168f; // shouldn't hurt anyone for now 

				if (button.transform.childCount != 0) button.transform.GetChild(0).gameObject.SetActive(true);
				Text buttonText = button.GetComponentInChildren<Text>();
				if (buttonText != null)
				{
					buttonText.text = text;
					buttonText.transform.localScale = Vector3.Scale(buttonText.transform.localScale, new Vector3(2, 2, 1));
				}

				GameObject.Destroy(button.GetComponent<ModeSelectButton>());
				button.AddComponent<PageButton>().onPressed += onPressed;

				if (!button.GetComponentInParent<Canvas>())
				{
					Canvas canvas = button.transform.parent.gameObject.AddComponent<Canvas>();
					canvas.renderMode = RenderMode.WorldSpace;
				}

				return button;
			}

			GameObject nextPageButton = CreatePageButton("-->", NextPage);
			nextPageButton.transform.localPosition = new Vector3(-0.575f, nextPageButton.transform.position.y, nextPageButton.transform.position.z);

			GameObject previousPageButton = CreatePageButton("<--", PreviousPage);
			previousPageButton.transform.localPosition = new Vector3(-0.575f, -0.318f, previousPageButton.transform.position.z);

			Destroy(cube);
		}
		public void NextPage()
		{
			if (page < GamemodeManager.Instance.PageCount - 1)
			{
				ShowPage(page + 1);
			}
		}

		public void PreviousPage()
		{
			if (page > 0)
			{
				ShowPage(page - 1);
			}
		}

		void ShowPage(int page)
		{
			this.page = page;
			List<Gamemode> currentGamemodes = GamemodeManager.Instance.Gamemodes.Skip(page * PageSize).Take(PageSize).ToList();

			int counter = 0;
			for (int i = 0; i < modeSelectButtons.Length; i++)
			{
				if (i < currentGamemodes.Count)
				{
					modeSelectButtons[i].enabled = true;
					modeSelectButtons[i].gameMode = currentGamemodes[i].GamemodeString;
				}
				else
				{
					modeSelectButtons[i].enabled = false;
					modeSelectButtons[i].gameMode = "";
					counter++;
				}
			}

			string displayText = string.Join("\n", currentGamemodes.Select(x => x.DisplayName));
			for (int i = 0; i < counter; i++)
			{
				displayText += '\n';
			}
			gamemodesText.text = displayText;

			GorillaComputer.instance.OnModeSelectButtonPress(GorillaComputer.instance.currentGameMode, GorillaComputer.instance.leftHanded);
		}
	}
}
