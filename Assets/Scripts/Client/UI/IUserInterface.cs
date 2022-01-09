using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserInterface 
{

    public void initializeComponents();

    public void onUIClose();

    public void onUIEnable();

    public void updateUI();

    public GameObject getInterfaceObject();

}
