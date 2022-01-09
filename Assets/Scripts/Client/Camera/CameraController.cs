using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController
{
    private Vector2 cameraPreviousPos;

    private Camera camera;

    public CameraController()
    {
        camera = Camera.main;
    }

    public void Update(Player thePlayer)
    {
        float cameraX = Mathf.Clamp(thePlayer.getEntityGameObject().transform.position.x, thePlayer.getCurrentWorld().getWorldProperties().cameraMinCoordinates.x, thePlayer.getCurrentWorld().getWorldProperties().cameraMaxCoordinates.x);
        float cameraY = Mathf.Clamp(thePlayer.getEntityGameObject().transform.position.y, thePlayer.getCurrentWorld().getWorldProperties().cameraMinCoordinates.y, thePlayer.getCurrentWorld().getWorldProperties().cameraMaxCoordinates.y);

        cameraPreviousPos = camera.transform.position;

        /*if (Vector2.Distance(cameraPreviousPos, new Vector2(cameraX, cameraY)) >= 5)
        {
            camera.transform.position = new Vector3(cameraX, cameraY, camera.transform.position.z);
        }
        else
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, new Vector3(cameraX, cameraY, camera.transform.position.z), Time.deltaTime * 1f);
        }*/

        camera.transform.position = new Vector3(camera.transform.position.x, cameraY, camera.transform.position.z);
        camera.transform.position = Vector3.Lerp(camera.transform.position, new Vector3(cameraX, camera.transform.position.y, camera.transform.position.z), Time.deltaTime * 1f);
    }

    public void placeCamera(Vector3 position)
    {
        Camera.main.transform.position = position;
    }

}
