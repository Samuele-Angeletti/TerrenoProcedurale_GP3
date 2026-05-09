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

    public GenerationResult Generate(AlgorithmType type)
    {
        switch (type)
        {
            case AlgorithmType.RandomWalk:
                return GenerateRandomWalk();
            case AlgorithmType.BSP:
                return GenerateBSP();
            case AlgorithmType.CellularAutomata:
                break;
        }

        return null;
    }
}