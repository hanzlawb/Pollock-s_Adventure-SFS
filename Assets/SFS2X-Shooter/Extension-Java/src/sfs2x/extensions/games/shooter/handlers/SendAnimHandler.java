package sfs2x.extensions.games.shooter.handlers;

import com.smartfoxserver.v2.entities.Room;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;
import com.smartfoxserver.v2.extensions.BaseClientRequestHandler;

import sfs2x.extensions.games.shooter.utils.RoomHelper;
import sfs2x.extensions.games.shooter.utils.UserHelper;

public class SendAnimHandler extends BaseClientRequestHandler {
	
	@Override
	  public void handleClientRequest(User u, ISFSObject data) {
	    SFSObject sFSObject = new SFSObject();
	    sFSObject.putUtfString("msg", data.getUtfString("msg"));
	    sFSObject.putInt("id", u.getId());
	    Room currentRoom = RoomHelper.getCurrentRoom(this);
	    send("anim", (ISFSObject)sFSObject, UserHelper.getRecipientsList(currentRoom, u));
	  }
	}
