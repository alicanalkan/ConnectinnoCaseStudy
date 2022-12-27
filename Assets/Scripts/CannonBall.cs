using ConnectinnoGames.Scripts.Object_Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using ConnectinnoGames.GameScripts;

public class CannonBall : MonoBehaviour
{
    [SerializeField] Transform firePosition;
    private List<PoolObjectType> spawnList = new List<PoolObjectType>();

    private PoolManager poolManager;
    private GameManager gameManager;

    private Tween rotateTween;
    private void Awake()
    {
        gameManager = GameManager.Instance;
        poolManager = PoolManager.Instance;

        //Cannonbal rotation tween
        rotateTween = transform.DORotate(new Vector3(0, -50, 0), 2f).SetLoops(-1, LoopType.Yoyo);
    }


    /// <summary>
    /// Start Task
    /// </summary>
    /// <returns></returns>
    public async Task StartSpawnAndThrow()
    {
        ShuffleList();

        for (int i = 0; i < spawnList.Count; i++)
        {
            var spawnedObject = poolManager.GetPoolObject(spawnList[i]);
            // Reset ingredient layer
            spawnedObject.layer = 6;
            //Get rigidbody and reset kinematic
            var rigidbody = spawnedObject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;

            // Init transform
            spawnedObject.transform.position = this.transform.position;
            spawnedObject.transform.localScale = Vector3.zero;

            rigidbody.velocity = Vector3.zero;
            rigidbody.AddForce(firePosition.forward * Random.Range(0.5f,1f), ForceMode.Impulse);

            spawnedObject.transform.DOScale(Vector3.one, 1f);

            spawnedObject.transform.SetParent(gameManager.transform);
            await Task.Delay(50);
        }
        DestroyCannonBall();
        return;
    }

    private void DestroyCannonBall()
    {
        transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
        {
            rotateTween.Kill();
            Destroy(this.gameObject);
        });
    }

    public void AddSpawnList(PoolObjectType poolObjectType)
    {
        spawnList.Add(poolObjectType);
    }


    /// <summary>
    /// Shuffle list for random order ingredinets spawn
    /// </summary>
    private void ShuffleList()
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            var temp = spawnList[i];
            int randomIndex = Random.Range(i, spawnList.Count);
            spawnList[i] = spawnList[randomIndex];
            spawnList[randomIndex] = temp;
        }
    }
}
