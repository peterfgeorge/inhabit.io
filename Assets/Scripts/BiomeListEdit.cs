using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeListEdit : MonoBehaviour
{
    public GameObject UI;
    IslandGenUIHandler UIHandler;

  void Awake()
  {
      UIHandler = UI.GetComponent<IslandGenUIHandler>();
  }
   void OnEnable()
    {
        UIHandler.PopulateBiomeEditList();
    }
}
