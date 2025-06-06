//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//public class SizeController : MonoBehaviour
//{
//    public List<SizeControlChild> sizeControlList = new List<SizeControlChild>();

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        sizeControlList = GetComponentsInChildren<SizeControlChild>().ToList();
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    public void InitControlPoint(ItemHasChosen item)
//    {
//        sizeControlList[0].rectTransform.localPosition = new Vector3(0, -((item.itemHasChosen.height * 10) / 2), 0);
//        sizeControlList[1].rectTransform.localPosition = new Vector3(0, ((item.itemHasChosen.height * 10) / 2), 0);
//        sizeControlList[2].rectTransform.localPosition = new Vector3(-((item.itemHasChosen.width * 10 / 2)), 0, 0);
//        sizeControlList[3].rectTransform.localPosition = new Vector3(((item.itemHasChosen.width * 10 / 2)), 0, 0);
//    }

//    public void EnableControlChild(bool status)
//    {
//        foreach (SizeControlChild child in sizeControlList) 
//        {
//            child.gameObject.SetActive(status);
//        }    
//    }    
//}       
