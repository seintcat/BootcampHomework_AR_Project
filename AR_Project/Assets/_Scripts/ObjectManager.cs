using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.XR.ARFoundation;

public class ObjectManager : MonoBehaviour
{
    public GameObject indicator;
    public ARRaycastManager raycastManager;
    public GameObject displayObject;
    public GameObject editorPlane;
    public Transform pokeball;

    private Vector3 deltaPos;
    private float rotationScaleMultiplier = 400.0f;
    private float throwPowerMultiplier = 0.05f;
    public float pokeballResetTime = 3;
    private Vector3 originPokeballPos;

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        originPokeballPos = pokeball.transform.position;

#if UNITY_EDITOR 
#elif UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        editorPlane.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlane();
    }

    private void DetectPlane()
    {
#if UNITY_EDITOR
        Vector3 touchPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touchPos);
        Vector3 direction = (touchWorldPos - transform.position).normalized;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, direction * 100, Color.red, 0.5f);
                if (hit.collider.name.Contains("Plane"))
                {
                    Debug.DrawRay(transform.position, direction * 100, Color.green, 0.5f);

                    displayObject.transform.position = hit.point;
                    indicator.transform.rotation = hit.transform.rotation;
                    displayObject.SetActive(true);
                }
                else
                {
                    displayObject.SetActive(false);
                }
            }

            deltaPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            deltaPos = Input.mousePosition - deltaPos;
            displayObject.transform.Rotate(Vector3.up, -deltaPos.normalized.x * Time.deltaTime * rotationScaleMultiplier);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, direction * 100, Color.red, 0.5f);
                if (hit.collider.name == "Pokeball" && pokeball != null)
                {
                    pokeball.transform.position = new Vector3(hit.point.x, hit.point.y, pokeball.transform.position.z);
                }
            }

            deltaPos = Input.mousePosition;
        }
        // 목적4: 포켓볼을 드래그&드랍으로 던지고 싶다.
        else if (Input.GetMouseButtonUp(0))
        {
            if (pokeball == null)
                return;

            deltaPos = Input.mousePosition - deltaPos;
            float throwPower = deltaPos.magnitude;

            // 목적4: 포켓폴을 던지고 싶다.
            pokeball.GetComponent<Rigidbody>().useGravity = true;
            pokeball.GetComponent<Rigidbody>().AddForce(direction * throwPower * throwPowerMultiplier, ForceMode.Impulse);
            Invoke("ResetPokeball", pokeballResetTime);

            deltaPos = Input.mousePosition;
        }
#elif UNITY_ANDROID
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hitInfo = new List<ARRaycastHit>();

        if(raycastManager.Raycast(screenCenter, hitInfo, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            indicator.transform.position = hitInfo[0].pose.position;
            //indicator.transform.rotation = hitInfo[0].pose.rotation;
            indicator.SetActive(true);
        }
        else
        {
            displayObject.SetActive(false);
            indicator.SetActive(false);
        }
#endif
    }

    private void TouchScreen()
    {
        Touch touch = Input.GetTouch(0);

        if (Input.touchCount > 0)
        {
            if(touch.phase == TouchPhase.Began && indicator.activeSelf)
            {
                displayObject.transform.position = indicator.transform.position;
                displayObject.transform.rotation = indicator.transform.rotation;
                displayObject.SetActive(true);
            }
        }
        else if(touch.phase == TouchPhase.Moved)
        {
            deltaPos = touch.deltaPosition;
            displayObject.transform.Rotate(Vector3.up, -deltaPos.normalized.x * Time.deltaTime * rotationScaleMultiplier);
        }
    }

    void ResetPokeball()
    {
        pokeball.position = originPokeballPos;
        pokeball.rotation = Quaternion.identity;
        pokeball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        pokeball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        pokeball.GetComponent<Rigidbody>().useGravity = false;
    }
}
