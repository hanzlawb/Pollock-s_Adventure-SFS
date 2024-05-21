package sfs2x.extensions.games.shooter.simulation;

import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;

// Player class representing an individual soldier in the world simulation
public class CombatPlayer {
	  public static final int maxHealth = 3;   // Maximum amount of health for a player
	  private static final int defaultHitHP = 1; // Amount of damage done per shot that hits
	  
	  public static final int maxAmmoReserve = 18;   // Maximum amount of ammo that can be held in reserve inventory
	  	  
	  private User sfsUser;   // SFS user that corresponds to this player
	  
	  private Weapon weapon;   // Weapon of this player
	  
	  private Transform transform;    // Transform of the player that is synchronized with clients
	  
	  private int health = 3;	  
	  private int score = 0;  
	  public int prefab = 0;  
	  public int color = 0;	  
	  private int ammoReserve = maxAmmoReserve;
	  
	  public boolean isDead() {
	    return (this.health <= 0);
	  }
	  
	  public void removeHealth(int count) {
	    this.health -= count;
	  }
	  
	  public void hit() {
	    removeHealth(defaultHitHP);
	  }
	  
	  public User getSfsUser() {
	    return this.sfsUser;
	  }
	  
	  public Transform getTransform() {
	    return this.transform;
	  }
	  
	  public CombatPlayer(User sfsUser) {
	    this.sfsUser = sfsUser;
	    this.weapon = new Weapon();
	    this.transform = Transform.random();
	  }
	  
	  public void toSFSObject(ISFSObject data) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putInt("id", this.sfsUser.getId());
	    sFSObject.putInt("score", this.score);
	    sFSObject.putInt("prefab", this.prefab);
	    sFSObject.putInt("color", this.color);
	    this.transform.toSFSObject((ISFSObject)sFSObject);
	    data.putSFSObject("player", (ISFSObject)sFSObject);
	  }
	  
	  public double getX() {
	    return this.transform.getX();
	  }
	  
	  public double getY() {
	    return this.transform.getY();
	  }
	  
	  public double getZ() {
	    return this.transform.getZ();
	  }
	  
	  public int getHealth() {
	    return this.health;
	  }
	  
	// Restore player stats when he's respawning
	  public void resurrect() {    
	    this.health = maxHealth;
	    this.weapon.resetAmmo();
	    this.ammoReserve = maxAmmoReserve;
	    this.transform = Transform.random();
	  }
	  
	  public void addKillToScore() {
	    this.score++;
	  }
	  
	  public Weapon getWeapon() {
	    return this.weapon;
	  }
	  
	// Reload the weapon
	  public void reload() {
	    if (this.ammoReserve == 0)
	      return; 
	    if (this.weapon.isFullyLoaded())
	      return; 
	    int ammoUsedInReload = this.weapon.reload(this.ammoReserve);
	    this.ammoReserve -= ammoUsedInReload;
	  }
	  
	  public int getAmmoReserve() {
	    return this.ammoReserve;
	  }
	  
	// Add more ammo (when player gets ammo item)
	  public void addAmmoToReserve(int i) {
	    this.ammoReserve += i;
	    if (this.ammoReserve > maxAmmoReserve)
	      this.ammoReserve = maxAmmoReserve; 
	  }
	  
	// Add more health (when player gets health item)
	  public void addHealth(int i) {
	    this.health += i;
	    if (this.health > maxHealth)
	      this.health = maxHealth; 
	  }
	  
	  public boolean hasMaxAmmoInReserve() {
	    return (this.ammoReserve == maxAmmoReserve);
	  }
	  
	  public boolean hasMaxHealth() {
	    return (this.health == maxHealth);
	  }
	  
	  public int getScore() {
	    return this.score;
	  }
	}
