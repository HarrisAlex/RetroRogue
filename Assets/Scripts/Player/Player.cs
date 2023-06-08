using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Character
{
    public event Action OnDie;

    protected override void Die()
    {
        OnDie.Invoke();
    }
}
