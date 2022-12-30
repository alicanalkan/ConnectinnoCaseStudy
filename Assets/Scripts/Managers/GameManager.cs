using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using ConnectinnoGames.Scripts.Object_Pooling;
using ConnectinnoGames.GameScripts.Ingredients;
using ConnectinnoGames.Scripts.Builder_Scripts.Managers;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts;
using ConnectinnoGames.Managers;
using System.Threading.Tasks;

namespace ConnectinnoGames.GameScripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private RecipeDatabase recipeDatabase;
        [SerializeField] private int recipeCountPerLevel;

        private List<int> currentLevelRecipes = new List<int>();

        public int gameTime;
        public int coinPerLevel;
        private int currentLevelRecipeIndex = 0;
        private List<int> correctIngredientIndex = new List<int>();

        private float safeWorldWidth;
        private float safeWorldHeight;

        private PoolManager poolManager;
        private ConnectinnoGameData gameData;

        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);


            DOTween.SetTweensCapacity(250, 60);

            if(SaveManager.SaveExists("SaveData"))
                gameData = SaveManager.LoadData<ConnectinnoGameData>("SaveData");
            else
                gameData = new ConnectinnoGameData();
            
            CalculateSafeSpawnZone();

        }

        private async void Start()
        {
            poolManager = PoolManager.Instance;

            await SceneLoadManager.LoadScene("MainMenu");

        }

        public async void StartGame()
        {
            await SceneLoadManager.LoadScene("Game");
            StartLevel();

            GenerateWordBorders();
        }

        private void StartLevel()
        {
            ///Sets Seed Based Random Generation.
            Random.InitState(gameData.level); 
            //Get random recipies from recipie Database;
            for (int i = 0; i < recipeCountPerLevel; i++)
            {
                var recipeIndex = UniqueRandomRecipes(0, recipeDatabase.recipes.Count);
                currentLevelRecipes.Add(recipeIndex);
            }

            SpawnIngredients();

        }

        /// <summary>
        /// Clear All Ingredients And Replay Same Level;
        /// </summary>
        private void ReplayLevel() 
        {
            currentLevelRecipeIndex = 0;

            DestroyAllIngredients();

            SpawnIngredients();

            SaveData();
        }

        /// <summary>
        /// Calulate 3D object spawn zone for avoid ui elements collisions;
        /// </summary>
        private void CalculateSafeSpawnZone()
        {
            float aspect = (float)Screen.width / Screen.height;
            safeWorldHeight = Camera.main.orthographicSize * 2;

            safeWorldWidth = safeWorldHeight * aspect;

        }

        /// <summary>
        /// Generate Word borders
        /// </summary>
        private void GenerateWordBorders()
        {
            float aspect = (float)Screen.width / Screen.height;
            float worldHeight = Camera.main.orthographicSize * 2;

            float WorldHeight = worldHeight;
            float worldWidth = worldHeight * aspect;

            float WorldWidth = worldWidth;

            var loadedBorderController = Resources.Load<BorderController>("BorderController");
            var borderController = Instantiate(loadedBorderController);

            var topPanelSize = 2.8f * aspect;
            borderController.RecalculateBorder(new Vector2(WorldWidth / 2, WorldHeight / 2), topPanelSize);
        }

        /// <summary>
        /// Spawn 3D GameObjects
        /// </summary>
        private async void SpawnIngredients()
        {
            ConnectinnoActions.OnRecipeStarted?.Invoke(recipeDatabase.recipes[currentLevelRecipes[currentLevelRecipeIndex]]);

            var loadedCannonBall = Resources.Load<CannonBall>("Cannonball");
            var cannonBall = Instantiate(loadedCannonBall);
            var cannonBallTransform = cannonBall.transform;

            cannonBallTransform.position = new Vector3((safeWorldWidth / 2) - 0.5f, 0, -(safeWorldHeight / 2) + 0.5f);

            //Add To Spawn List Fake Ingredients
            foreach (var fakeIngredients in System.Enum.GetValues(typeof(IngredientType)))
            {
                //Chek If ingredients is correct ingredient
                if (CheckIngredientExist((IngredientType)fakeIngredients))
                {
                    //Create more random fake objects per level
                    for (int i = 0; i < Random.Range(1, 3 + gameData.level); i++)
                    {
                        cannonBall.AddSpawnList((PoolObjectType)fakeIngredients);
                    }
                }
            }

            //Add To Spawn List Correct Ingredients
            foreach (var ingredient in recipeDatabase.recipes[currentLevelRecipes[currentLevelRecipeIndex]].Ingredients)
            {
                for (int i = 0; i < Random.Range(ingredient.count + 3, 10); i++)
                {
                    cannonBall.AddSpawnList((PoolObjectType)ingredient.ingerientType);
                }

                for (int i = 0; i < ingredient.count; i++)
                {
                    correctIngredientIndex.Add((int)ingredient.ingerientType);
                }
            }

            await cannonBall.StartSpawnAndThrow();


        }

        //Check Ingredints is recipe ingredinet
        public bool IsIngredientCorrect(IngredientType type)
        {
            var ingredient = (int)type;
            if (correctIngredientIndex.Contains(ingredient))
            {
                correctIngredientIndex.Remove(ingredient);
                var count = 0;
                foreach(var correctingredint in correctIngredientIndex)
                {
                    if(correctingredint == ingredient)
                        count++;
                }

                ConnectinnoActions.OnCorrectIngredientPlaced?.Invoke(type, count); 
                
                CheckIngredientsCompleted();
                return true;
            }
            return false;
        }

        public void NextRecipe()
        {
            if (IsLevelFinished())
            {
                if(gameData.level % 3 == 0)
                {
                    if (!gameData.openableChests.Contains(gameData.level))
                    {
                        gameData.openableChests.Add(gameData.level);

                    }
                }

                gameData.level++;

                SaveData();

                DestroyAllIngredients();

                currentLevelRecipeIndex = 0;

                currentLevelRecipes.Clear();

                PopupManager.ShowPopup(new WinDefinition(NextLevel));
            }
            else 
            {
                DestroyAllIngredients();
                IncreaseCurrentRecipeIndex();
                SpawnIngredients();
            }
        }

        private void NextLevel() 
        {
            ConnectinnoActions.StartTimer?.Invoke();

            StartLevel();
        }

        /// <summary>
        /// Reset PoolObjects
        /// </summary>
        private void DestroyAllIngredients()
        {
            var ingredinets = GetComponentsInChildren<Ingredient>();
            foreach(var obj in ingredinets)
            {
                poolManager.DestroyObject(obj.gameObject, (PoolObjectType)obj.type);
            }
        }

        /// <summary>
        /// Helper Functions Below
        /// </summary>
        /// 

        /// <summary>
        /// Get Random Safe For Ingredients Positions 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomSafePosition()
        {
            float aspect = (float)Screen.width / Screen.height;
            float worldHeight = Camera.main.orthographicSize * 2;

            float worldWidth = worldHeight * aspect;

            var topPanelSize = 3f * aspect;
            var borderWithSafeWorldHeight = worldWidth - topPanelSize;

            var minmaxX = Random.Range(-(safeWorldWidth / 2), safeWorldWidth / 2);
            var minmaxy = Random.Range(-(borderWithSafeWorldHeight / 2), borderWithSafeWorldHeight / 2);
            return new Vector3(minmaxX, 2f, minmaxy);
        }

        private void IncreaseCurrentRecipeIndex()
        {
            currentLevelRecipeIndex++;
        }

        public int UniqueRandomRecipes(int min, int max)
        {
            int index = Random.Range(min, max);
            while (currentLevelRecipes.Contains(index))
            {
                index = Random.Range(min, max);
            }
            return index;
        }

        private void CheckIngredientsCompleted()
        {
            if (correctIngredientIndex.Count == 0)
            {
                ConnectinnoActions.OnRecipeCompleted?.Invoke();
                var allChildIngredientsList = gameObject.GetComponentsInChildren<Ingredient>();
                foreach (var child in allChildIngredientsList)
                {
                    child.transform.DOScale(Vector3.zero, 1f);
                }
            }
        }

        public void TimeIsUp()
        {
            PopupManager.ShowPopup(new LoseDefinition());
        }

        public int GetCurrentLevel()
        {
            return gameData.level;
        }

        private bool IsLevelFinished()
        {
            if (currentLevelRecipeIndex >= recipeCountPerLevel - 1)
                return true;
            else
                return false;
        }

        private bool CheckIngredientExist(IngredientType ingredi)
        {
            foreach (var ingredient in recipeDatabase.recipes[currentLevelRecipes[currentLevelRecipeIndex]].Ingredients)
            {
                
                if ((int)ingredi == (int)ingredient.ingerientType)
                {
                    return false;
                }
            }
            return true;
        }


        public void AddCoin(int givenCoinAmount) 
        {
            gameData.coinAmount += givenCoinAmount;

            SaveData();
        }

        public ConnectinnoGameData GetGameData() 
        {
            return gameData;
        }

        public void SaveData() 
        {
            SaveManager.SaveData<ConnectinnoGameData>(gameData, "SaveData");
        }

        /// <summary>
        /// Delegate Actions
        /// </summary>
        private void OnEnable() 
        {
            ConnectinnoActions.OnReplayLevel += ReplayLevel;
        }
        /// <summary>
        /// Unload Action Callbacks for prevent memory leak
        /// </summary>
        private void OnDisable()
        {
            ConnectinnoActions.OnReplayLevel -= ReplayLevel;
        }
    }
}
