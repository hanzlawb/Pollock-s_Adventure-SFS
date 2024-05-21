package sfs2x.extensions.games.shooter.handlers;

import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.extensions.BaseClientRequestHandler;

import sfs2x.extensions.games.shooter.utils.RoomHelper;

//The reload request to reload the weapon
public class ReloadHandler extends BaseClientRequestHandler {
	
	@Override
	  public void handleClientRequest(User u, ISFSObject data) {
	    RoomHelper.getWorld(this).processReload(u);
	  }
	}
