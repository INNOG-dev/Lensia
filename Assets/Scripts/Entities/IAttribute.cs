using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttribute
{

    public string getName();

    public double getDefaultValue();

    public double clampValue(double value);

}
