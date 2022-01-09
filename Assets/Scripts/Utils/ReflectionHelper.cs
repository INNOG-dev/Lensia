using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class ReflectionHelper
{

    public static object instantiateDynamically(Type type , object[] parameters)
    {
        Type[] argsTypes = new Type[parameters.Length];

        for(int i = 0; i < argsTypes.Length; i++)
        {
            argsTypes[i] = parameters[i].GetType();
        }

        ConstructorInfo ctor = type.GetConstructor(argsTypes);

        return ctor.Invoke(parameters);
    }

}
