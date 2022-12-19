using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using ConnectinnoGames.SoundScripts;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Mask))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

    public int startingPage = 0;
    public float fastSwipeThresholdTime = 0.3f;
    public int fastSwipeThresholdDistance = 100;
    public float decelerationRate = 10f;

    // fast swipes should be fast and short. If too long, then it is not fast swipe
    private int _fastSwipeThresholdMaxLimit;

    private ScrollRect scrollRectComponent;
    private RectTransform scrollRectRect;
    private RectTransform container;

    private bool horizontal;
    
    // number of pages in container
    private int pageCount;
    private int currentPage;

    // whether lerping is in progress and target lerp position
    private bool lerp;
    private Vector2 lerpTo;

    // target position of every page
    private List<Vector2> pagePositions = new List<Vector2>();

    private float timeStamp;
    private Vector2 startPosition;

    private SoundManager soundManager;


    void Start() {
        scrollRectComponent = GetComponent<ScrollRect>();
        scrollRectRect = GetComponent<RectTransform>();
        container = scrollRectComponent.content;
        pageCount = container.childCount;

        // is it horizontal or vertical scrollrect
        if (scrollRectComponent.horizontal && !scrollRectComponent.vertical) {
            horizontal = true;
        } else if (!scrollRectComponent.horizontal && scrollRectComponent.vertical) {
            horizontal = false;
        } else {
            Debug.LogWarning("Confusing setting of horizontal/vertical direction. Default set to horizontal.");
            horizontal = true;
        }

        lerp = false;
        soundManager = SoundManager.Instance;
        // init
        SetPagePositions();
        SetPage(startingPage);

	}

    private void ChangeScreen(int ScreenIndex)
    {
        LerpToPage(currentPage + ScreenIndex);
    }

    void Update() {
        // if moving to target position
        if (lerp) {
            // prevent overshooting with values greater than 1
            float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
            container.anchoredPosition = Vector2.Lerp(container.anchoredPosition, lerpTo, decelerate);
            // time to stop lerping?
            if (Vector2.SqrMagnitude(container.anchoredPosition - lerpTo) < 0.25f) {
                // snap to target and stop lerping
                container.anchoredPosition = lerpTo;
                lerp = false;
                // clear also any scrollrect move that may interfere with our lerping
                scrollRectComponent.velocity = Vector2.zero;
            }

        }
    }
    private void SetPagePositions() {
        int width = 0;
        int height = 0;
        int offsetX = 0;
        int offsetY = 0;
        int containerWidth = 0;
        int containerHeight = 0;

        if (horizontal) {
            // screen width in pixels of scrollrect window
            width = (int)scrollRectRect.rect.width;
            // center position of all pages
            offsetX = width / 2;
            // total width
            containerWidth = width * pageCount;
            // limit fast swipe length - beyond this length it is fast swipe no more
            _fastSwipeThresholdMaxLimit = width;
        } else {
            height = (int)scrollRectRect.rect.height;
            offsetY = height / 2;
            containerHeight = height * pageCount;
            _fastSwipeThresholdMaxLimit = height;
        }

        // set width of container
        Vector2 newSize = new Vector2(containerWidth, containerHeight);
        container.sizeDelta = newSize;
        Vector2 newPosition = new Vector2(containerWidth / 2, containerHeight / 2);
        container.anchoredPosition = newPosition;

        // delete any previous settings
        pagePositions.Clear();

        // iterate through all container childern and set their positions
        for (int i = 0; i < pageCount; i++) {
            RectTransform child = container.GetChild(i).GetComponent<RectTransform>();
            Vector2 childPosition;
            if (horizontal) {
                childPosition = new Vector2(i * width - containerWidth / 2 + offsetX, 0f);
            } else {
                childPosition = new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));
            }
            child.anchoredPosition = childPosition;
            pagePositions.Add(-childPosition);
        }
    }

    private void SetPage(int aPageIndex) {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, pageCount - 1);
        container.anchoredPosition = pagePositions[aPageIndex];
        currentPage = aPageIndex;
    }

    public void LerpToPage(int aPageIndex) {
        soundManager.PlayClick();
        aPageIndex = Mathf.Clamp(aPageIndex, 0, pageCount - 1);
        lerpTo = pagePositions[aPageIndex];
        lerp = true;
        currentPage = aPageIndex;
    }


    private int GetNearestPage() {
        // based on distance from current position, find nearest page
        Vector2 currentPosition = container.anchoredPosition;

        float distance = float.MaxValue;
        int nearestPage = currentPage;

        for (int i = 0; i < pagePositions.Count; i++) {
            float testDist = Vector2.SqrMagnitude(currentPosition - pagePositions[i]);
            if (testDist < distance) {
                distance = testDist;
                nearestPage = i;
            }
        }

        return nearestPage;
    }

    public void OnBeginDrag(PointerEventData aEventData) {
        // if currently lerping, then stop it as user is draging
        lerp = false;
    }

    public void OnEndDrag(PointerEventData aEventData) {
        // how much was container's content dragged
        float difference;
        if (horizontal) {
            difference = startPosition.x - container.anchoredPosition.x;
        } else {
            difference = - (startPosition.y - container.anchoredPosition.y);
        }

        // test for fast swipe - swipe that moves only +/-1 item
        if (Time.unscaledTime - timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance &&
            Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit) {
            if (difference > 0) {
                ChangeScreen(1);
            } else {
                ChangeScreen(-1);
            }
        } else {
            // if not fast time, look to which page we got to
            LerpToPage(GetNearestPage());
        }

    }

    public void OnDrag(PointerEventData aEventData) {
        // save time - unscaled so pausing with Time.scale should not affect it
        timeStamp = Time.unscaledTime;
        // save current position of cointainer
        startPosition = container.anchoredPosition;
    }
}
