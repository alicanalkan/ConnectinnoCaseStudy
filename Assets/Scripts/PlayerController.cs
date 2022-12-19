using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConnectinnoGames.GameScripts;
using ConnectinnoGames.GameScripts.Ingredients;
using DG.Tweening;
using ConnectinnoGames.Scripts.Object_Pooling;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private PanGrids panGrids;
    [SerializeField] private LayerMask ingredientsLayerMask;

    [SerializeField] private GameObject pan;
    private Transform PanLayer;
    private int index;

    private GameManager gameManager;
    private PoolManager poolManager;

    private void Start()
    {
        panGrids = pan.GetComponent<PanGrids>();
        PanLayer = pan.transform.GetChild(0);
        gameManager = GameManager.Instance;
        poolManager = PoolManager.Instance;

    }
    /// <summary>
    /// This scripts allow to control ingredients
    /// </summary>
    void Update()
    {
        if (DoubleClick())
        {
            if (IsPointerOverUIElement()) return;

            var mouse = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mouse);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ingredientsLayerMask))
            {

                var durationMultiplier = Vector2.Distance(hit.transform.position, panGrids.panPositions[index].gridPos);

                Ingredient ingredient = hit.transform.GetComponent<Ingredient>();
                ingredient.isInPan = true;
                

                var animationTime = 5 / durationMultiplier;

                if (animationTime > 1)
                    animationTime = 1;

                hit.transform.DOMove(panGrids.panPositions[index].gridPos, animationTime).OnComplete(() => {
                    if (gameManager.IsIngredientCorrect((int)ingredient.type))
                    {
                        index++;
                        hit.transform.SetParent(PanLayer);
                        //var coinImage =  poolManager.GetPoolObject(PoolObjectType.CoinImage);

                    }
                    else
                    {
                        hit.transform.DOMove(gameManager.GetRandomSafePosition(), animationTime);
                        ingredient.isInPan = false;
                        //index--;
                    }
                });
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(IsPointerOverUIElement()) return;

                var mouse = Input.mousePosition;
                var ray = Camera.main.ScreenPointToRay(mouse);

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ingredientsLayerMask))
                {
                    Ingredient ingredient = hit.transform.GetComponent<Ingredient>();
                    if (!ingredient.isInPan)
                    {
                        IngredientDragReferenceData.TempObjBeingDragged = hit.transform.gameObject;
                    }
                }
                else
                {
                    IngredientDragReferenceData.TempObjBeingDragged = null;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                IngredientDragReferenceData.TempObjBeingDragged = null;
            }

        }

        if (IngredientDragReferenceData.TempObjBeingDragged != null)
        {
            var worldPosition = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
            var desiredPosition = new Vector2(worldPosition.x, worldPosition.y);
            IngredientDragReferenceData.TempObjBeingDragged.transform.position = desiredPosition;
        }
    }

    float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    bool DoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        return false;
    }

    private void ClearPan()
    {
        index = 0;

        Sequence endRecipeSequence = DOTween.Sequence();

        var PanRotationTween = PanLayer.DOLocalRotate(new Vector3(0, 2300, 0), 3, RotateMode.FastBeyond360).SetEase(Ease.InBack);
        endRecipeSequence.Append(PanRotationTween);
       
        var allChildIngredientsList = PanLayer.GetComponentsInChildren<Ingredient>();
        foreach (var child in allChildIngredientsList)
        {
            var destroyObjectTween = child.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
            {
                poolManager.DestroyObject(child.gameObject, (PoolObjectType)child.type);
            });
            endRecipeSequence.Insert(1,destroyObjectTween);
            
        }

        var allChildIngredientsOnManagerList = gameManager.transform.GetComponentsInChildren<Ingredient>();
        foreach (var child in allChildIngredientsOnManagerList)
        {
            var destroyChildTween =  child.transform.DOScale(Vector3.zero, 1f);
            endRecipeSequence.Insert(1,destroyChildTween);
        }

        endRecipeSequence.OnComplete(() =>
        {
            gameManager.NextRecipe();
        });
    }

    public Vector3 GetPanPosition()
    {
        return pan.transform.position;
    }

    private void OnEnable()
    {
        ConnectinnoActions.OnRecipeCompleted += ClearPan;
    }

    private void OnDestroy()
    {
        ConnectinnoActions.OnRecipeCompleted -= ClearPan;
    }


    /// <summary>
    /// Checks if we touched or hovering on Unity UI element.
    /// </summary>
    /// <returns></returns>
    private static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    /// <summary>
    /// Returns 'true' if we touched or hovering on Unity UI element.
    /// </summary>
    /// <param name="eventSystemRaycastResults"></param>
    /// <returns></returns>
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        foreach (var raycastResult in eventSystemRaycastResults)
        {
            if (raycastResult.gameObject.layer == 5)
                return true;
        }

        return false;
    }


    /// <summary>
    /// Gets all event system raycast results of current mouse or touch position.
    /// </summary>
    /// <returns></returns>
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }
}

public static class IngredientDragReferenceData
{
    public static GameObject TempObjBeingDragged;
}