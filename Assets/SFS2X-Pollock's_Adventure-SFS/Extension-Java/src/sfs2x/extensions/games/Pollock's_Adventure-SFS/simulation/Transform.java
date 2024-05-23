package sfs2x.extensions.games.shooter.simulation;

import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;

import java.util.Random;

// Transform class - keeps player or items position and rotation

public class Transform {
	  private double x;	  
	  private double y;	  
	  private double z;	  
	  private double rotx;	  
	  private double roty;	  
	  private double rotz;	  
	  private double srotx;	  
	  private double sroty;  
	  private double srotz;  
	  private long timeStamp = 0L;  // Timestamp is stored to perform interpolation on client
	  
	  private static Random rnd = new Random();
	  
	// Create random transform choosing from the predefined spawnPoints list
	  public static Transform random() {
	    Transform[] spawnPoints = getSpawnPoints();
	    int i = rnd.nextInt(spawnPoints.length);
	    return spawnPoints[i];
	  }
	  
	  public static Transform randomAmmoItem() {
	    Transform[] spawnPoints = getAmmoSpawnPoints();
	    int i = rnd.nextInt(spawnPoints.length);
	    return spawnPoints[i];
	  }
	  
	  public static Transform randomHealthItem() {
	    Transform[] spawnPoints = getHealthSpawnPoints();
	    int i = rnd.nextInt(spawnPoints.length);
	    return spawnPoints[i];
	  }
	  
	// Create random transform using the specified bounds
	  public static Transform randomWorld() {
	    double x = rnd.nextDouble() * 70.0D + -35.0D;
	    double z = rnd.nextDouble() * 75.0D + -70.0D;
	    double y = 6.0D;
	    return new Transform(x, y, z, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	  }
	  
  // Hard coded spawnPoints - where players will spawn
	  private static Transform[] getSpawnPoints() {
	    Transform[] spawnPoints = new Transform[8];
	    spawnPoints[0] = new Transform(-30.0D, 1.0D, 22.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[1] = new Transform(13.0D, 1.0D, 27.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[2] = new Transform(1.0D, 1.0D, -17.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[3] = new Transform(1.0D, 1.0D, 8.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[4] = new Transform(6.0D, 1.0D, 47.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[5] = new Transform(-5.0D, 1.0D, 47.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[6] = new Transform(0.0D, 1.0D, 24.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[7] = new Transform(0.0D, 1.0D, 37.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    return spawnPoints;
	  }
	  
	// Hard coded spawnPoints - where Ammo will spawn
	  private static Transform[] getAmmoSpawnPoints() {
	    Transform[] spawnPoints = new Transform[8];
	    spawnPoints[0] = new Transform(-14.0D, 0.1D, 3.0D, -90.0D, 0.0D, 90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[1] = new Transform(-12.0D, 0.1D, 7.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[2] = new Transform(-12.0D, 0.1D, -4.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[3] = new Transform(11.0D, 0.1D, -9.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[4] = new Transform(1.0D, 0.1D, -28.0D, -90.0D, 0.0D, -90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[5] = new Transform(-0.9D, 0.1D, -30.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[6] = new Transform(-22.0D, 0.1D, 14.0D, -90.0D, 0.0D, -90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[7] = new Transform(-20.0D, 0.1D, 38.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    return spawnPoints;
	  }
	  
	// Hard coded spawnPoints - where Health will spawn
	  private static Transform[] getHealthSpawnPoints() {
	    Transform[] spawnPoints = new Transform[8];
	    spawnPoints[0] = new Transform(-14.0D, 0.1D, 1.0D, -90.0D, 0.0D, 90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[1] = new Transform(-10.0D, 0.1D, 7.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[2] = new Transform(-10.0D, 0.1D, -4.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[3] = new Transform(9.0D, 0.1D, -9.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[4] = new Transform(3.0D, 0.1D, -28.0D, -90.0D, 0.0D, -90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[5] = new Transform(1.0D, 0.1D, -30.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[6] = new Transform(-21.0D, 0.1D, 17.0D, -90.0D, 0.0D, -90.0D, 0.0D, 0.0D, 0.0D);
	    spawnPoints[7] = new Transform(-20.0D, 0.1D, 35.0D, -90.0D, 0.0D, 0.0D, 0.0D, 0.0D, 0.0D);
	    return spawnPoints;
	  }
	  
	  public Transform(double x, double y, double z, double rotx, double roty, double rotz, double srotx, double sroty, double srotz) {
	    this.x = x;
	    this.y = y;
	    this.z = z;
	    this.rotx = rotx;
	    this.roty = roty;
	    this.rotz = rotz;
	    this.srotx = srotx;
	    this.sroty = sroty;
	    this.srotz = srotz;
	  }
	  
	  public double getRotx() {
	    return this.rotx;
	  }
	  
	  public double getRoty() {
	    return this.roty;
	  }
	  
	  public double getsRotx() {
	    return this.srotx;
	  }
	  
	  public double getsRoty() {
	    return this.sroty;
	  }
	  
	  public double getX() {
	    return this.x;
	  }
	  
	  public double getY() {
	    return this.y;
	  }
	  
	  public double getZ() {
	    return this.z;
	  }
	  
	  public void setTimeStamp(long timeStamp) {
	    this.timeStamp = timeStamp;
	  }
	  
	  public long getTimeStamp() {
	    return this.timeStamp;
	  }
	  
	  public static Transform fromSFSObject(ISFSObject data) {
	    ISFSObject transformData = data.getSFSObject("transform");
	    double x = transformData.getDouble("x").doubleValue();
	    double y = transformData.getDouble("y").doubleValue();
	    double z = transformData.getDouble("z").doubleValue();
	    double rx = transformData.getDouble("rx").doubleValue();
	    double ry = transformData.getDouble("ry").doubleValue();
	    double rz = transformData.getDouble("rz").doubleValue();
	    double srx = transformData.getDouble("srx").doubleValue();
	    double sry = transformData.getDouble("sry").doubleValue();
	    double srz = transformData.getDouble("srz").doubleValue();
	    long timeStamp = transformData.getLong("t").longValue();
	    Transform transform = new Transform(x, y, z, rx, ry, rz, srx, sry, srz);
	    transform.setTimeStamp(timeStamp);
	    return transform;
	  }
	  
	  public void toSFSObject(ISFSObject data) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putDouble("x", this.x);
	    sFSObject.putDouble("y", this.y);
	    sFSObject.putDouble("z", this.z);
	    sFSObject.putDouble("rx", this.rotx);
	    sFSObject.putDouble("ry", this.roty);
	    sFSObject.putDouble("rz", this.rotz);
	    sFSObject.putDouble("srx", this.srotx);
	    sFSObject.putDouble("sry", this.sroty);
	    sFSObject.putDouble("srz", this.srotz);
	    sFSObject.putLong("t", this.timeStamp);
	    data.putSFSObject("transform", (ISFSObject)sFSObject);
	  }
	  
	// Copy another transform to this one
	  public void load(Transform another) {
	    this.x = another.x;
	    this.y = another.y;
	    this.z = another.z;
	    this.rotx = another.rotx;
	    this.roty = another.roty;
	    this.rotz = another.rotz;
	    this.srotx = another.srotx;
	    this.sroty = another.sroty;
	    this.srotz = another.srotz;
	    setTimeStamp(another.getTimeStamp());
	  }
	  
	// Calculate distance to another transform
	  public double distanceTo(Transform transform) {
	    double dx = Math.pow(getX() - transform.getX(), 2.0D);
	    double dy = Math.pow(getY() - transform.getY(), 2.0D);
	    double dz = Math.pow(getZ() - transform.getZ(), 2.0D);
	    return Math.sqrt(dx + dy + dz);
	  }
	}
