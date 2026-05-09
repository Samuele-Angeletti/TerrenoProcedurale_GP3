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

    public GenerationResult Generate(AlgorithmType type)
    {
        switch (type)
        {
            case AlgorithmType.RandomWalk:
                return GenerateRandomWalk();
        }

        return null;
    }
}