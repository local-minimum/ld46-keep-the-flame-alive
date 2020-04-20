using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RobotFactory : MonoBehaviour
{
    public delegate void FactoryActivateEvent(RobotFactory factory);
    public static event FactoryActivateEvent OnActivateFactory;
    public delegate void SpawnRobotEvent(RobotController robot);
    public static event SpawnRobotEvent OnSpawnRobot;

    [SerializeField] RobotController robotPrefab;
    [SerializeField] Material[] materials;
    [SerializeField] MeshRenderer[] ringRenderers;
    [SerializeField] int progressIndex;

    static int activeProgressIndex;
    static List<RobotFactory> _factories;

    public bool IsActiveFactory
    {
        get
        {
            return activeProgressIndex == progressIndex;
        }
    }

    Tile _tile;
    Tile Tile
    {
        get
        {
            if (_tile == null)
            {
                _tile = GetComponentInParent<Tile>();
            }
            return _tile;
        }
    }
    static List<RobotFactory> factories
    {
        get
        {
            if (_factories == null)
            {
                _factories = new List<RobotFactory>();
                _factories.AddRange(FindObjectsOfType<RobotFactory>().Where(factory => factory.gameObject.activeSelf));
            }
            return _factories;
        }
    }

    private void OnEnable()
    {
        if (!factories.Contains(this)) _factories.Add(this);
        SetFactoryMaterial();
        RobotController.OnRobotDeath += RobotController_OnRobotDeath;
    }

    void SetFactoryMaterial()
    {
        for (int i=0; i<ringRenderers.Length; i++)
        {
            ringRenderers[i].material = materials[IsActiveFactory ? i + 1 : 0];
        }
        
    }


    private void OnDisable()
    {
        factories.Remove(this);
        RobotController.OnRobotDeath -= RobotController_OnRobotDeath;
    }

    private void RobotController_OnRobotDeath(RobotController robot)
    {
        if (IsActiveFactory) StartCoroutine(Respawn());
    }

    private void Start()
    {
        if (IsActiveFactory) StartCoroutine(LateStart());
    }

    IEnumerator<WaitForSeconds> Respawn()
    {
        yield return new WaitForSeconds(1.5f);
        OnActivateFactory?.Invoke(this);
        yield return new WaitForSeconds(0.5f);
        var anim = GetComponent<Animator>();
        anim.SetTrigger("Spawn");
        yield return new WaitForSeconds(.6f);
        var robot = Instantiate(robotPrefab, transform);        
        robot.SetSpawn(Tile);
        yield return new WaitForSeconds(0.3f);
        OnSpawnRobot?.Invoke(robot);
    }

    IEnumerator<WaitForSeconds> LateStart()
    {
        OnActivateFactory?.Invoke(this);
        yield return new WaitForSeconds(1f);
        var anim = GetComponent<Animator>();
        anim.SetTrigger("Spawn");
        yield return new WaitForSeconds(0.6f);
        var robot = Instantiate(robotPrefab, transform);
        robot.SetSpawn(Tile);
        yield return new WaitForSeconds(0.5f);
        OnSpawnRobot?.Invoke(robot);
    }

    private void OnTriggerEnter(Collider other)
    {
        var robot = other.GetComponentInParent<RobotController>();
        if  (robot== null || !robot.FlameAlive) return;
        if (progressIndex > activeProgressIndex)
        {
            activeProgressIndex = progressIndex;
            var factories = RobotFactory.factories;
            int i = 0;
            while (i < factories.Count)
            {
                factories[i].SetFactoryMaterial();
                i++;
            }
            
            
        }
    }
}
