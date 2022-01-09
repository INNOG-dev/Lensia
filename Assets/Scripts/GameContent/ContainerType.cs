using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerType : RegistryObject
{

    private Type container;

    public ContainerType(Type container, string registryName)
    {
        if (container.GetType().IsInstanceOfType(typeof(Container)))
        {
            this.container = container;
        }
        else
        {
            throw new SystemException("Only container can be registered in this registry");
        }
    }

    public Container getContainer(NetworkUser user)
    {
        return (Container)Activator.CreateInstance(container, new object[] { getId(), user });
    }

}
