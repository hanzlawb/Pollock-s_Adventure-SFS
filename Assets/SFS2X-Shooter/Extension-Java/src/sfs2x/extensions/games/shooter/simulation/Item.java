package sfs2x.extensions.games.shooter.simulation;

import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;

// Item class describes an item on the scene

public class Item {
	  public static final double touchDistance = 2.0D;   // How close must the player approach to the item to take it
	  
	  private int id;   // The unique id of the item
	  
	  private Transform transform;  // The position and rotation of the item in the world
	  
	  private ItemType itemType;  // The type of the item (ammo or healthpack)
	  
	  public Item(int id, Transform transform, ItemType itemType) {
	    this.id = id;
	    this.transform = transform;
	    this.itemType = itemType;
	  }
	  
	  public ItemType getItemType() {
	    return this.itemType;
	  }
	  
 // Put the item info to JSON object to send it to client
	  public void toSFSObject(ISFSObject data) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", this.id);
	    sFSObject.putUtfString("type", this.itemType.toString());
	    this.transform.toSFSObject((ISFSObject)sFSObject);
	    data.putSFSObject("item", (ISFSObject)sFSObject);
	  }
	  
	  public boolean isClose(Transform newTransform) {
	    double d = newTransform.distanceTo(this.transform);
	    return (d <= 1.0D);
	  }
	}
