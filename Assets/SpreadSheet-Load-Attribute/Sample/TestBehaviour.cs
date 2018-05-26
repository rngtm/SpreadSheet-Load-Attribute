namespace GoogleSpreadSheet.Sample
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    [GetSheet]
    public class TestBehaviour : MonoBehaviour
    {
        [GetSheet("光速度")] private int _lightSpeed;
        [GetSheet("重力加速度")] private float _gravity;
        
        IEnumerator Start()
        {
            while (!SpreadSheetLoader.IsSuccess)
            {
                yield return null;
            }

            Debug.LogFormat("_lightSpeed = {0}", _lightSpeed);
            Debug.LogFormat("_gravity = {0}", _gravity);
        }
    }
}