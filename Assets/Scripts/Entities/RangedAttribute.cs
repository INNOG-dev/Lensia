using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttribute : IAttribute
{

    private string attributeName;

    private double minimumValue;

    private double maximumValue;

    private double defaultValue;

    public RangedAttribute(string name, double defaultValue, double minimumValue, double maximumValue)
    {
        attributeName = name;

        if (minimumValue > maximumValue)
        {
            throw new System.ArgumentException("Minimum value cannot be bigger than maximum value!");
        }
        else if (defaultValue < minimumValue)
        {
            throw new System.ArgumentException("Default value cannot be lower than minimum value!");
        }
        else if (defaultValue > maximumValue)
        {
            throw new System.ArgumentException("Default value cannot be bigger than maximum value!");
        }

        this.minimumValue = minimumValue;
        this.maximumValue = maximumValue;
        this.defaultValue = defaultValue;
    }

    public double clampValue(double value)
    {
        if(value > maximumValue)
        {
            value = maximumValue;
        }

        if(value < minimumValue)
        {
            value = minimumValue;
        }

        return value;
    }

    public string getName()
    {
        return attributeName;
    }

    public double getDefaultValue()
    {
        return defaultValue;
    }
}
