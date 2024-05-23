package sfs2x.extensions.games.shooter.handlers;

import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.extensions.BaseClientRequestHandler;

import sfs2x.extensions.games.shooter.ShooterExtension;
import sfs2x.extensions.games.shooter.simulation.CombatPlayer;
import sfs2x.extensions.games.shooter.simulation.World;
import sfs2x.extensions.games.shooter.utils.RoomHelper;

public class SpawnMeHandler extends BaseClientRequestHandler {
	
	@Override
	  public void handleClientRequest(User u, ISFSObject data) {
	    World world = RoomHelper.getWorld(this);
	    int playerPrefab = data.getInt("prefab").intValue();
	    int playerColor = data.getInt("color").intValue();
	    boolean newPlayer = world.addOrRespawnPlayer(u, playerPrefab, playerColor);
	    if (newPlayer)
	    	// Send this player data about all the other players
	      sendOtherPlayersInfo(u); 
	    
	    // Instantiating more items together with spawning player
	    world.spawnItems();
	  }
	  
	// Send the data for all the other players to the newly joined client
	private void sendOtherPlayersInfo(User targetUser) {
	    World world = RoomHelper.getWorld(this);
	    for (CombatPlayer player : world.getPlayers()) {
	      if (player.isDead())
	        continue; 
	      if (player.getSfsUser().getId() != targetUser.getId()) {
	        ShooterExtension ext = (ShooterExtension)getParentExtension();
	        ext.clientInstantiatePlayer(player, targetUser);
	      } 
	    } 
	  }
	}
