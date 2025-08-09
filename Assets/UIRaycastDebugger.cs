using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log("UI objects under click (top first):");
                foreach (RaycastResult result in results)
                {
                    Debug.Log($"- {result.gameObject.name} (in {result.gameObject.scene.name})");
                }
            }
            else
            {
                Debug.Log("No UI object detected under click.");
            }
        }
    }
}
