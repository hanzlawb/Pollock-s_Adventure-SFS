using System;
using UnityEngine;
using UnityEngine.UI;
using Sfs2X.Entities;

/**
 * Script attached to Game List Item prefab.
 */
public class GameListItem : MonoBehaviour
{
	public Button playButton;
	public Text nameText;
	public Text detailsText;

	public int roomId;

	/**
	 * Initialize the prefab instance.
	 */
	public void Init(Room room)
	{
		nameText.text = room.Name;
		roomId = room.Id;

		SetState(room);
	}

	/**
	 * Update prefab instance based on the corresponding Room state.
	 */
	public void SetState(Room room)
	{
		int playerSlots = room.MaxUsers - room.UserCount;

		// Set player count and spectator count in game list item
		detailsText.text = String.Format("Available player slots: {0}", playerSlots);

		// Enable/disable game play button
		playButton.interactable = playerSlots > 0;
	}
}
