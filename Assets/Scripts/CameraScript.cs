using UnityEngine;

public class CameraScript : MonoBehaviour
{

    private GameObject cameraParams;
    private Transform cameraPlaceTransform;
    private Transform cameraTargetTransform;
    private Vector3 velocity = Vector3.zero;
    private const float followDelay = 0.125f;

    void Start ()
    {
        cameraParams = GameObject.Find("CameraParams");
        cameraPlaceTransform = cameraParams.transform.Find("CameraPlace").transform;
        cameraTargetTransform = cameraParams.transform.Find("CameraTarget").transform;
    }

    private void LateUpdate()
    {
        Vector3 newCameraParamsPosition = Player.Instance.GetNearestPointCenteredBetweenCurrentTileEdges();
        //Debug.DrawLine(player.transform.position, newCameraParamsPosition, Color.red, 5.0f);
        cameraParams.transform.position = newCameraParamsPosition;

        // other attempts to smoothen camera
        // transform.position = cameraPlaceTransform.position;
        // transform.position += (cameraPlaceTransform.position - transform.position) * followDelay;
        // transform.position = Vector3.Lerp(transform.position, cameraPlaceTransform.position, followDelay);

        transform.position = Vector3.SmoothDamp(transform.position, cameraPlaceTransform.position, ref velocity, followDelay);
        transform.LookAt(cameraTargetTransform);
    }
}
