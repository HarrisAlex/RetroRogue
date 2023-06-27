using System;

public class Player : Character
{
    public static event Action PlayerDeath;

    protected override void Die()
    {
        PlayerDeath.Invoke();
    }
}
