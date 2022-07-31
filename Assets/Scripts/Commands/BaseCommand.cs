using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCommand : ICommand
{
    internal Rigidbody2D rb;

    public BaseCommand() { }

    public abstract void execute();
}
