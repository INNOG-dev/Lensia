using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAttribute
{

    private Dictionary<string, IAttribute> entityAttributes = new Dictionary<string, IAttribute>();

    public void registerAttribute(IAttribute attribute)
    {
        entityAttributes.Add(attribute.getName(), attribute);
    }

    public IAttribute getAttribute(String attributeName)
    {
        if(entityAttributes.ContainsKey(attributeName)) return entityAttributes[attributeName];

        return null;
    }



}
