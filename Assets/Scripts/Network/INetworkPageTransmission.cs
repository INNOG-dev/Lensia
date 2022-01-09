using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkPageTransmission<E> where E : INetworkSerializable, new()
{
    public int getElementsPerPage();

    public List<E> nextElements();

    public int getLastElementIndex();


}
