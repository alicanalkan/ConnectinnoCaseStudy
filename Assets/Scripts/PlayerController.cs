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

    private float clicked = 0;
    private float clicktime = 0;
    //Double Click Time
    private float clickdelay = 0.5f;

    [SerializeField] private float velocityMultiplier;

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
        if (Input.GetMouseButtonDown(0))
        {
            ///Racast Ingredients layer Masks
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var raycastResult = Physics.Raycast(ray, out var raycastHit, Mathf.Infinity, ingredientsLayerMask);

            if (raycastResult)
            {
                IngredientDragReferenceData.TempObjBeingDragged = raycastHit.transform.gameObject;
                IngredientDragReferenceData.TempRigidbody = raycastHit.transform.gameObject.GetComponent<Rigidbody>();

                clicked++;

                if (clicked == 1) clicktime = Time.time;

                //Check is doubleClick detected
                if (clicked > 1 && Time.time - clicktime < clickdelay)
                {
                    //reset clicked count & clicktime
                    clicked = 0;
                    clicktime = 0;

                    DoubleClickObject(raycastHit.transform.gameObject);
                    
                    //return;
                }
                else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
            }
        }

        //double click not detected start dragging
        if (IngredientDragReferenceData.TempObjBeingDragged)
        {
            if (IsPointerOverUIElement()) return;

            var worldPosition = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
            var targetPosition = new Vector3(worldPosition.x, 0, worldPosition.z);
            var velocity = Vector3.zero;
            var desiredPosition = Vector3.SmoothDamp(IngredientDragReferenceData.TempObjBeingDragged.transform.position, targetPosition, ref velocity, 0.02f);

            IngredientDragReferenceData.TempRigidbody.useGravity = false;
           
            //Update ingredient Position
            IngredientDragReferenceData.TempRigidbody.MovePosition(desiredPosition);

            if (Input.GetMouseButtonUp(0))
            {
                //Set velocity to object
                IngredientDragReferenceData.TempRigidbody.velocity = (targetPosition - IngredientDragReferenceData.TempObjBeingDragged.transform.position) * velocityMultiplier;

                IngredientDragReferenceData.TempRigidbody.useGravity = true;

                IngredientDragReferenceData.TempObjBeingDragged = null;
                IngredientDragReferenceData.TempRigidbody = null;
            }
        }
    }

    private void DoubleClickObject(GameObject Ingredient)
    {
        Ingredient ingredient = Ingredient.transform.GetComponent<Ingredient>();

        //Calculate animaton time
        var animationTime = GetAnimationTimeBetweenCords(Ingredient.transform.position, panGrids.panPositions[index].gridPos);

        IngredientDragReferenceData.TempRigidbody.useGravity = false;
        Ingredient.transform.SetParent(PanLayer);

        // Create tween path 
        //first value over the pan object
        //second value pan grid position
        Vector3[] positionArray = new[] { new Vector3(Ingredient.transform.position.x, pan.transform.position.y + 1, Ingredient.transform.position.z), panGrids.panPositions[index].gridPos };

        if (gameManager.IsIngredientCorrect(ingredient.type))
        {
            //Place to pan
            Ingredient.transform.DOPath(positionArray, animationTime);
            Ingredient.transform.gameObject.layer = 7;
            Ingredient.transform.localEulerAngles = Vector3.zero;
            var rb = Ingredient.GetComponent<Rigidbody>().isKinematic = true;
            index++;
        }
        else
        {
            //Reset transform tweens;
            Ingredient.transform.DOPath(positionArray, animationTime).OnComplete(() => {
                Ingredient.transform.DOMove(gameManager.GetRandomSafePosition(), animationTime / 2);
            });
            Ingredient.transform.SetParent(gameManager.transform);
        }
    }

    private void ClearPan()
    {
        index = 0;

        Sequence endRecipeSequence = DOTween.Sequence();

        var PanRotationTween = PanLayer.DOLocalRotate(new Vector3(0, 2300, 0), 3, RotateMode.FastBeyond360).SetEase(Ease.InBack);
        endRecipeSequence.Append(PanRotationTween);
       
        var allChildIngredientsList = PanLayer.GetComponentsInChildren<Ingredient>();
     

        //all ingredients scale tween
        foreach (var child in allChildIngredientsList)
        {
            var destroyObjectTween = child.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
            {
                poolManager.DestroyObject(child.gameObject, (PoolObjectType)child.type);
            });
            endRecipeSequence.Insert(2,destroyObjectTween);
            
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

    private void DestroyObjects()
    {
        var allChildIngredientsList = PanLayer.GetComponentsInChildren<Ingredient>();
        foreach (var child in allChildIngredientsList)
        {
            poolManager.DestroyObject(child.gameObject, (PoolObjectType)child.type);
        }
    }
    private void OnEnable()
    {
        ConnectinnoActions.OnRecipeCompleted += ClearPan;
        ConnectinnoActions.OnReplayLevel += DestroyObjects;
    }

    private void OnDestroy()
    {
        ConnectinnoActions.OnRecipeCompleted -= ClearPan;
        ConnectinnoActions.OnReplayLevel -= DestroyObjects;
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


    private float GetAnimationTimeBetweenCords(Vector2 fistPos, Vector2 secondPos)
    {
        var durationMultiplier = Vector2.Distance(fistPos, secondPos);
        var animationTime = 5 / durationMultiplier;

        if (animationTime > 1)
            animationTime = 1;
        return animationTime;
    }
}

/// <summary>
/// Temporary object class
/// </summary>
public static class IngredientDragReferenceData
{
    public static GameObject TempObjBeingDragged;
    public static Rigidbody TempRigidbody;
}