﻿using UnityEngine;

namespace Assets.Scripts.Generation
{
    [CreateAssetMenu(menuName = "Dungeon/Settings/Generation")]
    public class GenerationSettings : ScriptableObject
    {
        [Range(0, 3)]
        public int seed = 0;
        public int roomCount = 20;
        public int gridWidth = 128;
        public int gridHeight = 128;
        public int maxRoomWidth = 25;
        public int maxRoomHeight = 25;
        public int minRoomWidth = 7;
        public int minRoomHeight = 7;
        public int maxRoomAttempts = 5;
        public int maxHallwayExpansion = 3;
        public float extraHallwayGenerationChance = 0;
    }
}