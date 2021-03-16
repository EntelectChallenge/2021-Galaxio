using System;
using System.Collections.Generic;
using Domain.Models;

namespace Engine.Interfaces
{
    public interface IWorldObjectGenerationService
    {
        List<GameObject> GenerateWorldFood(List<GameObject> placedFood, List<int> playerSeeds, List<GameObject> gameObjects);
        List<GameObject> GeneratePlayerStartingFood(List<int> playerSeeds, List<GameObject> gameObjects);
        List<int> GeneratePlayerSeeds(List<BotObject> players);
        List<Tuple<GameObject, GameObject>> GenerateWormholes(List<GameObject> stateGameObjects, int wormholeSeed);
        Tuple<List<GameObject>, int> GenerateGasClouds(List<GameObject> gameObjects, int gasCloudSeed);
        Tuple<List<GameObject>, int> GenerateAsteroidFields(List<GameObject> gameObjects, int asteroidFieldSeed);
    }
}