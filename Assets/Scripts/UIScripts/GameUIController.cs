using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using ConnectinnoGames.GameScripts;
using ConnectinnoGames.SoundScripts;
using ConnectinnoGames.Scripts.Object_Pooling;

namespace ConnectinnoGames.UIScripts
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI Timer;
        [SerializeField] private Transform coinImageTransform;
        [SerializeField] private TextMeshProUGUI levelText;

        [SerializeField] private List<Image> images = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> IngredientCount = new List<TextMeshProUGUI>();
        [SerializeField] private TextMeshProUGUI recipeText;

        private GameManager gameManager;
        private PoolManager poolManager;
        private Vector3 panPosition;
        private SoundManager soundManager;

        private BaseRecipe ingredients;

        private int coinAmount;

        private void Start()
        {
            soundManager = SoundManager.Instance;
            gameManager = GameManager.Instance;
            poolManager = PoolManager.Instance;

            var gameData = gameManager.GetGameData();
            coinAmount = gameData.coinAmount;
            coinText.text = coinAmount.ToString();

            levelText.text = $"Level {gameData.level}";

            var panWorldPosition = FindObjectOfType<PlayerController>().GetPanPosition();

            Vector3 screenPos = Camera.main.WorldToScreenPoint(panWorldPosition);
            float h = Screen.height;
            float w = Screen.width;
            float x = screenPos.x - (w / 2);
            float y = screenPos.y - (h / 2);
            float s = GetComponent<Canvas>().scaleFactor;

            panPosition = new Vector2(x, y) / s;

            StartCoroutine(GameTimeCountdown(gameManager.gameTime));
        }

        /// <summary>
        /// Start Game timer,Update Level text ,StopTimer remaining from previous level
        /// </summary>
        private void StartGameTimer() 
        {
            StopAllCoroutines();
            StartCoroutine(GameTimeCountdown(gameManager.gameTime));

            levelText.text = $"Level {gameManager.GetCurrentLevel()}";
        }

        /// <summary>
        /// Gives Extra 30Seconds
        /// </summary>
        private void Extra30Seconds() 
        {
            StartCoroutine(GameTimeCountdown(30));
        }

        /// <summary>
        /// Game Timer routine
        /// </summary>
        /// <param name="givenSeconds"></param>
        /// <returns></returns>
        private IEnumerator GameTimeCountdown(int givenSeconds)
        {
            int counter = givenSeconds;
            while (counter > 0)
            {
                yield return new WaitForSeconds(1);
                counter--;
                int minutes = Mathf.FloorToInt(counter / 60F);
                int seconds = Mathf.FloorToInt(counter - minutes * 60);
                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
                Timer.text = niceTime;
            }

            gameManager.TimeIsUp();
        }

        /// <summary>
        /// Start Coin Animaton
        /// </summary>
        public void OnRecipeCompleted()
        {
            AddCoin(gameManager.coinPerLevel);
        }

        /// <summary>
        /// Update ui for current recipe
        /// </summary>
        /// <param name="recipeIngredients"></param>
        private void UpdateRecipeUI(BaseRecipe recipeIngredients)
        {
            ingredients = recipeIngredients;
            recipeText.text = recipeIngredients.RecipieName;
            var recipies = recipeIngredients.Ingredients;
            for (int i = 0; i < recipies.Count; i++)
            {
                RecipeIngredients ingredient = recipies[i];
                StartCoroutine(LoadTexture(ingredient, i));
            }
        }

        /// <summary>
        /// Load Ingredient Texture
        /// </summary>
        /// <param name="recipeIngredients"></param>
        /// <param name="ingredientIndex"></param>
        /// <returns></returns>
        IEnumerator LoadTexture(RecipeIngredients recipeIngredients, int ingredientIndex)
        {
            RecipeIngredients ingredient = recipeIngredients;
            ResourceRequest request = Resources.LoadAsync<Sprite>("Ingredients/" + ingredient.ingerientType.ToString().ToLower());
            yield return request;
            images[ingredientIndex].sprite = (Sprite)request.asset;
            images[ingredientIndex].color = new Color(255, 255, 255, 255);
            IngredientCount[ingredientIndex].text = $"x{ingredient.count}";
        }


        /// <summary>
        /// Update Sellected type object count on ui
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        public void UpdateIngredientCount(IngredientType type, int count)
        {
            for (int i = 0; i < ingredients.Ingredients.Count; i++)
            {
                if (ingredients.Ingredients[i].ingerientType == type)
                {
                    IngredientCount[i].text = $"x{count}";
                }
            }
           
        }
        /// <summary>
        /// CoinAnimatons
        /// </summary>
        /// <param name="amount"></param>
        private void AddCoin(int amount)
        {
            float aspect = (float)Screen.width / Screen.height;
            var safeWorldHeight = Camera.main.orthographicSize * 2;

            var safeWorldWidth = safeWorldHeight * aspect;
            var worldPadding = 2;
            safeWorldWidth -= worldPadding;
            safeWorldHeight -= worldPadding;

            for (int i = 0; i < amount; i++)
            {
                var coinImage = poolManager.GetPoolObject(PoolObjectType.CoinImage);
                coinImage.transform.SetParent(transform);
                coinImage.transform.localScale = Vector3.one;
                coinImage.GetComponent<RectTransform>().anchoredPosition = panPosition;

                var controlPoint = new Vector3(x: -safeWorldWidth / 2, y:0,z: -safeWorldHeight / 2);
                var midControlPoint = new Vector3(-safeWorldWidth / 2, 0, 0);

                Vector3[] positionArray = new[] { coinImageTransform.position, controlPoint, midControlPoint };

                coinImage.transform.DOPath(positionArray, Random.Range(1.5f, 3f), PathType.CubicBezier).OnComplete(() =>
                {
                    coinAmount++;
                    coinText.text = coinAmount.ToString();
                    soundManager.PlayCoin();
                });
            }
            gameManager.AddCoin(amount);
        }

        /// <summary>
        /// Remove used coins
        /// </summary>
        /// <param name="amount">Coin amount</param>
        private void RemoveCoin(int amount)
        {
            coinAmount -= amount;
            coinText.text = coinAmount.ToString();
            gameManager.AddCoin(-amount);
        }

        /// <summary>
        /// Delegate Actions
        /// </summary>
        private void OnEnable()
        {
            ConnectinnoActions.OnRecipeStarted += UpdateRecipeUI;
            ConnectinnoActions.OnExtraSeconds += Extra30Seconds;
            ConnectinnoActions.StartTimer += StartGameTimer;
            ConnectinnoActions.OnRecipeCompleted += OnRecipeCompleted;
            ConnectinnoActions.OnAddCoin += AddCoin;
            ConnectinnoActions.OnRemoveCoin += RemoveCoin;
            ConnectinnoActions.OnCorrectIngredientPlaced += UpdateIngredientCount;
        }
        /// <summary>
        /// Unload Action Callbacks for prevent memory leak
        /// </summary>
        private void OnDisable()
        {
            ConnectinnoActions.StartTimer -= StartGameTimer;
            ConnectinnoActions.OnExtraSeconds -= Extra30Seconds;
            ConnectinnoActions.OnRecipeCompleted -= OnRecipeCompleted;
            ConnectinnoActions.OnRecipeStarted -= UpdateRecipeUI;
            ConnectinnoActions.OnAddCoin -= AddCoin;
            ConnectinnoActions.OnRemoveCoin -= RemoveCoin;
            ConnectinnoActions.OnCorrectIngredientPlaced -= UpdateIngredientCount;
        }
    }
}

