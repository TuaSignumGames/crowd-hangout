using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private GameObject[] _coreServiceContainers;

    private IService[] _services;

    private void Awake()
    {
        _services = new IService[_coreServiceContainers.Length];

        for (int i = 0; i < _coreServiceContainers.Length; i++)
        {
            _services[i] = _coreServiceContainers[i].GetComponent<IService>();

            if (_coreServiceContainers[i].activeInHierarchy)
            {
                _services[i].Initialize();
            }
        }
    }

    private void Start()
    {
        AppManager.Instance.LoadGameScene();
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < _services.Length; i++)
        {
            _services[i].Deinitialize();
        }
    }
}
