package sfs2x.extensions.games.shooter.simulation;

import com.smartfoxserver.v2.entities.User;

import sfs2x.extensions.games.shooter.ShooterExtension;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Random;

//The main World server model. Contains players, items, and all the other needed world objects
public class World {
	   private static final int maxSpawnedItems = 6;  // Maximum items that can be spawned at once
	// World bounds - to create random transforms
	  public static final double minX = -35.0D;	  
	  public static final double maxX = 35.0D;  
	  public static final double minZ = -70.0D;	  
	  public static final double maxZ = 5.0D;
	  
	  private static Random rnd = new Random();	  
	  private int itemId = 0;   // Item id counter - to generate unique ids
	  
	  private ShooterExtension extension;   // Reference to the server extension
	  
	  private List<CombatPlayer> players = new ArrayList<>();  	// Players
	  
	  private List<Item> items = new ArrayList<>();  	// Items
	  
	  public World(ShooterExtension extension) {
	    this.extension = extension;
	    rnd.setSeed((new Date()).getTime());
	  }
	  
	  public List<CombatPlayer> getPlayers() {
	    return this.players;
	  }
	  
	// Spawning new items
	  public void spawnItems() {
			int itemsCount = rnd.nextInt(maxSpawnedItems);

			int healthItemsCount = itemsCount / 2;
			int hc = 0;
			extension.trace("Spawn " + itemsCount + " items.");

			for (int i = 0; i < itemsCount; i++) {
				ItemType itemType = (hc++ < healthItemsCount) ? ItemType.HealthPack : ItemType.Ammo;
				if (hasMaxItems(itemType)) {
					continue;
				}

		        if (itemType == ItemType.Ammo) {
			          Item item = new Item(this.itemId++, Transform.randomAmmoItem(), itemType);
			          this.items.add(item);
			          this.extension.clientInstantiateItem(item);
			        } else if (itemType == ItemType.HealthPack) {
			          Item item = new Item(this.itemId++, Transform.randomHealthItem(), itemType);
			          this.items.add(item);
			          this.extension.clientInstantiateItem(item);
			        }  
			}
	  }
	  
	  private boolean hasMaxItems(ItemType itemType) {
	    int counter = 0;
	    for (Item item : this.items) {
	      if (item.getItemType() == itemType)
	        counter++; 
	    } 
	    return (counter > 3);
	  }
	  
	// Add new player if he doesn't exist, or resurrect him if he already added
	  public boolean addOrRespawnPlayer(User user, int playerPrefab, int playerColor) {
	    CombatPlayer player = getPlayer(user);
	    if (player == null) {
	      player = new CombatPlayer(user);
	      this.players.add(player);
	      player.prefab = playerPrefab;
	      player.color = playerColor;
	      this.extension.clientInstantiatePlayer(player);
	      return true;
	    } 
	    player.resurrect();
	    this.extension.clientInstantiatePlayer(player);
	    return false;
	  }
	  
	// Trying to move player. If the new transform is not valid, returns null
	  public Transform movePlayer(User u, Transform newTransform) {
	    CombatPlayer player = getPlayer(u);
	    if (isValidNewTransform(player, newTransform)) {
	      player.getTransform().load(newTransform);
	      checkItem(player, newTransform);
	      return newTransform;
	    } 
	    return null;
	  }
	  
	  // Check the player intersection with item - to pick it up
	  private void checkItem(CombatPlayer player, Transform newTransform) {
	    byte b;
	    int i;
	    Object[] arrayOfObject;
	    for (i = (arrayOfObject = this.items.toArray()).length, b = 0; b < i; ) {
	      Object itemObj = arrayOfObject[b];
	      Item item = (Item)itemObj;
	      if (item.isClose(newTransform)) {
	        try {
	          useItem(player, item);
	        } catch (Throwable e) {
	          this.extension.trace(new Object[] { "Exception using item " + e.getMessage() });
	        } 
	        return;
	      } 
	      b++;
	    } 
	  }
	  
	// Applying the item effect and removing the item from World
	  private void useItem(CombatPlayer player, Item item) {
	    if (item.getItemType() == ItemType.Ammo) {
	      if (player.hasMaxAmmoInReserve())
	        return; 
	      player.addAmmoToReserve(18);
	      this.extension.clientUpdateAmmo(player);
	    } else if (item.getItemType() == ItemType.HealthPack) {
	      if (player.hasMaxHealth())
	        return; 
	      player.addHealth(1);
	      this.extension.clientUpdateHealth(player);
	    } 
	    this.extension.clientRemoveItem(item, player);
	    this.items.remove(item);
	  }
	  
	  public Transform getTransform(User u) {
	    CombatPlayer player = getPlayer(u);
	    return player.getTransform();
	  }
	  
	  private boolean isValidNewTransform(CombatPlayer player, Transform newTransform) {
			// Check if the given transform is valid in terms of collisions, speed hacks etc
			// In this example, the server will always accept a new transform from the client

	    return true;
	  }
	  
	// Gets the player corresponding to the specified SFS user
	  private CombatPlayer getPlayer(User u) {
	    for (CombatPlayer player : this.players) {
	      if (player.getSfsUser().getId() == u.getId())
	        return player; 
	    } 
	    return null;
	  }
	  
	// Process the shot from client
	  public void processShot(User fromUser, int enemyHit) {
	    CombatPlayer player = getPlayer(fromUser);
	    if (player.isDead())
	      return; 
	    if (player.getWeapon().getAmmoCount() <= 0)
	      return; 
	    if (!player.getWeapon().isReadyToFire())
	      return; 
	    player.getWeapon().shoot();
	    this.extension.clientUpdateAmmo(player);
	    this.extension.clientEnemyShotFired(player);
	    for (CombatPlayer pl : this.players) {
	      if (pl != player)
	        if (pl.getSfsUser().getId() == enemyHit) {
	          playerHit(player, pl);
	          return;
	        }  
	    } 
	  }
	  
	// Performing reload
	  public void processReload(User fromUser) {
	    CombatPlayer player = getPlayer(fromUser);
	    if (player.isDead())
	      return; 
	    if (player.getAmmoReserve() == 0)
	      return; 
	    if (!player.getWeapon().canReload())
	      return; 
	    player.reload();
	    this.extension.clientReloaded(player);
	    this.extension.clientUpdateAmmo(player);
	  }
	  
	  // Applying the hit to the player.
	// Processing the health and death
	  private void playerHit(CombatPlayer fromPlayer, CombatPlayer pl) {
		if (pl.isDead())
			      return; 
	    pl.removeHealth(1);
	    if (pl.isDead()) {
	      fromPlayer.addKillToScore();   // Adding frag to the player if he killed the enemy
	      this.extension.updatePlayerScore(fromPlayer);
	      this.extension.clientKillPlayer(pl, fromPlayer);
	    } else {
	      this.extension.clientUpdateHealth(pl);  // Updating the health of the hit enemy
	    } 
	  }
	  
		// When user lefts the room or disconnects - removing him from the players list 
	  public void userLeft(User user) {
	    CombatPlayer player = getPlayer(user);
	    if (player == null)
	      return; 
	    this.players.remove(player);
	  }
	}
