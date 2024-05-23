package sfs2x.extensions.games.shooter.simulation;

// Weapon class describes the weapon user has

public class Weapon {
  public static final int maxAmmo = 6;  // Max loaded ammo
  
  private static final double shotTime = 600.0D;  // The minimum time between 2 shots
  
  private static final double reloadTime = 1300.0D;  // The time weapon is being reloaded (cannot shot this time)
  
  private int ammoCount = 6;   // Loaded Ammo
  
  private long lastShotTime = 0L;
  
  private long lastReloadTime = 0L;
  
  public Weapon() {
    resetAmmo();
  }
  
  public void resetAmmo() {
    this.ammoCount = maxAmmo;
  }
  
// Reload the weapon
  public int reload(int ammoReserve) {
    this.lastReloadTime = System.currentTimeMillis();
    int usedAmmo = maxAmmo - this.ammoCount;
    if (usedAmmo > ammoReserve)
      usedAmmo = ammoReserve; 
    this.ammoCount += usedAmmo;
    return usedAmmo;
  }
  
// Shoot from this weapon
  public void shoot() {
    if (this.ammoCount > 0) {
      this.ammoCount--;
      this.lastShotTime = System.currentTimeMillis();
    } 
  }
  
  public int getAmmoCount() {
    return this.ammoCount;
  }
  
  public boolean isFullyLoaded() {
    return (this.ammoCount >= maxAmmo);
  }
  
// Check if it's possible to shoot at this moment
  public boolean isReadyToFire() {
    if (this.ammoCount == 0)
      return false; 
    if (this.lastReloadTime + reloadTime > System.currentTimeMillis())
      return false; 
    if (this.lastShotTime + shotTime > System.currentTimeMillis())
      return false; 
    return true;
  }
  
// Check if it's possible to reload at this moment 
  public boolean canReload() {
    if (isFullyLoaded())
      return false; 
    if (this.lastReloadTime + reloadTime > System.currentTimeMillis())
      return false; 
    if (this.lastShotTime + shotTime > System.currentTimeMillis())
      return false; 
    return true;
  }
}
