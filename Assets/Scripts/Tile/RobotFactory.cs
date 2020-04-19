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
    [SerializeField] MeshRenderer factoryRenderer;
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
        factoryRenderer.material = materials[IsActiveFactory ? 1 : 0];
        RobotController.OnRobotDeath += RobotController_OnRobotDeath;
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
        yield return new WaitForSeconds(2f);
        var robot = Instantiate(robotPrefab, transform);
        robot.SetSpawn(Tile);
        OnSpawnRobot?.Invoke(robot);
    }

    IEnumerator<WaitForSeconds> LateStart()
    {
        yield return new WaitForSeconds(0.5f);
        OnActivateFactory?.Invoke(this);
        yield return new WaitForSeconds(1f);
        var robot = Instantiate(robotPrefab, transform);
        robot.SetSpawn(Tile);
        OnSpawnRobot?.Invoke(robot);
    }
}
