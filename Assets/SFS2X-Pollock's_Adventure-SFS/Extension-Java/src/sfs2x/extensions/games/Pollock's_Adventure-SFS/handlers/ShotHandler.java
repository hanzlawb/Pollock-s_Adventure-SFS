package sfs2x.extensions.games.shooter.handlers;

import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.extensions.BaseClientRequestHandler;

import sfs2x.extensions.games.shooter.utils.RoomHelper;


//This request is sent when player shoots
public class ShotHandler extends BaseClientRequestHandler {
	
	@Override
	  public void handleClientRequest(User u, ISFSObject data) {
	    int enemyHit = data.getInt("target").intValue();
	    RoomHelper.getWorld(this).processShot(u, enemyHit);
	  }
	}
