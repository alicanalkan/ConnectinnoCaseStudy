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

    private Vector3 oldMause;
    private float mouseSpeed;
    [SerializeField] private float velocityKeppTimeMultiplier;
    [SerializeField] private float velocityTimeMultiplier;

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

                //Calculate animaton time
                var animationTime = 5 / durationMultiplier;

                if (animationTime > 1)
                    animationTime = 1;

                var rigidBody = hit.transform.GetComponent<Rigidbody>();
                rigidBody.useGravity = false;
                hit.transform.SetParent(PanLayer);

                // Create tween path 
                //first value over the pan object
                //second value pan grid position
                Vector3[] positionArray = new[] { new Vector3(hit.transform.position.x, pan.transform.position.y + 1, hit.transform.position.z), panGrids.panPositions[index].gridPos };

                if (gameManager.IsIngredientCorrect(ingredient.type))
                {
                    //Place to pan
                    hit.transform.DOPath(positionArray, animationTime);
                    hit.transform.gameObject.layer = 7;
                    hit.transform.localEulerAngles = Vector3.zero;
                    rigidBody.isKinematic = true;
                    
                    index++;
                }
                else
                {
                    //Reset transform tweens;
                    hit.transform.DOPath(positionArray, animationTime).OnComplete(() => {
                        hit.transform.DOMove(gameManager.GetRandomSafePosition(), animationTime / 2 );
                        rigidBody.useGravity = true;
                        rigidBody.velocity = Vector3.zero;
                    });
                    hit.transform.SetParent(gameManager.transform);
                }
            }
        }
        else
        {
            //Drag handler
            if (Input.GetMouseButtonDown(0))
            {
                if(IsPointerOverUIElement()) return;

                var mouse = Input.mousePosition;
                var ray = Camera.main.ScreenPointToRay(mouse);

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ingredientsLayerMask))
                {
                    IngredientDragReferenceData.TempObjBeingDragged = hit.transform.gameObject;
                    IngredientDragReferenceData.TempRigidbody = hit.transform.gameObject.GetComponent<Rigidbody>();

                    // Remove gravity physcis
                    IngredientDragReferenceData.TempRigidbody.useGravity = false;
                    var worldPosition = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    oldMause = new Vector3(worldPosition.x, 0.2f, worldPosition.z);

                    mouseSpeed = velocityTimeMultiplier;
                }
                else
                {
                    IngredientDragReferenceData.TempObjBeingDragged = null;
                    IngredientDragReferenceData.TempRigidbody = null;
                }
            }
            else if (Input.GetMouseButtonUp(0) && IngredientDragReferenceData.TempObjBeingDragged)
            {
                var worldPosition = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
                var mouse_diff = (oldMause - new Vector3(worldPosition.x, 0.2f, worldPosition.z));

                //Check Mouse speed prevent negative values
                if (mouseSpeed < 0)
                    mouseSpeed = 0;

                //Set velocity to object
                IngredientDragReferenceData.TempObjBeingDragged.GetComponent<Rigidbody>().velocity = (mouse_diff * mouseSpeed * -1);
                IngredientDragReferenceData.TempRigidbody.useGravity = true;

                IngredientDragReferenceData.TempObjBeingDragged = null;
                IngredientDragReferenceData.TempRigidbody = null;

            }
        }

        if (IngredientDragReferenceData.TempObjBeingDragged != null)
        {
            //Tack input position and drag object
            if (IsPointerOverUIElement()) return;
            var worldPosition = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
            var desiredPosition = new Vector3(worldPosition.x, 0.2f, worldPosition.z);

            //Down mouse velocity
            mouseSpeed -= Time.deltaTime * velocityKeppTimeMultiplier;

            //Update ingredient Position
            IngredientDragReferenceData.TempObjBeingDragged.transform.position = desiredPosition;

        } 
    }

    bool DoubleClick()
    {
        //first click
        if (Input.GetMouseButtonDown(0))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        //second click
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        //check first and second click time delta
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
}

/// <summary>
/// Temporary object class
/// </summary>
public static class IngredientDragReferenceData
{
    public static GameObject TempObjBeingDragged;
    public static Rigidbody TempRigidbody;
}