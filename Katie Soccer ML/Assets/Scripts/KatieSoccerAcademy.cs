﻿using MLAgents;
using UnityEngine;

public class KatieSoccerAcademy : Academy
{
    /// <summary>
    /// The spawn area margin multiplier.
    /// ex: .9 means 90% of spawn area will be used. 
    /// .1 margin will be left (so players don't spawn off of the edge). 
    /// The higher this value, the longer training time required.
    /// </summary>
	public float spawnAreaMarginMultiplier;

    public float TimePenalty = 500f;
}
