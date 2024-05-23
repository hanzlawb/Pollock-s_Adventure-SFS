package sfs2x.extensions.games.shooter;

import com.smartfoxserver.v2.core.SFSEventType;
import com.smartfoxserver.v2.entities.Room;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;
import com.smartfoxserver.v2.extensions.SFSExtension;

import sfs2x.extensions.games.shooter.handlers.*;
import sfs2x.extensions.games.shooter.simulation.CombatPlayer;
import sfs2x.extensions.games.shooter.simulation.Item;
import sfs2x.extensions.games.shooter.simulation.World;
import sfs2x.extensions.games.shooter.utils.RoomHelper;
import sfs2x.extensions.games.shooter.utils.UserHelper;

import java.util.List;


// The extension main class. Used to handle requests from clients and send messages back to them
//
// Requests that can be send from clients:
// - sendTransform
// - sendAnim
// - spawnMe
// - getTime
// - shot
// - reload
//
// Responses send from the extension to clients:
// - time
// - anim
// - spawnPlayer
// - transform
// - notransform
// - killed
// - health
// - score
// - ammo
// - spawnItem
// - removeItem
// - enemyShotFired
// - reloaded
//

public class ShooterExtension extends SFSExtension {

	private World world; // Reference to World simulation model

	public World getWorld() {  
		return world;
	}

	@Override
	public void init() {
	    this.world = new World(this); // Creating the world model 
	    
	    // Subscribing the request handlers
	    addRequestHandler("sendTransform", SendTransformHandler.class);
	    addRequestHandler("sendAnim", SendAnimHandler.class);
	    addRequestHandler("spawnMe", SpawnMeHandler.class);
	    addRequestHandler("getTime", GetTimeHandler.class);
	    addRequestHandler("shot", ShotHandler.class);
	    addRequestHandler("reload", ReloadHandler.class);
	    addEventHandler(SFSEventType.USER_DISCONNECT, OnUserGoneHandler.class);
	    addEventHandler(SFSEventType.USER_LEAVE_ROOM, OnUserGoneHandler.class);
	    addEventHandler(SFSEventType.USER_LOGOUT, OnUserGoneHandler.class);
	    trace(new Object[] { "Shooter extension initialized" });
	  }
	  
	  public void destroy() {
	    this.world = null;
	    super.destroy();
	  }
	  
	  // Send message to client when a player is killed
	  public void clientKillPlayer(CombatPlayer pl, CombatPlayer killerPl) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", pl.getSfsUser().getId());
	    sFSObject.putInt("killerId", killerPl.getSfsUser().getId());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("killed", (ISFSObject)sFSObject, userList);
	  }
	
	  // Send message to clients when the health value of a player is updated
	  public void clientUpdateHealth(CombatPlayer pl) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", pl.getSfsUser().getId());
	    sFSObject.putInt("health", pl.getHealth());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("health", (ISFSObject)sFSObject, userList);
	  }
	  
	// Send message to clients when the score value of a player is updated
 	  public void updatePlayerScore(CombatPlayer pl) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", pl.getSfsUser().getId());
	    sFSObject.putInt("score", pl.getScore());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("score", (ISFSObject)sFSObject, userList);
	  }
	  
 	// When new item is spawned - send message to all the clients
	  public void clientInstantiateItem(Item item) {
	    SFSObject sFSObject = new SFSObject();
	    item.toSFSObject((ISFSObject)sFSObject);
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("spawnItem", (ISFSObject)sFSObject, userList);
	  }
	  
	// When someone picked an item up, send message to all the clients, so they will remove the item from scene 
	  public void clientRemoveItem(Item item, CombatPlayer player) {
	    SFSObject sFSObject = new SFSObject();
	    item.toSFSObject((ISFSObject)sFSObject);
	    sFSObject.putInt("playerId", player.getSfsUser().getId());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("removeItem", (ISFSObject)sFSObject, userList);
	  }
	  
	// When someone has made a shot, send message to all the clients to inform about it
	  public void clientEnemyShotFired(CombatPlayer pl) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", pl.getSfsUser().getId());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("enemyShotFired", (ISFSObject)sFSObject, userList);
	  }
	  
	// When someone has reloaded the weapon, send message to all the clients to inform about it.
	  public void clientReloaded(CombatPlayer player) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", player.getSfsUser().getId());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    send("reloaded", (ISFSObject)sFSObject, userList);
	  }
	  
	// Send instantiate new player message to all the clients
	  public void clientInstantiatePlayer(CombatPlayer player) {
	    clientInstantiatePlayer(player, (User)null);
	    clientUpdateAmmo(player);
	  }
	  
	//Send the player instantiation message to all the clients or to a specified user only
	  public void clientInstantiatePlayer(CombatPlayer player, User targetUser) {
	    SFSObject sFSObject = new SFSObject();
	    player.toSFSObject((ISFSObject)sFSObject);
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    if (targetUser == null) {
	      List<User> userList = UserHelper.getRecipientsList(currentRoom);
	    // Sending to all the users
	      send("spawnPlayer", (ISFSObject)sFSObject, userList);
	    } else {
	    // Sending to the specified user
	      send("spawnPlayer", (ISFSObject)sFSObject, targetUser);
	    } 
	  }
	  
	// Send message to clients when the ammo value of a player is updated
	  public void clientUpdateAmmo(CombatPlayer player) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", player.getSfsUser().getId());
	    sFSObject.putInt("ammo", player.getWeapon().getAmmoCount());
	    sFSObject.putInt("maxAmmo", 6);
	    sFSObject.putInt("unloadedAmmo", player.getAmmoReserve());
	    send("ammo", (ISFSObject)sFSObject, player.getSfsUser());
	  }
	}
