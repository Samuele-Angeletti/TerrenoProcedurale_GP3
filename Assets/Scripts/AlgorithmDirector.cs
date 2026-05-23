using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AlgorithmDirector : MonoBehaviour
{
    [Header("Complex Walker Settings")]
    [SerializeField] int complexSteps = 500;
    [SerializeField] int complexSize = 50;
    [SerializeField] int walkerCount = 1;
    [SerializeField] int seed = 0;
    [SerializeField] bool randomSeeded = false;

    [Header("BSP Settings")]
    [SerializeField] int bspWidth = 50;
    [SerializeField] int bspHeight = 50;
    [SerializeField] int bspMinPartitionWidth = 5;
    [SerializeField] int bspMinPartitionHeight = 5;
    [SerializeField] int bspRoomPadding = 1;
    [SerializeField] int bspSeed = 1;
    [SerializeField] bool bspRandomSeed = false;

    [Header("Cellular Automata Settings")]
    [SerializeField] int caWidth = 50;
    [SerializeField] int caHeight = 50;
    [SerializeField] int caInitialWallChance = 45;
    [SerializeField] int caSteps = 5;
    [SerializeField] int caBirthLimit = 4;
    [SerializeField] int caDeathLimit = 3;
    [SerializeField] bool caSolidBorder = true;
    [SerializeField] int caSeed = 45;
    [SerializeField] bool caRandomSeed = false;

    public enum AlgorithmType { RandomWalk, BSP, CellularAutomata };

    public GenerationResult GenerateRandomWalk()
    {
        return new RandomWalkGenerator(new RandomWalkSettings
        {
            Start = (complexSize / 2, complexSize / 2),
            Steps = complexSteps,
            WalkerCount = walkerCount,
            Bounds = (0, 0, complexSize - 1, complexSize - 1),
            Seed = seed,
            RandomSeed = randomSeeded
        }).Generate();
    }

    public GenerationResult GenerateBSP()
    {
        return new BSPGenerator(new BSPSettings
        {
            Width = bspWidth,
            Height = bspHeight,
            MinPartitioningHeight = bspMinPartitionHeight,
            RoomPadding = bspRoomPadding,
            Seed = seed,
            MinPartitioningWidth = bspMinPartitionWidth,
            RandomSeed = bspRandomSeed
        }).Generate();
    }

    private GenerationResult GenerateCellularAutomata()
    {
        return new CellularAutomataGenerator(new CellularAutomataSettings()
        {
            Width = caWidth,
            Height = caHeight,
            InitialWallChance = caInitialWallChance,
            Steps = caSteps,
            BirthLimit = caBirthLimit,
            DeathLimit = caDeathLimit,
            Seed = caSeed,
            SolidBorder = caSolidBorder,
            RandomSeed = caRandomSeed
        }).Generate();
    }

    public GenerationResult Generate(AlgorithmType type)
    {
        switch (type)
        {
            case AlgorithmType.RandomWalk:
                return GenerateRandomWalk();
            case AlgorithmType.BSP:
                return GenerateBSP();
            case AlgorithmType.CellularAutomata:
                return GenerateCellularAutomata();
        }

        return null;
    }

}