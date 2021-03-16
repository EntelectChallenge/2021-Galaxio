package Models;

import java.util.*;

public class GameState {

    public World world;
    public List<GameObject> gameObjects;
    public List<GameObject> playerGameObjects;

    public GameState() {
        world = new World();
        gameObjects = new ArrayList<GameObject>();
        playerGameObjects = new ArrayList<GameObject>();
    }

    public GameState(World world , List<GameObject> gameObjects, List<GameObject> playerGameObjects) {
        this.world = world;
        this.gameObjects = gameObjects;
        this.playerGameObjects = playerGameObjects;
    }

    public World getWorld() {
        return world;
    }

    public void setWorld(World world) {
        this.world = world;
    }

    public List<GameObject> getGameObjects() {
        return gameObjects;
    }

    public void setGameObjects(List<GameObject> gameObjects) {
        this.gameObjects = gameObjects;
    }

    public List<GameObject> getPlayerGameObjects() {
        return playerGameObjects;
    }

    public void setPlayerGameObjects(List<GameObject> playerGameObjects) {
        this.playerGameObjects = playerGameObjects;
    }

}
