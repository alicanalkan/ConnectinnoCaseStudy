using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using ConnectinnoGames.Scripts.Object_Pooling;
using ConnectinnoGames.GameScripts.Ingredients;
using ConnectinnoGames.Scripts.Builder_Scripts.Managers;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts;
using ConnectinnoGames.Managers;

namespace ConnectinnoGames.GameScripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private RecipeDatabase recipeDatabase;
        [SerializeField] private float safeZonePaddingX;
        [SerializeField] private float safeZonePaddingY;
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
        private SoundScripts.SoundManager soundManager;
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);


            DOTween.SetTweensCapacity(250, 10);

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

            soundManager = SoundScripts.SoundManager.Instance;
        }

        public async void StartGame()
        {
            await SceneLoadManager.LoadScene("Game");
            StartLevel();
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
            float worldHeight = Camera.main.orthographicSize * 2;

            safeWorldHeight = worldHeight - safeZonePaddingY;
            float worldWidth = worldHeight * aspect;

            safeWorldWidth = worldWidth - safeZonePaddingX;
        }

        /// <summary>
        /// Spawn 3D GameObjects
        /// </summary>
        private void SpawnIngredients()
        {
            ConnectinnoActions.OnRecipeStarted?.Invoke(recipeDatabase.recipes[currentLevelRecipes[currentLevelRecipeIndex]]);

            //Spawn Fake Ingredients
            foreach (var fakeIngredients in System.Enum.GetValues(typeof(IngredientType)))
            {
                //Chek If ingredients is correct ingredient
                if (CheckIngredientExist((IngredientType)fakeIngredients))
                {
                    //Create more random fake objects per level
                    for (int i = 0; i < Random.Range(5, 5 + gameData.level); i++)
                    {
                        var minmaxX = Random.Range(-(safeWorldWidth / 2), safeWorldWidth / 2);
                        var minmaxy = Random.Range(-(safeWorldHeight / 2), safeWorldHeight / 2);
                        var fakeObj = poolManager.GetPoolObject((PoolObjectType)fakeIngredients);
                        fakeObj.transform.DOMove(new Vector2(minmaxX, minmaxy), 1.5f);

                        fakeObj.transform.SetParent(this.transform);
                    }

                }

            }

            //Spawn correct Ingredients
            foreach (var ingredient in recipeDatabase.recipes[currentLevelRecipes[currentLevelRecipeIndex]].Ingredients)
            {
                for(int i = 0; i < Random.Range(ingredient.count + 3, 20); i++)
                {
                    var minmaxX = Random.Range(-(safeWorldWidth / 2), safeWorldWidth / 2);
                    var minmaxy = Random.Range(-(safeWorldHeight / 2), safeWorldHeight / 2);
                    var correctObj = poolManager.GetPoolObject((PoolObjectType)ingredient.ingerientType);
                    correctObj.transform.DOMove(new Vector2(minmaxX, minmaxy), 1.5f);

                    correctObj.transform.SetParent(this.transform);
                }

                for (int i = 0; i < ingredient.count; i++)
                {
                    correctIngredientIndex.Add((int)ingredient.ingerientType);
                }
            }
        }

        public bool IsIngredientCorrect(int ingredient)
        {
            if (correctIngredientIndex.Contains(ingredient))
            {
                correctIngredientIndex.Remove(ingredient);
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
            currentLevelRecipeIndex = 0;

            currentLevelRecipes.Clear();

            ConnectinnoActions.OnNextLevelStarted?.Invoke();

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
        public Vector2 GetRandomSafePosition()
        {
            var minmaxX = Random.Range(-(safeWorldWidth / 2), safeWorldWidth / 2);
            var minmaxy = Random.Range(-(safeWorldHeight / 2), safeWorldHeight / 2);
            return new Vector2(minmaxX, minmaxy);
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
